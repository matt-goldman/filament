# .NET MAUI Native Interop Binding Assessment for Filament

**Repository:** [google/filament](https://github.com/google/filament)  
**Library Version:** 1.69.5  
**Assessment Date:** 2026-03-06  
**Purpose:** Evaluate the feasibility of creating .NET MAUI native interop binding libraries and a cross-platform .NET MAUI class library that exposes a unified rendering API.

---

## Executive Summary

Creating .NET MAUI bindings for Filament is **feasible**, though the effort differs significantly between platforms. The Android binding is relatively straightforward because Filament already ships a complete Java/JNI API (45+ classes) as an AAR on Maven Central. The iOS binding is substantially more complex because Filament exposes only a C++ API on iOS — there are no Objective-C or Swift wrappers — and .NET MAUI binding projects require an Objective-C or Swift interface. A custom Objective-C++ wrapper library must be authored before the binding project can be created.

| Platform | Bindability | Effort | Key Blocker |
|----------|-------------|--------|-------------|
| Android  | ✅ Straightforward | Low–Medium | None; AAR binding is well-understood in .NET MAUI |
| iOS      | ⚠️ Requires wrapper | High | No Objective-C/Swift layer exists; must be authored |
| Cross-platform | ✅ Achievable | Medium | Platform surface integration (SwapChain) differs |

---

## 1. Android Binding

### 1.1 What Exists

Filament ships official Android support as a set of AARs published to Maven Central under the group `com.google.android.filament`. All libraries are at version **1.69.5**.

| AAR Artifact | Description |
|---|---|
| `filament-android` | Core rendering engine (45+ Java classes) |
| `gltfio-android` | glTF 2.0 asset loader (8 classes) |
| `filament-utils-android` | Camera utilities, IBL preprocessing (Kotlin) |
| `filamat-android` | Runtime material compiler (2 classes) |

The main package `com.google.android.filament` contains all core rendering classes backed by JNI to native C++ via `libfilament-jni.so`.

### 1.2 Key Public API (Core Classes)

| Java Class | Purpose |
|---|---|
| `Engine` | Entry point; creates/destroys all resources |
| `Renderer` | Executes frame rendering (beginFrame/render/endFrame) |
| `View` | Viewport, scene, camera, and post-processing options |
| `Scene` | Flat container of renderable entities and lights |
| `Camera` | Perspective/orthographic camera |
| `SwapChain` | Native window surface abstraction |
| `RenderTarget` | Off-screen render-to-texture target |
| `Texture` | GPU texture resources |
| `Material` / `MaterialInstance` | PBR material definitions and instances |
| `VertexBuffer` / `IndexBuffer` | Geometry GPU buffers |
| `EntityManager` | Creates/destroys entity IDs |
| `TransformManager` | Sets entity transforms |
| `RenderableManager` | Attaches renderable components |
| `LightManager` | Attaches directional/spot/point lights |
| `IndirectLight` | Image-based lighting (IBL) |
| `Skybox` | Environment background |
| `ColorGrading` | Tone mapping and color correction |
| `UiHelper` (android package) | SurfaceView/TextureView surface management |

The full list of packages:

```
com.google.android.filament          // 30+ core rendering classes
com.google.android.filament.android  // Android UI helpers (UiHelper, etc.)
com.google.android.filament.gltfio   // glTF 2.0 support
com.google.android.filament.filamat  // Material compilation
com.google.android.filament.utils    // Camera manipulation, IBL
```

### 1.3 Binding Approach

A .NET MAUI Android binding library wraps the existing AAR with minimal extra work:

1. Create a `.csproj` of type `AndroidBindingLibrary`.
2. Add the `filament-android-1.69.5.aar` (and optionally `gltfio-android`, `filamat-android`, `filament-utils-android`) as `@(LibraryProjectZip)` items.
3. Add `Transforms/Metadata.xml` to resolve any naming conflicts or obfuscation.
4. Optionally add `Additions/*.cs` to add C#-friendly extension methods or partial classes.

The NuGet package for the AAR can reference the upstream Maven Central artifact directly rather than bundling the binary.

### 1.4 Android Build & Architecture Notes

- The JNI `.so` libraries inside the AAR support: `arm64-v8a`, `armeabi-v7a`, `x86_64`, `x86`.
- The library initializes via `Filament.init()` (loads `libfilament-jni.so`).
- Surface integration uses `UiHelper` which wraps `SurfaceView`/`TextureView`/`SurfaceHolder`.
- Rendering must happen on a dedicated render thread; `Engine` is not thread-safe.
- The entity-component system (ECS) must be respected: entities are 32-bit integer IDs, components are attached via managers.

---

## 2. iOS Binding

### 2.1 What Exists

Filament ships prebuilt iOS static libraries (`.a`) distributed via CocoaPods (`pod 'Filament', '~> 1.69.5'`). The public API is **C++ only** — there are no Objective-C or Swift wrappers in the repository.

The CocoaPods spec bundles the following subspecs:

| Subspec | Libraries | Purpose |
|---|---|---|
| `filament` | libfilament.a, libbackend.a, libfilabridge.a, libfilaflat.a, libibl.a, libgeometry.a | Core rendering |
| `filamat` | libfilamat.a, libshaders.a, libsmol-v.a | Material compiler |
| `gltfio_core` | libgltfio_core.a, libdracodec.a, libuberarchive.a, libstb.a | glTF loading |
| `camutils` | libcamutils.a | Camera manipulation |
| `utils` | libutils.a | Entity/EntityManager/JobSystem |
| `image` | libimage.a | Image processing |
| `ktxreader` | libktxreader.a, libbasis_transcoder.a | KTX texture support |
| `viewer` | libviewer.a, libcivetweb.a | Viewer framework |
| `uberz` | libuberzlib.a, libzstd.a | Compression |
| `math` | (header-only) | Math types: float3, mat4f, quaternion |
| `tsl` | (header-only) | Type-safe layer |

- **Minimum iOS deployment target:** 11.0 (CocoaPods spec); samples typically target 12.1+.
- **Recommended backend:** Metal (iOS 11+); OpenGL ES 3.0 is legacy/deprecated.

### 2.2 The C++ Problem

**This is the critical blocker for iOS.** .NET MAUI cannot directly bind a C++ library. The binding infrastructure expects Objective-C or Swift interfaces that map to C# classes. To create an iOS binding, one of the following wrapper strategies must be chosen first:

#### Option A: Objective-C++ Wrapper Framework (Recommended)

Create a new `.xcframework` or `.framework` project that:
- Uses `.mm` (Objective-C++) source files to wrap the C++ Filament API.
- Exposes an Objective-C interface for each class that needs to be bound.
- Compiles against the Filament static libraries.
- Is then bound via a standard .NET MAUI iOS binding project (type `NativeReference`).

This is the approach used in Apple's own Metal/SceneKit wrappers and is well-documented in the .NET MAUI community. The iOS samples in the repository already demonstrate the Objective-C++ bridging pattern (`.mm` files calling C++ directly).

**Estimated wrapper scope:** 20–30 Objective-C classes wrapping the core C++ API.

#### Option B: C Shim Library

Create a pure-C wrapper (`extern "C"` functions) for the core API, compile it as a static library, then use `[DllImport]` P/Invoke from C#. This is lighter to author but requires manual marshalling of every type and is error-prone with complex types (builders, enums, callbacks).

#### Option C: Swift Wrapper (Not Recommended Currently)

Swift 5.9 introduced limited C++ interoperability, but it is not production-ready for a complex API like Filament. Objective-C++ is the mature, proven path.

### 2.3 Recommended iOS Wrapper Class Mapping

The following C++ classes should be wrapped in Objective-C++ for the minimum viable binding:

| C++ Class | Objective-C Class | Notes |
|---|---|---|
| `filament::Engine` | `FLTEngine` | Lifecycle management |
| `filament::Renderer` | `FLTRenderer` | Frame rendering |
| `filament::View` | `FLTView` | Viewport/options |
| `filament::Scene` | `FLTScene` | Entity container |
| `filament::Camera` | `FLTCamera` | Projection matrix |
| `filament::SwapChain` | `FLTSwapChain` | Metal/GL surface |
| `filament::Material` | `FLTMaterial` | Shader programs |
| `filament::MaterialInstance` | `FLTMaterialInstance` | Material parameters |
| `filament::Texture` | `FLTTexture` | GPU textures |
| `filament::RenderTarget` | `FLTRenderTarget` | Off-screen rendering |
| `utils::EntityManager` | `FLTEntityManager` | Entity lifecycle |
| `filament::TransformManager` | `FLTTransformManager` | Transforms |
| `filament::RenderableManager` | `FLTRenderableManager` | Mesh rendering |
| `filament::LightManager` | `FLTLightManager` | Lighting |
| `filament::IndirectLight` | `FLTIndirectLight` | IBL |
| `filament::Skybox` | `FLTSkybox` | Background |

### 2.4 iOS Binding Project Structure

```
FilamentBinding.iOS/
├── FilamentWrapper/            # Objective-C++ wrapper framework (must be authored)
│   ├── FLTEngine.h/.mm
│   ├── FLTRenderer.h/.mm
│   ├── FLTView.h/.mm
│   ├── ...
│   ├── Filament.xcframework    # Or link against CocoaPods
│   └── FilamentWrapper.xcodeproj
├── FilamentBinding.iOS.csproj  # .NET MAUI iOS Binding Library
├── ApiDefinitions.cs           # [BaseType], [Export], [Static] bindings
└── StructsAndEnums.cs          # C# enum/struct definitions
```

---

## 3. Cross-Platform .NET MAUI Class Library

### 3.1 Unified API Design

The cross-platform library should expose a clean .NET API that delegates to the platform-specific bindings through .NET MAUI multi-targeting (platform folders).

**Recommended project structure:**

```
Filament.Maui/                      # Cross-platform class library
├── Filament.Maui.csproj            # Multi-targeted project file
├── Engine.cs                       # Platform-agnostic interfaces/base classes
├── Renderer.cs
├── Scene.cs
├── View.cs
├── Camera.cs
├── Material.cs
├── MaterialInstance.cs
├── Texture.cs
├── Entity.cs
├── FilamentView.cs                 # MAUI control hosting native surface
├── Platforms/
│   ├── Android/
│   │   ├── Engine.cs               # Android implementation using binding
│   │   ├── FilamentView.cs         # SurfaceView/TextureView integration
│   │   └── ...
│   └── iOS/
│       ├── Engine.cs               # iOS implementation using binding
│       ├── FilamentView.cs         # CAMetalLayer integration
│       └── ...
└── nuget/
    └── Filament.Maui.nuspec
```

### 3.2 Minimum Viable API Surface

The following C# interfaces represent the minimum cross-platform API:

```csharp
namespace Filament.Maui
{
    public interface IFilamentEngine : IDisposable
    {
        IRenderer CreateRenderer();
        IScene CreateScene();
        IView CreateView();
        ICamera CreateCamera();
        ISwapChain CreateSwapChain(object nativeSurface);
        ITransformManager TransformManager { get; }
        IRenderableManager RenderableManager { get; }
        IEntityManager EntityManager { get; }
        ILightManager LightManager { get; }
        void DestroyRenderer(IRenderer renderer);
        void DestroyScene(IScene scene);
        void DestroyView(IView view);
        void DestroyCamera(ICamera camera);
        void DestroySwapChain(ISwapChain swapChain);
        void FlushAndWait();
    }

    public interface IRenderer
    {
        bool BeginFrame(ISwapChain swapChain);
        void Render(IView view);
        void EndFrame();
    }

    public interface IView
    {
        void SetScene(IScene scene);
        void SetCamera(ICamera camera);
        void SetViewport(int left, int bottom, int width, int height);
        void SetClearColor(float r, float g, float b, float a);
    }

    public interface IScene
    {
        void AddEntity(int entity);
        void RemoveEntity(int entity);
        void SetSkybox(ISkybox skybox);
        void SetIndirectLight(IIndirectLight indirectLight);
    }

    public interface ICamera
    {
        void SetProjection(double fovDegrees, double aspect, double near, double far);
        void LookAt(double eyeX, double eyeY, double eyeZ,
                    double centerX, double centerY, double centerZ,
                    double upX, double upY, double upZ);
    }
}
```

### 3.3 Platform-Specific Surface Integration

The single most complex cross-platform concern is surface management. Each platform requires different handling:

| Concern | Android | iOS |
|---|---|---|
| Native surface type | `Surface` (from SurfaceView/TextureView) | `CAMetalLayer` |
| MAUI control | `SurfaceView` handler or `UiHelper` | `MTKView` or plain `UIView` |
| SwapChain creation | `engine.createSwapChain(surface)` | `engine->createSwapChain((__bridge void*)layer)` |
| Render loop | Background `HandlerThread` | `CADisplayLink` or `MTKViewDelegate` |
| Resize handling | `onResized()` callback | `MTKViewDelegate.drawableSizeWillChange` |

The cross-platform library should include a `FilamentView` (MAUI `View` subclass or `ContentView`) with platform-specific handlers that abstract this complexity.

### 3.4 Multi-Targeting Project File Example

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net9.0-android;net9.0-ios</TargetFrameworks>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <RootNamespace>Filament.Maui</RootNamespace>
  </PropertyGroup>

  <!-- Android binding reference -->
  <ItemGroup Condition="$(TargetFramework.StartsWith('net9.0-android'))">
    <ProjectReference Include="..\FilamentBinding.Android\FilamentBinding.Android.csproj" />
  </ItemGroup>

  <!-- iOS binding reference -->
  <ItemGroup Condition="$(TargetFramework.StartsWith('net9.0-ios'))">
    <ProjectReference Include="..\FilamentBinding.iOS\FilamentBinding.iOS.csproj" />
  </ItemGroup>
</Project>
```

---

## 4. Project Structure Recommendation

```
maui/
├── FilamentBinding.Android/         # Android AAR binding library
│   ├── FilamentBinding.Android.csproj
│   ├── Jars/
│   │   ├── filament-android-1.69.5.aar
│   │   └── gltfio-android-1.69.5.aar  (optional)
│   ├── Transforms/
│   │   └── Metadata.xml
│   └── Additions/
│       └── FilamentExtensions.cs
│
├── FilamentWrapper.iOS/             # Objective-C++ wrapper (must be built)
│   ├── FilamentWrapper.xcodeproj
│   ├── FLTEngine.h/.mm
│   ├── FLTRenderer.h/.mm
│   ├── FLTView.h/.mm
│   ├── FLTScene.h/.mm
│   ├── FLTCamera.h/.mm
│   ├── FLTSwapChain.h/.mm
│   ├── FLTMaterial.h/.mm
│   ├── FLTMaterialInstance.h/.mm
│   ├── FLTTexture.h/.mm
│   └── ...
│
├── FilamentBinding.iOS/             # .NET MAUI iOS binding library
│   ├── FilamentBinding.iOS.csproj
│   ├── ApiDefinitions.cs
│   ├── StructsAndEnums.cs
│   └── Native/
│       └── FilamentWrapper.xcframework
│
└── Filament.Maui/                   # Cross-platform .NET MAUI class library
    ├── Filament.Maui.csproj
    ├── Interfaces/
    │   ├── IFilamentEngine.cs
    │   ├── IRenderer.cs
    │   ├── IView.cs
    │   ├── IScene.cs
    │   ├── ICamera.cs
    │   └── ...
    ├── FilamentView.cs              # MAUI control (cross-platform)
    └── Platforms/
        ├── Android/
        │   ├── FilamentEngineAndroid.cs
        │   ├── FilamentViewAndroid.cs
        │   └── ...
        └── iOS/
            ├── FilamentEngineiOS.cs
            ├── FilamentViewiOS.cs
            └── ...
```

---

## 5. Feasibility Assessment Matrix

| Factor | Android | iOS | Cross-Platform |
|---|---|---|---|
| **Native binding available** | ✅ Full Java API (AAR) | ❌ C++ only | N/A |
| **Binding project type** | AndroidBindingLibrary | NativeReference + ObjC binding | Multi-target class library |
| **Wrapper required** | None | Objective-C++ wrapper (~20 classes) | Interface + platform impl |
| **Rendering backend** | OpenGL ES, Vulkan | Metal (recommended), OpenGL ES | Metal/Vulkan per platform |
| **Minimum OS** | Android API 21+ (recommended) | iOS 11.0+ | Follows platform minimums |
| **Surface integration** | UiHelper/SurfaceView | CAMetalLayer | FilamentView control |
| **Key complexity** | AAR metadata cleanup | Authoring ObjC++ wrapper | SwapChain abstraction |
| **Estimated effort** | 1–2 weeks | 4–6 weeks (incl. wrapper) | 2–3 weeks |

---

## 6. Risks and Mitigations

### Risk 1: Objective-C++ Wrapper Maintenance
- **Risk:** The iOS wrapper must be manually kept in sync with Filament C++ API changes across versions.
- **Mitigation:** Limit the wrapper to the minimum viable API surface. Pin to a specific Filament version and update intentionally.

### Risk 2: Binary Distribution of Native Libraries
- **Risk:** The Filament static libraries are large (~100MB+ for all iOS variants). Bundling in a NuGet package may hit size limits.
- **Mitigation:** Use `XCFramework` with only the needed subspecs. Consider distributing libraries separately via a download script similar to how CocoaPods/Maven Central work.

### Risk 3: ABI Compatibility
- **Risk:** Filament JNI and C++ ABIs may change between versions, breaking bindings.
- **Mitigation:** Pin NuGet package to a specific Filament version with clear version mapping documentation.

### Risk 4: Thread Safety and Render Loop
- **Risk:** Filament's `Engine` is not thread-safe. MAUI's UI thread differs from a rendering thread.
- **Mitigation:** The cross-platform library must expose clear threading primitives and document that all Filament calls must happen on the render thread. The `FilamentView` control should manage this internally.

### Risk 5: iOS Simulator Support
- **Risk:** Filament iOS libraries exclude ARM64 simulator (`EXCLUDED_ARCHS[sdk=iphonesimulator*] = arm64`). This may limit development on Apple Silicon Macs.
- **Mitigation:** Use x86_64 simulator targets or cloud build agents for iOS. Filament does support x86_64 simulator.

### Risk 6: Material Pipeline Complexity
- **Risk:** Filament materials are precompiled binaries that differ between Android (OpenGL/Vulkan) and iOS (Metal). Loading the same `.mat` file on both platforms requires shipping multiple compiled material variants.
- **Mitigation:** Ship pre-compiled material bundles per platform. Use the `matc` tool from Filament releases to compile materials during build time.

---

## 7. Recommendations

### Short-term (MVP)

1. **Start with Android binding** — lowest effort, highest payoff, no wrapper required.
2. **Create a minimal cross-platform interface** with Android implementation only, stubbing iOS.
3. **Author the iOS Objective-C++ wrapper** for the 13 core classes as a separate deliverable.

### Medium-term

4. **Build iOS binding** once the Objective-C++ wrapper is complete and tested.
5. **Implement `FilamentView` MAUI control** for both platforms.
6. **Publish NuGet packages**: `Filament.Maui.Binding.Android`, `Filament.Maui.Binding.iOS`, `Filament.Maui`.

### Long-term

7. **Add glTF loading support** via `gltfio-android` (Android) and a gltfio Objective-C++ wrapper (iOS).
8. **Add camera utilities** (`Manipulator`) for out-of-the-box camera controls.
9. **Material hot reload** via `filamat-android` and a filamat iOS wrapper.
10. **Contribute back** the Objective-C++ wrapper to the upstream Filament repository.

---

## 8. Related Prior Art

No existing .NET/C# or MAUI bindings for Filament were found in this repository or its known forks. Community projects to check before starting:

- Search NuGet for `filament` packages.
- Check GitHub for `filament maui` or `filament xamarin` forks.
- Review [Uno Platform](https://platform.uno/) for any existing 3D rendering integration patterns useful for design.

---

## Appendix A: Android Core Class Reference

| Java Class | C# Binding Class (proposed) | Package |
|---|---|---|
| `Engine` | `FilamentEngine` | `Filament.Maui` |
| `Renderer` | `FilamentRenderer` | `Filament.Maui` |
| `View` | `FilamentView` | `Filament.Maui` |
| `Scene` | `FilamentScene` | `Filament.Maui` |
| `Camera` | `FilamentCamera` | `Filament.Maui` |
| `SwapChain` | `FilamentSwapChain` | `Filament.Maui` |
| `Material` | `FilamentMaterial` | `Filament.Maui` |
| `MaterialInstance` | `FilamentMaterialInstance` | `Filament.Maui` |
| `Texture` | `FilamentTexture` | `Filament.Maui` |
| `RenderTarget` | `FilamentRenderTarget` | `Filament.Maui` |
| `EntityManager` | `FilamentEntityManager` | `Filament.Maui` |
| `TransformManager` | `FilamentTransformManager` | `Filament.Maui` |
| `RenderableManager` | `FilamentRenderableManager` | `Filament.Maui` |
| `LightManager` | `FilamentLightManager` | `Filament.Maui` |
| `IndirectLight` | `FilamentIndirectLight` | `Filament.Maui` |
| `Skybox` | `FilamentSkybox` | `Filament.Maui` |
| `ColorGrading` | `FilamentColorGrading` | `Filament.Maui` |
| `UiHelper` | *(internal to FilamentView)* | `Filament.Maui.Platforms.Android` |

## Appendix B: iOS Objective-C++ Wrapper Class Reference

| C++ Class | Proposed ObjC Class | Wrapper Notes |
|---|---|---|
| `filament::Engine` | `FLTEngine` | Wraps `Engine::create()` and destroy methods |
| `filament::Renderer` | `FLTRenderer` | Wraps beginFrame/render/endFrame |
| `filament::View` | `FLTView` | Viewport, scene, camera setters |
| `filament::Scene` | `FLTScene` | addEntities, setIndirectLight, setSkybox |
| `filament::Camera` | `FLTCamera` | setProjection, lookAt |
| `filament::SwapChain` | `FLTSwapChain` | Takes CAMetalLayer pointer |
| `filament::Material` | `FLTMaterial` | createInstance, parameter queries |
| `filament::MaterialInstance` | `FLTMaterialInstance` | setParameter for all types |
| `filament::Texture` | `FLTTexture` | Builder pattern, setImage |
| `filament::RenderTarget` | `FLTRenderTarget` | Attachment config |
| `utils::EntityManager` | `FLTEntityManager` | create/destroy entities |
| `filament::TransformManager` | `FLTTransformManager` | setTransform, create |
| `filament::RenderableManager` | `FLTRenderableManager` | Builder pattern |
| `filament::LightManager` | `FLTLightManager` | Builder pattern |
| `filament::IndirectLight` | `FLTIndirectLight` | Builder pattern |
| `filament::Skybox` | `FLTSkybox` | Builder pattern |
| `filament::VertexBuffer` | `FLTVertexBuffer` | Builder, setBufferAt |
| `filament::IndexBuffer` | `FLTIndexBuffer` | Builder, setBuffer |
