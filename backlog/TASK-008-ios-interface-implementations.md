# TASK-008: iOS Implementation of Cross-Platform Interfaces

**Phase:** 3 — iOS Binding
**Estimated Effort:** 3–5 days
**Depends On:** TASK-003, TASK-007
**Relevant Skills:** `filament-ios-binding`, `filament-maui-api-surface`

## Objective

Implement all cross-platform interfaces defined in TASK-003 for the iOS platform inside `Filament.Maui/Platforms/iOS/`. Each implementation wraps the corresponding `FLT*` class from `FilamentBinding.iOS`. This mirrors the structure of TASK-004 (Android implementations) and enables iOS apps to use `IFilamentEngine` and friends with no platform-specific code in the shared app layer.

## Prerequisites

- TASK-003 complete — all `IFilament*` interfaces defined
- TASK-007 complete — `FilamentBinding.iOS.dll` builds with clean API exposing all `FLT*` classes
- Familiarity with the `FLT*` Objective-C wrapper class API from TASK-006
- Understanding that `Entity` is `uint` in the `FLT*` API but `int` in the cross-platform interfaces (cast required)

## Deliverables

- `maui/Filament.Maui/Platforms/iOS/FilamentEngineiOS.cs` — `IFilamentEngine` implementation
- `maui/Filament.Maui/Platforms/iOS/FilamentRendereriOS.cs` — `IFilamentRenderer` implementation
- `maui/Filament.Maui/Platforms/iOS/FilamentViewiOS.cs` — `IFilamentView` implementation
- `maui/Filament.Maui/Platforms/iOS/FilamentSceneiOS.cs` — `IFilamentScene` implementation
- `maui/Filament.Maui/Platforms/iOS/FilamentCameraiOS.cs` — `IFilamentCamera` implementation
- `maui/Filament.Maui/Platforms/iOS/FilamentSwapChainiOS.cs` — `IFilamentSwapChain` implementation
- `maui/Filament.Maui/Platforms/iOS/FilamentMaterialiOS.cs` — `IFilamentMaterial` + `IFilamentMaterialInstance` implementations
- `maui/Filament.Maui/Platforms/iOS/FilamentManagersiOS.cs` — `IFilamentEntityManager`, `IFilamentTransformManager`, `IFilamentRenderableManager` implementations
- `maui/Filament.Maui/Platforms/iOS/FilamentFactory.cs` — iOS-specific factory creating `IFilamentEngine`
- `dotnet build -f net10.0-ios` succeeds

## Detailed Steps

### Step 1: Add the FilamentBinding.iOS reference to the project

Uncomment the iOS `ProjectReference` in `Filament.Maui.csproj`:

```xml
<ItemGroup Condition="$(TargetFramework.StartsWith('net10.0-ios'))">
  <ProjectReference Include="..\FilamentBinding.iOS\FilamentBinding.iOS.csproj" />
</ItemGroup>
```

### Step 2: Implement FilamentFactory for iOS

`maui/Filament.Maui/Platforms/iOS/FilamentFactory.cs`:

```csharp
using FilamentBinding.iOS;

namespace Filament.Maui;

public static class FilamentFactory
{
    /// <summary>
    /// Creates a Filament engine using the Metal backend (recommended for iOS).
    /// Must be called on the render thread — all subsequent Filament calls must
    /// originate from the same thread.
    /// </summary>
    public static IFilamentEngine CreateEngine() =>
        new FilamentEngineiOS(FLTEngine.Create(FLTBackend.Metal));
}
```

### Step 3: Implement IFilamentEngine

`maui/Filament.Maui/Platforms/iOS/FilamentEngineiOS.cs`:

