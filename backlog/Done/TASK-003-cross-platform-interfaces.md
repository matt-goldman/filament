# TASK-003: Cross-Platform Interface Definitions

**Phase:** 2 — Cross-Platform Core
**Estimated Effort:** 2–3 days
**Depends On:** None (can run in parallel with TASK-001 and TASK-002)
**Relevant Skills:** `filament-maui-api-surface`, `filament-maui-project-structure`

## Objective

Define all C# interfaces and shared types in the `Filament.Maui` multi-targeted class library project. These interfaces form the stable API contract that Android (TASK-004) and iOS (TASK-008) implementations must satisfy, and they are what consumer applications use directly. Getting the interface design right before implementing either platform prevents rework.

## Prerequisites

- .NET 10 SDK with MAUI workload installed
- Clear understanding of both the Android Java API (`android/filament-android/src/main/java/com/google/android/filament/`) and the iOS C++ API (`filament/include/filament/`) to ensure the interfaces are achievable on both platforms

## Deliverables

- `maui/Filament.Maui/Filament.Maui.csproj` — multi-targeted project file (`net10.0-android;net10.0-ios`)
- `maui/Filament.Maui/Interfaces/IFilamentEngine.cs`
- `maui/Filament.Maui/Interfaces/IFilamentRenderer.cs`
- `maui/Filament.Maui/Interfaces/IFilamentView.cs`
- `maui/Filament.Maui/Interfaces/IFilamentScene.cs`
- `maui/Filament.Maui/Interfaces/IFilamentCamera.cs`
- `maui/Filament.Maui/Interfaces/IFilamentSwapChain.cs`
- `maui/Filament.Maui/Interfaces/IFilamentMaterial.cs`
- `maui/Filament.Maui/Interfaces/IFilamentMaterialInstance.cs`
- `maui/Filament.Maui/Interfaces/IFilamentTexture.cs`
- `maui/Filament.Maui/Interfaces/IFilamentEntityManager.cs`
- `maui/Filament.Maui/Interfaces/IFilamentTransformManager.cs`
- `maui/Filament.Maui/Interfaces/IFilamentRenderableManager.cs`
- `maui/Filament.Maui/FilamentView.cs` — the public MAUI `View` control (cross-platform entry point)
- `maui/Filament.Maui/FilamentRenderThread.cs` — shared render thread abstraction (optional but recommended)
- Project builds successfully for both TFMs with no platform implementations yet

## Detailed Steps

### Step 1: Create the project file

`maui/Filament.Maui/Filament.Maui.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0-android;net10.0-ios</TargetFrameworks>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <RootNamespace>Filament.Maui</RootNamespace>
  </PropertyGroup>

  <!-- Android: reference the AAR binding (added after TASK-001/002 are complete) -->
  <ItemGroup Condition="$(TargetFramework.StartsWith('net10.0-android'))">
    <ProjectReference Include="..\FilamentBinding.Android\FilamentBinding.Android.csproj" />
  </ItemGroup>

  <!-- iOS: reference the XCFramework binding (added after TASK-007 is complete) -->
  <ItemGroup Condition="$(TargetFramework.StartsWith('net10.0-ios'))">
    <!-- <ProjectReference Include="..\FilamentBinding.iOS\FilamentBinding.iOS.csproj" /> -->
  </ItemGroup>
</Project>
```

Create platform folder structure:
```bash
mkdir -p maui/Filament.Maui/Interfaces
mkdir -p maui/Filament.Maui/Platforms/Android
mkdir -p maui/Filament.Maui/Platforms/iOS
```

### Step 2: Define the core interfaces

`maui/Filament.Maui/Interfaces/IFilamentEngine.cs`:

