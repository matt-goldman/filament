# TASK-002: Android Binding Cleanup — Metadata.xml and Additions

**Phase:** 1 — Android Binding
**Estimated Effort:** 2–3 days
**Depends On:** TASK-001
**Relevant Skills:** `filament-android-binding`

## Objective

Fix the raw generated Android binding from TASK-001 by authoring `Transforms/Metadata.xml` transforms and `Additions/*.cs` partial classes. The goal is a clean, idiomatic C# API that handles Java-specific patterns (interface → abstract class, inner Builder classes, deprecated types, naming conflicts) so that TASK-004 can implement the cross-platform interfaces without friction.

## Prerequisites

- TASK-001 complete — `FilamentBinding.Android.dll` builds successfully
- Understanding of the binding errors/warnings reported by the TASK-001 build
- .NET MAUI Android binding documentation: `https://learn.microsoft.com/en-us/dotnet/android/binding-libs/customizing-bindings/`

## Deliverables

- `maui/FilamentBinding.Android/Transforms/Metadata.xml` — complete transforms addressing all name conflicts, interface→abstract-class conversions, and `[Obsolete]` annotations
- `maui/FilamentBinding.Android/Additions/FilamentExtensions.cs` — C#-friendly helpers (e.g., overloads accepting `byte[]` instead of `Java.Nio.ByteBuffer`)
- `maui/FilamentBinding.Android/Additions/UiHelperCallbackAdditions.cs` — clean C# pattern for `UiHelper.RendererCallback`
- Updated `dotnet build` with zero errors and minimal actionable warnings

## Detailed Steps

### Step 1: Address the UiHelper.RendererCallback Java interface

`UiHelper.RendererCallback` is a Java interface. Java interfaces with callbacks are exposed in C# bindings as abstract classes. Add to `Transforms/Metadata.xml`:

```xml
<metadata>
  <!-- UiHelper.RendererCallback: Java interface → C# abstract class -->
  <attr path="/api/package[@name='com.google.android.filament.android']/interface[@name='UiHelper.RendererCallback']"
        name="abstract">true</attr>
</metadata>
```

In `Additions/UiHelperCallbackAdditions.cs`, add a C#-friendly delegate-based adapter:

```csharp
namespace Com.Google.Android.Filament.Android;

/// <summary>
/// Delegate-based adapter for UiHelper.RendererCallback.
/// Avoids requiring callers to subclass the abstract callback class.
/// </summary>
public sealed class FilamentRendererCallback : UiHelper.RendererCallback
{
    public Action<Android.Views.Surface, long>? NativeWindowChanged { get; set; }
    public Action? DetachedFromSurface { get; set; }
    public Action<int, int>? Resized { get; set; }

    public override void OnNativeWindowChanged(Android.Views.Surface surface, long flags) =>
        NativeWindowChanged?.Invoke(surface, flags);

    public override void OnDetachedFromSurface() =>
        DetachedFromSurface?.Invoke();

    public override void OnResized(int width, int height) =>
        Resized?.Invoke(width, height);
}
```

### Step 2: Resolve namespace collision with `.android` sub-package

The `com.google.android.filament.android` package contains classes whose auto-generated C# names may collide with the parent namespace. Add renames:

```xml
  <!-- Avoid collision: android subpackage → FilamentAndroid prefix -->
  <attr path="/api/package[@name='com.google.android.filament.android']/class[@name='AndroidPlatform']"
        name="managedName">FilamentAndroidPlatform</attr>

  <!-- Ensure UiHelper lands in a usable namespace -->
  <attr path="/api/package[@name='com.google.android.filament.android']/class[@name='UiHelper']"
        name="managedName">UiHelper</attr>
```

### Step 3: Mark deprecated types as [Obsolete]

```xml
  <!-- ToneMapper is deprecated in favour of ColorGrading -->
  <attr path="/api/package[@name='com.google.android.filament']/class[@name='ToneMapper']"
        name="obsolete">true</attr>
```

### Step 4: Verify Builder inner classes resolve correctly

The `Engine.Builder`, `Texture.Builder`, `Material.Builder`, etc. should map naturally. If the binding generator produces duplicate or missing types, add entries such as:

```xml
  <!-- Ensure Engine.Builder inner class is accessible -->
  <attr path="/api/package[@name='com.google.android.filament']/class[@name='Engine']/class[@name='Builder']"
        name="managedName">Builder</attr>
```

Test that the following compiles in a scratch project:

```csharp
var texture = new Com.Google.Android.Filament.Texture.Builder()
    .Width(256)
    .Height(256)
    .Levels(1)
    .Sampler(Texture.Sampler.Sampler2d)
    .Format(Texture.InternalFormat.Rgba8)
    .Build(engine);
```

### Step 5: Add ByteBuffer convenience helpers

Filament's Java API takes `Java.Nio.ByteBuffer` for materials and buffer data. Add C#-friendly overloads in `Additions/FilamentExtensions.cs`:

```csharp
using Java.Nio;

namespace Com.Google.Android.Filament;

public static class MaterialExtensions
{
    /// <summary>Creates a Material from a raw byte array (e.g., loaded from Assets).</summary>
    public static Material? BuildFromBytes(this Material.Builder builder, Engine engine, byte[] materialData)
    {
        var buffer = ByteBuffer.Wrap(materialData);
        return builder.Payload(buffer, materialData.Length).Build(engine);
    }
}
```

### Step 6: Validate Kotlin stdlib handling

`filament-utils-android` bundles the Kotlin standard library. If your MAUI app already includes Kotlin transitively, add an exclusion to avoid binary size increase:

```xml
<ItemGroup>
  <AndroidSkipResourceProcessing Include="Jars\filament-utils-android-1.69.5.aar"
                                  Resources="true" />
</ItemGroup>
```

Or consult the MAUI build output for `Duplicate entry` warnings related to `kotlin-stdlib` and address as needed.

### Step 7: Full rebuild and API review

```bash
cd maui/FilamentBinding.Android
dotnet build -c Debug 2>&1 | grep -E "error|warning" | head -50
```

Review all remaining warnings and decide which require Metadata.xml entries versus which can be suppressed.

## Acceptance Criteria

- [ ] `dotnet build` completes with **zero errors**
- [ ] `UiHelper.RendererCallback` is accessible as an abstract class or via `FilamentRendererCallback` delegate adapter
- [ ] No namespace collision errors between `com.google.android.filament` and `com.google.android.filament.android`
- [ ] `Material.Builder`, `Texture.Builder`, `Engine.Builder` all compile and function correctly
- [ ] `MaterialExtensions.BuildFromBytes()` helper exists and compiles
- [ ] `ToneMapper` is annotated `[Obsolete]`
- [ ] `Com.Google.Android.Filament.Filament.Init()` compiles and links

## Reference

- See `.github/skills/filament-android-binding/SKILL.md` — "Key Metadata Transforms" and "Important Patterns" sections
- See `docs/maui-binding-notes.md` — "Android Binding" critical notes
- Android binding customization: `https://learn.microsoft.com/en-us/dotnet/android/binding-libs/customizing-bindings/java-bindings-metadata`
- Filament Android Java source: `android/filament-android/src/main/java/com/google/android/filament/`