```csharp
using FilamentBinding.iOS;
using ObjCRuntime;

namespace Filament.Maui;

internal sealed class FilamentEngineiOS : IFilamentEngine
{
    internal readonly FLTEngine _engine;
    private bool _disposed;

    public FilamentEngineiOS(FLTEngine engine) => _engine = engine;

    public IFilamentRenderer CreateRenderer() =>
        new FilamentRendereriOS(_engine.CreateRenderer(), this);

    public IFilamentScene CreateScene() =>
        new FilamentSceneiOS(_engine.CreateScene());

    public IFilamentView CreateView() =>
        new FilamentViewiOS(_engine.CreateView());

    public IFilamentCamera CreateCamera() =>
        new FilamentCameraiOS(_engine.CreateCamera());

    public IFilamentSwapChain CreateSwapChain(object nativeSurface)
    {
        // nativeSurface is an NSObject wrapping a CAMetalLayer handle.
        // Pass as void* via Handle property.
        var nsObj = (Foundation.NSObject)nativeSurface;
        var sc = _engine.CreateSwapChainFromLayer(nsObj.Handle.ToPointer());
        return new FilamentSwapChainiOS(sc, this);
    }

    public IFilamentTransformManager TransformManager =>
        new FilamentTransformManageriOS(_engine.TransformManager);

    public IFilamentRenderableManager RenderableManager =>
        new FilamentRenderableManageriOS(_engine.RenderableManager);

    public IFilamentEntityManager EntityManager =>
        new FilamentEntityManageriOS(_engine.EntityManager);

    public void DestroyRenderer(IFilamentRenderer renderer) =>
        _engine.DestroyRenderer(((FilamentRendereriOS)renderer)._renderer);

    public void DestroyScene(IFilamentScene scene) =>
        _engine.DestroyScene(((FilamentSceneiOS)scene)._scene);

    public void DestroyView(IFilamentView view) =>
        _engine.DestroyView(((FilamentViewiOS)view)._view);

    public void DestroyCamera(IFilamentCamera camera) =>
        _engine.DestroyCamera(((FilamentCameraiOS)camera)._camera);

    public void DestroySwapChain(IFilamentSwapChain swapChain) =>
        _engine.DestroySwapChain(((FilamentSwapChainiOS)swapChain)._swapChain);

    public void FlushAndWait() => _engine.FlushAndWait();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _engine.Destroy();
    }
}
```

### Step 4: Implement IFilamentRenderer

```csharp
using FilamentBinding.iOS;

namespace Filament.Maui;

internal sealed class FilamentRendereriOS : IFilamentRenderer
{
    internal readonly FLTRenderer _renderer;
    private readonly FilamentEngineiOS _engine;

    public FilamentRendereriOS(FLTRenderer renderer, FilamentEngineiOS engine)
    {
        _renderer = renderer;
        _engine = engine;
    }

    public bool BeginFrame(IFilamentSwapChain swapChain) =>
        _renderer.BeginFrame(((FilamentSwapChainiOS)swapChain)._swapChain);

    public void Render(IFilamentView view) =>
        _renderer.Render(((FilamentViewiOS)view)._view);

    public void EndFrame() => _renderer.EndFrame();

    public void Dispose() => _engine.DestroyRenderer(this);
}
```

### Step 5: Implement IFilamentView, IFilamentScene, IFilamentCamera

`maui/Filament.Maui/Platforms/iOS/FilamentViewiOS.cs`:

```csharp
using FilamentBinding.iOS;

namespace Filament.Maui;

internal sealed class FilamentViewiOS : IFilamentView
{
    internal readonly FLTView _view;

    public FilamentViewiOS(FLTView view) => _view = view;

    public void SetScene(IFilamentScene scene) =>
        _view.SetScene(((FilamentSceneiOS)scene)._scene);

    public void SetCamera(IFilamentCamera camera) =>
        _view.SetCamera(((FilamentCameraiOS)camera)._camera);

    public void SetViewport(int left, int bottom, int width, int height) =>
        _view.SetViewportLeft(left, bottom: bottom, width: (uint)width, height: (uint)height);

    public void SetClearColor(float r, float g, float b, float a) =>
        _view.SetClearColorRed(r, green: g, blue: b, alpha: a);

    public void SetPostProcessingEnabled(bool enabled) =>
        _view.SetPostProcessingEnabled(enabled);

    public void Dispose() { /* Destroyed via engine.DestroyView() */ }
}
```

`maui/Filament.Maui/Platforms/iOS/FilamentCameraiOS.cs`:

```csharp
using FilamentBinding.iOS;

namespace Filament.Maui;

internal sealed class FilamentCameraiOS : IFilamentCamera
{
    internal readonly FLTCamera _camera;

    public FilamentCameraiOS(FLTCamera camera) => _camera = camera;

    public void SetProjection(double fovDegrees, double aspect, double near, double far) =>
        _camera.SetProjectionFov(fovDegrees, aspect: aspect, near: near, far: far);

    public void LookAt(double ex, double ey, double ez,
                       double cx, double cy, double cz,
                       double ux, double uy, double uz) =>
        _camera.LookAtEyeX(ex, eyeY: ey, eyeZ: ez,
                            centerX: cx, centerY: cy, centerZ: cz,
                            upX: ux, upY: uy, upZ: uz);

    public void Dispose() { /* Destroyed via engine.DestroyCamera() */ }
}
```