```csharp
namespace Filament.Maui;

/// <summary>
/// Cross-platform Filament engine — creates and destroys all GPU resources.
/// Entity is an int on both platforms (Android: int, iOS: uint32_t wrapped as int).
/// </summary>
public interface IFilamentEngine : IDisposable
{
    IFilamentRenderer CreateRenderer();
    IFilamentScene CreateScene();
    IFilamentView CreateView();
    IFilamentCamera CreateCamera();
    /// <param name="nativeSurface">
    /// Android: android.view.Surface object. iOS: handle to CAMetalLayer (via ObjCRuntime.Runtime).
    /// </param>
    IFilamentSwapChain CreateSwapChain(object nativeSurface);
    IFilamentTransformManager TransformManager { get; }
    IFilamentRenderableManager RenderableManager { get; }
    IFilamentEntityManager EntityManager { get; }
    void DestroyRenderer(IFilamentRenderer renderer);
    void DestroyScene(IFilamentScene scene);
    void DestroyView(IFilamentView view);
    void DestroyCamera(IFilamentCamera camera);
    void DestroySwapChain(IFilamentSwapChain swapChain);
    /// <summary>Blocks until all pending GPU work is complete. Call before resizing or destroying resources.</summary>
    void FlushAndWait();
}
```

`maui/Filament.Maui/Interfaces/IFilamentRenderer.cs`:

```csharp
namespace Filament.Maui;

public interface IFilamentRenderer : IDisposable
{
    /// <returns>True if the frame should be rendered (surface is valid).</returns>
    bool BeginFrame(IFilamentSwapChain swapChain);
    void Render(IFilamentView view);
    void EndFrame();
}
```

`maui/Filament.Maui/Interfaces/IFilamentView.cs`:

```csharp
namespace Filament.Maui;

public interface IFilamentView : IDisposable
{
    void SetScene(IFilamentScene scene);
    void SetCamera(IFilamentCamera camera);
    void SetViewport(int left, int bottom, int width, int height);
    void SetClearColor(float r, float g, float b, float a);
    void SetPostProcessingEnabled(bool enabled);
}
```

`maui/Filament.Maui/Interfaces/IFilamentScene.cs`:

```csharp
namespace Filament.Maui;

public interface IFilamentScene : IDisposable
{
    /// <param name="entity">Entity ID — an int (not a class) on both platforms.</param>
    void AddEntity(int entity);
    void RemoveEntity(int entity);
    void SetSkybox(IFilamentSkybox? skybox);
    void SetIndirectLight(IFilamentIndirectLight? ibl);
}
```

`maui/Filament.Maui/Interfaces/IFilamentCamera.cs`:

```csharp
namespace Filament.Maui;

public interface IFilamentCamera : IDisposable
{
    void SetProjection(double fovDegrees, double aspect, double near, double far);
    void LookAt(double eyeX, double eyeY, double eyeZ,
                double centerX, double centerY, double centerZ,
                double upX, double upY, double upZ);
}
```

`maui/Filament.Maui/Interfaces/IFilamentSwapChain.cs`:

```csharp
namespace Filament.Maui;

/// <summary>Opaque handle to a platform native window surface.</summary>
public interface IFilamentSwapChain : IDisposable { }
```

`maui/Filament.Maui/Interfaces/IFilamentMaterial.cs`:

```csharp
namespace Filament.Maui;

public interface IFilamentMaterial : IDisposable
{
    IFilamentMaterialInstance CreateInstance();
}
```

`maui/Filament.Maui/Interfaces/IFilamentMaterialInstance.cs`:

```csharp
namespace Filament.Maui;

public interface IFilamentMaterialInstance : IDisposable
{
    void SetParameterFloat(string name, float value);
    void SetParameterFloat4(string name, float x, float y, float z, float w);
    void SetParameterTexture(string name, IFilamentTexture texture);
}
```

`maui/Filament.Maui/Interfaces/IFilamentTexture.cs`:

```csharp
namespace Filament.Maui;

public interface IFilamentTexture : IDisposable { }
```

`maui/Filament.Maui/Interfaces/IFilamentEntityManager.cs`:

```csharp
namespace Filament.Maui;

/// <summary>
/// Creates and destroys entity IDs. Entity is an int (32-bit) on both platforms —
/// it is NOT a class with a native pointer.
/// </summary>
public interface IFilamentEntityManager
{
    int Create();
    void Destroy(int entity);
}
```

