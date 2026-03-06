---
name: filament-maui-api-surface
description: >
  Defines the minimum viable cross-platform .NET MAUI API surface for Filament.
  Covers the 13 core classes that map 1:1 between Android (Java) and iOS (C++),
  recommended C# interface definitions, multi-targeted project structure, platform
  folder layout, and TFM configuration for net10.0-android and net10.0-ios.
  USE FOR: "filament maui api", "filament cross-platform", "IFilamentEngine",
  "filament interfaces", "filament class library", "filament net10", "filament TFM",
  "filament multi-target".
  DO NOT USE FOR: Android-specific binding (use filament-android-binding), iOS-specific
  binding (use filament-ios-binding), surface/window integration (use filament-surface-integration).
---

# Filament Cross-Platform .NET MAUI API Surface

## Minimum Viable Class Set

These 13 core classes exist in both the Android Java API and the iOS C++ API
and form the foundation of a cross-platform .NET MAUI Filament library.

| Concept | Android (Java) | iOS (C++) | C# Interface |
|---|---|---|---|
| Engine | `Engine` | `filament::Engine` | `IFilamentEngine` |
| Renderer | `Renderer` | `filament::Renderer` | `IFilamentRenderer` |
| View | `View` | `filament::View` | `IFilamentView` |
| Scene | `Scene` | `filament::Scene` | `IFilamentScene` |
| Camera | `Camera` | `filament::Camera` | `IFilamentCamera` |
| SwapChain | `SwapChain` | `filament::SwapChain` | `IFilamentSwapChain` |
| Material | `Material` | `filament::Material` | `IFilamentMaterial` |
| MaterialInstance | `MaterialInstance` | `filament::MaterialInstance` | `IFilamentMaterialInstance` |
| Texture | `Texture` | `filament::Texture` | `IFilamentTexture` |
| Entity | `int entity` | `utils::Entity` (uint32) | `int` or `FilamentEntity` struct |
| EntityManager | `EntityManager` | `utils::EntityManager` | `IFilamentEntityManager` |
| TransformManager | `TransformManager` | `filament::TransformManager` | `IFilamentTransformManager` |
| RenderableManager | `RenderableManager` | `filament::RenderableManager` | `IFilamentRenderableManager` |

---

## Recommended C# Interface Definitions

```csharp
namespace Filament.Maui;

public interface IFilamentEngine : IDisposable
{
    IFilamentRenderer CreateRenderer();
    IFilamentScene CreateScene();
    IFilamentView CreateView();
    IFilamentCamera CreateCamera();
    IFilamentSwapChain CreateSwapChain(object nativeSurface);
    IFilamentTransformManager TransformManager { get; }
    IFilamentRenderableManager RenderableManager { get; }
    IFilamentEntityManager EntityManager { get; }
    void DestroyRenderer(IFilamentRenderer renderer);
    void DestroyScene(IFilamentScene scene);
    void DestroyView(IFilamentView view);
    void DestroyCamera(IFilamentCamera camera);
    void DestroySwapChain(IFilamentSwapChain swapChain);
    void FlushAndWait();
}

public interface IFilamentRenderer
{
    bool BeginFrame(IFilamentSwapChain swapChain);
    void Render(IFilamentView view);
    void EndFrame();
}

public interface IFilamentView
{
    void SetScene(IFilamentScene scene);
    void SetCamera(IFilamentCamera camera);
    void SetViewport(int left, int bottom, int width, int height);
    void SetClearColor(float r, float g, float b, float a);
    void SetPostProcessingEnabled(bool enabled);
}

public interface IFilamentScene
{
    void AddEntity(int entity);
    void RemoveEntity(int entity);
    void SetSkybox(IFilamentSkybox? skybox);
    void SetIndirectLight(IFilamentIndirectLight? ibl);
}

public interface IFilamentCamera
{
    void SetProjection(double fovDegrees, double aspect, double near, double far);
    void LookAt(double eyeX, double eyeY, double eyeZ,
                double centerX, double centerY, double centerZ,
                double upX, double upY, double upZ);
}

public interface IFilamentEntityManager
{
    int Create();
    void Destroy(int entity);
}
```

---

## Multi-Targeted Project File

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

  <!-- Android: reference the AAR binding project -->
  <ItemGroup Condition="$(TargetFramework.StartsWith('net10.0-android'))">
    <ProjectReference Include="..\FilamentBinding.Android\FilamentBinding.Android.csproj" />
  </ItemGroup>

  <!-- iOS: reference the XCFramework binding project -->
  <ItemGroup Condition="$(TargetFramework.StartsWith('net10.0-ios'))">
    <ProjectReference Include="..\FilamentBinding.iOS\FilamentBinding.iOS.csproj" />
  </ItemGroup>
</Project>
```

---

## Recommended Project & Folder Structure

```
maui/
├── FilamentBinding.Android/         # Android AAR binding library
│   ├── FilamentBinding.Android.csproj  (net10.0-android)
│   ├── Jars/
│   │   ├── filament-android-1.69.5.aar
│   │   └── gltfio-android-1.69.5.aar    (optional)
│   ├── Transforms/
│   │   └── Metadata.xml
│   └── Additions/
│       └── FilamentExtensions.cs
│
├── FilamentWrapper.iOS/             # Objective-C++ wrapper (must be built separately)
│   ├── FilamentWrapper.xcodeproj
│   └── *.h / *.mm                   # FLTEngine, FLTRenderer, FLTView, etc.
│
├── FilamentBinding.iOS/             # .NET MAUI iOS binding library
│   ├── FilamentBinding.iOS.csproj   (net10.0-ios)
│   ├── ApiDefinitions.cs
│   ├── StructsAndEnums.cs
│   └── Native/
│       └── FilamentWrapper.xcframework
│
└── Filament.Maui/                   # Cross-platform class library
    ├── Filament.Maui.csproj         (net10.0-android;net10.0-ios)
    ├── Interfaces/
    │   ├── IFilamentEngine.cs
    │   ├── IFilamentRenderer.cs
    │   ├── IFilamentView.cs
    │   ├── IFilamentScene.cs
    │   ├── IFilamentCamera.cs
    │   └── ...
    ├── FilamentView.cs              # MAUI View control (cross-platform entry point)
    └── Platforms/
        ├── Android/
        │   ├── FilamentEngineAndroid.cs   # IFilamentEngine → Java Engine
        │   ├── FilamentViewHandler.cs     # UiHelper + SurfaceView
        │   └── ...
        └── iOS/
            ├── FilamentEngineiOS.cs       # IFilamentEngine → FLTEngine
            ├── FilamentViewHandler.cs     # CAMetalLayer + CADisplayLink
            └── ...
```

---

## Material Pipeline Note

Filament materials must be **precompiled per backend** and shipped as binary blobs.
Use the `matc` tool from the Filament release package to compile `.mat` files:

```bash
# Compile for Android (OpenGL + Vulkan)
matc -p mobile -a opengl -a vulkan -o default.mat.android default.matc

# Compile for iOS (Metal)
matc -p mobile -a metal -o default.mat.ios default.matc
```

Load platform-appropriate materials in the MAUI app:
```csharp
#if ANDROID
    using var stream = await FileSystem.OpenAppPackageFileAsync("materials/default.mat.android");
#elif IOS
    using var stream = await FileSystem.OpenAppPackageFileAsync("materials/default.mat.ios");
#endif
```