### Step 6: Implement manager interfaces

`maui/Filament.Maui/Platforms/iOS/FilamentManagersiOS.cs`:

```csharp
using FilamentBinding.iOS;
using System.Numerics;

namespace Filament.Maui;

internal sealed class FilamentEntityManageriOS : IFilamentEntityManager
{
    private readonly FLTEntityManager _mgr;
    public FilamentEntityManageriOS(FLTEntityManager mgr) => _mgr = mgr;

    // Entity is uint in FLT* API, int in cross-platform interface — safe cast for IDs < 2^31
    public int Create() => (int)_mgr.Create();
    public void Destroy(int entity) => _mgr.Destroy((uint)entity);
}

internal sealed class FilamentTransformManageriOS : IFilamentTransformManager
{
    private readonly FLTTransformManager _mgr;
    public FilamentTransformManageriOS(FLTTransformManager mgr) => _mgr = mgr;

    public void Create(int entity) => _mgr.CreateComponent((uint)entity);

    public void SetTransform(int entity, float[] mat4ColumnMajor)
    {
        // Convert float[16] column-major array to simd_float4x4
        // The FLT wrapper accepts OpenTK.Matrix4 or System.Numerics.Matrix4x4
        // depending on how Sharpie mapped simd_float4x4 — adjust as needed
        var m = new Matrix4x4(
            mat4ColumnMajor[0],  mat4ColumnMajor[1],  mat4ColumnMajor[2],  mat4ColumnMajor[3],
            mat4ColumnMajor[4],  mat4ColumnMajor[5],  mat4ColumnMajor[6],  mat4ColumnMajor[7],
            mat4ColumnMajor[8],  mat4ColumnMajor[9],  mat4ColumnMajor[10], mat4ColumnMajor[11],
            mat4ColumnMajor[12], mat4ColumnMajor[13], mat4ColumnMajor[14], mat4ColumnMajor[15]
        );
        _mgr.SetTransform(m, forEntity: (uint)entity);
    }
}
```

### Step 7: Material loading helper for iOS

```csharp
using FilamentBinding.iOS;
using Foundation;

namespace Filament.Maui;

public static class FilamentMaterialLoader
{
    /// <summary>
    /// Loads a compiled .mat file from the app bundle and creates a Material.
    /// </summary>
    public static IFilamentMaterial LoadMaterial(IFilamentEngine engine, byte[] matData)
    {
        var fltEngine = ((FilamentEngineiOS)engine)._engine;
        var nsData = NSData.FromArray(matData);
        var material = FLTMaterial.BuildWithEngine(fltEngine, nsData);
        return new FilamentMaterialiOS(material);
    }
}
```

### Step 8: Build and verify

```bash
cd maui/Filament.Maui
dotnet build -f net10.0-ios -c Debug
```

## Acceptance Criteria

- [ ] `dotnet build -f net10.0-ios` succeeds with no errors
- [ ] `FilamentFactory.CreateEngine()` returns a non-null `IFilamentEngine` wrapping `FLTEngine` with Metal backend
- [ ] All 13 minimum viable interface types are implemented in `Platforms/iOS/`
- [ ] `Entity` is `int` in all cross-platform interface implementations (cast from `uint` FLT values)
- [ ] `CreateSwapChain(object nativeSurface)` correctly passes a `CAMetalLayer` handle to `FLTEngine.CreateSwapChainFromLayer`
- [ ] Material loading from `byte[]` works via `FilamentMaterialLoader.LoadMaterial()`
- [ ] `FlushAndWait()` delegates to `FLTEngine.FlushAndWait()`
- [ ] No direct references to `FilamentBinding.iOS.*` types appear outside `Platforms/iOS/`

## Reference

- See `.github/skills/filament-ios-binding/SKILL.md` — known gotchas (pointer-to-pointer, simd types)
- See `.github/skills/filament-maui-api-surface/SKILL.md` — interface definitions and platform folder layout
- See `docs/maui-binding-notes.md` — "iOS Binding" critical notes
- TASK-007: `FilamentBinding.iOS` — source of `FLT*` types used here