`maui/Filament.Maui/Interfaces/IFilamentTransformManager.cs`:

```csharp
namespace Filament.Maui;

public interface IFilamentTransformManager
{
    void Create(int entity);
    void SetTransform(int entity, float[] mat4ColumnMajor);
}
```

`maui/Filament.Maui/Interfaces/IFilamentRenderableManager.cs`:

```csharp
namespace Filament.Maui;

public interface IFilamentRenderableManager
{
    // Builder pattern exposed via platform implementations.
    // Minimal cross-platform surface — extend as needed.
    void SetMaterialInstanceAt(int entity, int primitiveIndex, IFilamentMaterialInstance instance);
}
```

Add stub interfaces for `IFilamentSkybox` and `IFilamentIndirectLight` referenced above:

```csharp
// IFilamentSkybox.cs
namespace Filament.Maui;
public interface IFilamentSkybox : IDisposable { }

// IFilamentIndirectLight.cs
namespace Filament.Maui;
public interface IFilamentIndirectLight : IDisposable { }
```

### Step 3: Define the FilamentView MAUI control

`maui/Filament.Maui/FilamentView.cs`:

```csharp
namespace Filament.Maui;

/// <summary>
/// Cross-platform MAUI View that hosts a Filament rendering surface.
/// Platform handlers (Android SurfaceView, iOS UIView+CAMetalLayer) are
/// registered via MauiProgram.cs.
/// </summary>
public class FilamentView : Microsoft.Maui.Controls.View
{
    public static readonly BindableProperty EngineProperty =
        BindableProperty.Create(nameof(Engine), typeof(IFilamentEngine), typeof(FilamentView));

    public IFilamentEngine? Engine
    {
        get => (IFilamentEngine?)GetValue(EngineProperty);
        set => SetValue(EngineProperty, value);
    }

    public event EventHandler<FilamentFrameEventArgs>? FrameRendering;

    internal void OnFrameRendering(FilamentFrameEventArgs e) =>
        FrameRendering?.Invoke(this, e);
}

public class FilamentFrameEventArgs : EventArgs
{
    public IFilamentRenderer Renderer { get; }
    public IFilamentView View { get; }
    public FilamentFrameEventArgs(IFilamentRenderer renderer, IFilamentView view)
    {
        Renderer = renderer;
        View = view;
    }
}
```

### Step 4: Build and confirm compilation for both TFMs

```bash
cd maui/Filament.Maui
dotnet build -c Debug
```

This should compile cleanly for both `net10.0-android` and `net10.0-ios` even without platform implementations (the `Platforms/` files are added in TASK-004 and TASK-008).

## Acceptance Criteria

- [ ] `Filament.Maui.csproj` targets `net10.0-android;net10.0-ios`
- [ ] All 13 minimum viable interfaces are defined: `IFilamentEngine`, `IFilamentRenderer`, `IFilamentView`, `IFilamentScene`, `IFilamentCamera`, `IFilamentSwapChain`, `IFilamentMaterial`, `IFilamentMaterialInstance`, `IFilamentTexture`, `IFilamentEntityManager`, `IFilamentTransformManager`, `IFilamentRenderableManager`, plus `IFilamentSkybox`, `IFilamentIndirectLight`
- [ ] `Entity` is represented as `int` in all interface signatures (not a class)
- [ ] `FilamentView` MAUI control is defined with `Engine` bindable property and `FrameRendering` event
- [ ] `dotnet build` succeeds for both TFMs without errors
- [ ] No platform-specific code in the shared interface files

## Reference

- See `.github/skills/filament-maui-api-surface/SKILL.md` — full interface definitions and project file
- See `.github/skills/filament-maui-project-structure/SKILL.md` — folder layout
- See `docs/maui-binding-notes.md` — "Cross-Platform Library" section
- MAUI multi-targeting: `https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/`
