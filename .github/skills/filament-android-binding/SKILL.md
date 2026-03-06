---
name: filament-android-binding
description: >
  Guidance for creating a .NET MAUI Android binding library for Filament.
  Covers the available Java/JNI AAR artifacts on Maven Central, the 45+ public
  classes in com.google.android.filament, binding project setup, Metadata.xml
  transforms, and initialization patterns.
  USE FOR: "filament android binding", "AAR binding", "com.google.android.filament",
  "filament-android", "gltfio-android", "filamat-android", "filament-utils-android",
  "AndroidBindingLibrary", "filament android maui".
  DO NOT USE FOR: iOS binding (use filament-ios-binding), cross-platform API design
  (use filament-maui-api-surface), surface/window integration (use filament-surface-integration).
---

# Filament Android Binding for .NET MAUI

Filament ships a complete Java/JNI API for Android as a set of AARs on Maven Central.
No wrapper library needs to be authored — the AAR can be bound directly using a standard
.NET MAUI `AndroidBindingLibrary` project.

---

## Available AAR Artifacts (version 1.69.5)

| Maven Artifact | Description | Package |
|---|---|---|
| `com.google.android.filament:filament-android:1.69.5` | Core rendering engine — 45+ Java classes | `com.google.android.filament` |
| `com.google.android.filament:gltfio-android:1.69.5` | glTF 2.0 asset loader — 8 classes | `com.google.android.filament.gltfio` |
| `com.google.android.filament:filament-utils-android:1.69.5` | Camera utilities and IBL preprocessing (Kotlin) | `com.google.android.filament.utils` |
| `com.google.android.filament:filamat-android:1.69.5` | Runtime material compiler — 2 classes | `com.google.android.filament.filamat` |

**Download URL pattern:**
```
https://repo1.maven.org/maven2/com/google/android/filament/filament-android/1.69.5/filament-android-1.69.5.aar
```

---

## Core Public API Classes

The `com.google.android.filament` package contains:

| Java Class | C# Binding Name | Purpose |
|---|---|---|
| `Engine` | `FilamentEngine` | Entry point; creates/destroys all resources |
| `Renderer` | `FilamentRenderer` | Frame rendering (beginFrame/render/endFrame) |
| `View` | `FilamentView` | Viewport, scene, camera, post-processing |
| `Scene` | `FilamentScene` | Flat container of entities and lights |
| `Camera` | `FilamentCamera` | Perspective/orthographic camera |
| `SwapChain` | `FilamentSwapChain` | Native window surface abstraction |
| `Material` | `FilamentMaterial` | PBR material shader programs |
| `MaterialInstance` | `FilamentMaterialInstance` | Per-draw material parameter overrides |
| `Texture` | `FilamentTexture` | GPU texture resources |
| `RenderTarget` | `FilamentRenderTarget` | Off-screen render-to-texture |
| `EntityManager` | `FilamentEntityManager` | Creates/destroys entity IDs (singleton) |
| `TransformManager` | `FilamentTransformManager` | Entity position/rotation/scale |
| `RenderableManager` | `FilamentRenderableManager` | Attaches mesh+material components |
| `LightManager` | `FilamentLightManager` | Directional/spot/point lights |
| `IndirectLight` | `FilamentIndirectLight` | Image-based lighting (IBL) |
| `Skybox` | `FilamentSkybox` | Environment background |
| `ColorGrading` | `FilamentColorGrading` | Tone mapping and color correction |
| `UiHelper` | *(internal to FilamentViewHandler)* | SurfaceView/TextureView surface management |

**Android-specific package** (`com.google.android.filament.android`):
- `UiHelper` — manages `SurfaceView`/`TextureView`/`SurfaceHolder` and invokes `RendererCallback`
- `FilamentHelper` — `synchronizePendingFrames()` utility
- `DisplayHelper`, `TextureHelper`

---

## Binding Project Setup

### Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-android</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <LibraryProjectZip Include="Jars\filament-android-1.69.5.aar" />
    <!-- Optionally add additional AARs: gltfio, filamat, utils -->
  </ItemGroup>
</Project>
```

### Dependency Order for Multiple AARs

```
filament-android  ←  gltfio-android  ←  filament-utils-android
filament-android (standalone) ←  filamat-android
```

When binding multiple AARs, add them in dependency order in the project file.

---

## Key Metadata Transforms (Transforms/Metadata.xml)

Common transforms needed:

```xml
<metadata>
  <!-- Avoid namespace collision with the .android subpackage classes -->
  <attr path="/api/package[@name='com.google.android.filament.android']/class[@name='AndroidPlatform']"
        name="managedName">FilamentAndroidPlatform</attr>

  <!-- UiHelper.RendererCallback is a Java interface — must be abstract class in C# -->
  <attr path="/api/package[@name='com.google.android.filament.android']/interface[@name='UiHelper.RendererCallback']"
        name="abstract">true</attr>

  <!-- Mark deprecated ToneMapper as Obsolete -->
  <attr path="/api/package[@name='com.google.android.filament']/class[@name='ToneMapper']"
        name="obsolete">true</attr>
</metadata>
```

---

## Initialization

The native JNI library must be loaded before any Filament call:

```csharp
// Load libfilament-jni.so — call once at app startup
Com.Google.Android.Filament.Filament.Init();

// For gltfio support:
Com.Google.Android.Filament.Gltfio.Gltfio.Init();
```

---

## JNI ABI Support

The AAR includes native `.so` libraries for:
- `arm64-v8a` (primary — modern 64-bit Android)
- `armeabi-v7a` (legacy 32-bit ARM)
- `x86_64` (emulator)
- `x86` (legacy emulator)

---

## Important Patterns

1. **Entity is an `int`**, not a class — `int entity = EntityManager.Get().Create()`.
   In C# it should remain an `int` or thin `struct`, not a native-pointer object.

2. **Builder pattern** — most resource classes use inner `Builder` objects (e.g.,
   `Engine.Builder`, `Texture.Builder`). These map naturally to C# binding nested classes.

3. **`UiHelper.RendererCallback` is a Java interface** — must be handled as an
   abstract class in the C# binding (use `Transforms/Metadata.xml` or `Additions/*.cs`).

4. **Kotlin stdlib** — `filament-utils-android` includes the Kotlin standard library.
   Exclude it if it is already transitively included in your app to avoid binary size increase.

5. **Thread safety** — `Engine` is NOT thread-safe. All Filament calls must be made
   from the same dedicated render thread. `FlushAndWait()` is needed to synchronize.

6. **Materials are byte arrays** — Load `.mat` files into a `ByteBuffer`; there is no
   file-path API. Use `Android.Content.Res.AssetManager` to load from `assets/`.
