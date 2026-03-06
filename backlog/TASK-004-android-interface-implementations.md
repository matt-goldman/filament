# TASK-004: Android Implementation of Cross-Platform Interfaces

**Phase:** 1 — Android Binding
**Estimated Effort:** 3–5 days
**Depends On:** TASK-002, TASK-003
**Relevant Skills:** `filament-android-binding`, `filament-maui-api-surface`

## Objective

Implement all cross-platform interfaces defined in TASK-003 for the Android platform inside `Filament.Maui/Platforms/Android/`. Each implementation wraps the corresponding Java binding class from `FilamentBinding.Android`. This enables Android apps to use the `IFilamentEngine` and friends without any platform-specific code.

## Prerequisites

- TASK-002 complete — `FilamentBinding.Android.dll` builds with clean API
- TASK-003 complete — all `IFilament*` interfaces are defined
- Understanding of the Filament Java API in `android/filament-android/src/main/java/com/google/android/filament/`

## Deliverables

- `maui/Filament.Maui/Platforms/Android/FilamentEngineAndroid.cs` — `IFilamentEngine` implementation
- `maui/Filament.Maui/Platforms/Android/FilamentRendererAndroid.cs` — `IFilamentRenderer` implementation
- `maui/Filament.Maui/Platforms/Android/FilamentViewAndroid.cs` — `IFilamentView` implementation
- `maui/Filament.Maui/Platforms/Android/FilamentSceneAndroid.cs` — `IFilamentScene` implementation
- `maui/Filament.Maui/Platforms/Android/FilamentCameraAndroid.cs` — `IFilamentCamera` implementation
- `maui/Filament.Maui/Platforms/Android/FilamentSwapChainAndroid.cs` — `IFilamentSwapChain` implementation
- `maui/Filament.Maui/Platforms/Android/FilamentMaterialAndroid.cs` — `IFilamentMaterial` + `IFilamentMaterialInstance` implementations
- `maui/Filament.Maui/Platforms/Android/FilamentManagersAndroid.cs` — `IFilamentEntityManager`, `IFilamentTransformManager`, `IFilamentRenderableManager` implementations
- `maui/Filament.Maui/Platforms/Android/FilamentFactory.cs` — static factory to create an `IFilamentEngine` on Android
- Full `dotnet build` for `net10.0-android` succeeds

## Detailed Steps

### Step 1: Add initialization and factory

`maui/Filament.Maui/Platforms/Android/FilamentFactory.cs`:

```csharp
using Com.Google.Android.Filament;

namespace Filament.Maui;

public static class FilamentFactory
{
    private static bool _initialized;

    /// <summary>
    /// Call once in MauiProgram.cs before using any Filament API.
    /// Loads libfilament-jni.so via Filament.Init().
    /// </summary>
    public static IFilamentEngine CreateEngine()
    {
        if (!_initialized)
        {
            Com.Google.Android.Filament.Filament.Init();
            _initialized = true;
        }
        var javaEngine = Engine.Create();
        return new FilamentEngineAndroid(javaEngine);
    }
}
```

### Step 2: Implement IFilamentEngine

`maui/Filament.Maui/Platforms/Android/FilamentEngineAndroid.cs`:

```csharp
using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

internal sealed class FilamentEngineAndroid : IFilamentEngine
{
    internal readonly JFilament.Engine _engine;
    private bool _disposed;

    public FilamentEngineAndroid(JFilament.Engine engine) => _engine = engine;

    public IFilamentRenderer CreateRenderer() =>
        new FilamentRendererAndroid(_engine.CreateRenderer(), this);

    public IFilamentScene CreateScene() =>
        new FilamentSceneAndroid(_engine.CreateScene());

    public IFilamentView CreateView() =>
        new FilamentViewAndroid(_engine.CreateView());

    public IFilamentCamera CreateCamera() =>
        new FilamentCameraAndroid(_engine.CreateCamera(_engine.EntityManager.Create()));

    public IFilamentSwapChain CreateSwapChain(object nativeSurface) =>
        new FilamentSwapChainAndroid(
            _engine.CreateSwapChain((Android.Views.Surface)nativeSurface, 0), this);

    public IFilamentTransformManager TransformManager =>
        new FilamentTransformManagerAndroid(_engine.TransformManager);

    public IFilamentRenderableManager RenderableManager =>
        new FilamentRenderableManagerAndroid(_engine.RenderableManager);

    public IFilamentEntityManager EntityManager =>
        new FilamentEntityManagerAndroid(_engine.EntityManager);

    public void DestroyRenderer(IFilamentRenderer renderer) =>
        _engine.DestroyRenderer(((FilamentRendererAndroid)renderer)._renderer);

    public void DestroyScene(IFilamentScene scene) =>
        _engine.DestroyScene(((FilamentSceneAndroid)scene)._scene);

    public void DestroyView(IFilamentView view) =>
        _engine.DestroyView(((FilamentViewAndroid)view)._view);

    public void DestroyCamera(IFilamentCamera camera) =>
        _engine.DestroyCamera(((FilamentCameraAndroid)camera)._camera);

    public void DestroySwapChain(IFilamentSwapChain swapChain) =>
        _engine.DestroySwapChain(((FilamentSwapChainAndroid)swapChain)._swapChain);

    public void FlushAndWait() => _engine.FlushAndWait();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _engine.Dispose();
    }
}
```

### Step 3: Implement IFilamentRenderer

`maui/Filament.Maui/Platforms/Android/FilamentRendererAndroid.cs`:

```csharp
using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

internal sealed class FilamentRendererAndroid : IFilamentRenderer
{
    internal readonly JFilament.Renderer _renderer;
    private readonly FilamentEngineAndroid _engine;

    public FilamentRendererAndroid(JFilament.Renderer renderer, FilamentEngineAndroid engine)
    {
        _renderer = renderer;
        _engine = engine;
    }

    public bool BeginFrame(IFilamentSwapChain swapChain) =>
        _renderer.BeginFrame(((FilamentSwapChainAndroid)swapChain)._swapChain, 0);

    public void Render(IFilamentView view) =>
        _renderer.Render(((FilamentViewAndroid)view)._view);

    public void EndFrame() => _renderer.EndFrame();

    public void Dispose() => _engine.DestroyRenderer(this);
}
```

### Step 4: Implement IFilamentView, IFilamentScene, IFilamentCamera

`maui/Filament.Maui/Platforms/Android/FilamentViewAndroid.cs`:

```csharp
using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

internal sealed class FilamentViewAndroid : IFilamentView
{
    internal readonly JFilament.View _view;

    public FilamentViewAndroid(JFilament.View view) => _view = view;

    public void SetScene(IFilamentScene scene) =>
        _view.Scene = ((FilamentSceneAndroid)scene)._scene;

    public void SetCamera(IFilamentCamera camera) =>
        _view.Camera = ((FilamentCameraAndroid)camera)._camera;

    public void SetViewport(int left, int bottom, int width, int height) =>
        _view.Viewport = new JFilament.Viewport(left, bottom, width, height);

    public void SetClearColor(float r, float g, float b, float a) =>
        _view.SetClearColor(r, g, b, a);

    public void SetPostProcessingEnabled(bool enabled) =>
        _view.PostProcessingEnabled = enabled;

    public void Dispose() { /* Destroyed via engine.DestroyView() */ }
}
```

Implement `FilamentSceneAndroid`, `FilamentCameraAndroid`, `FilamentSwapChainAndroid` following the same pattern, delegating to the corresponding Java binding properties and methods.

### Step 5: Implement manager interfaces

`maui/Filament.Maui/Platforms/Android/FilamentManagersAndroid.cs`:

```csharp
using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

internal sealed class FilamentEntityManagerAndroid : IFilamentEntityManager
{
    private readonly JFilament.EntityManager _mgr;
    public FilamentEntityManagerAndroid(JFilament.EntityManager mgr) => _mgr = mgr;
    public int Create() => _mgr.Create();
    public void Destroy(int entity) => _mgr.Destroy(entity);
}

internal sealed class FilamentTransformManagerAndroid : IFilamentTransformManager
{
    private readonly JFilament.TransformManager _mgr;
    public FilamentTransformManagerAndroid(JFilament.TransformManager mgr) => _mgr = mgr;

    public void Create(int entity) => _mgr.Create(entity);

    public void SetTransform(int entity, float[] mat4ColumnMajor)
    {
        // Filament Android takes a float[] for the 4x4 column-major transform matrix
        _mgr.SetTransform(entity, mat4ColumnMajor);
    }
}

internal sealed class FilamentRenderableManagerAndroid : IFilamentRenderableManager
{
    private readonly JFilament.RenderableManager _mgr;
    public FilamentRenderableManagerAndroid(JFilament.RenderableManager mgr) => _mgr = mgr;

    public void SetMaterialInstanceAt(int entity, int primitiveIndex, IFilamentMaterialInstance instance) =>
        _mgr.SetMaterialInstanceAt(
            _mgr.GetInstance(entity), primitiveIndex,
            ((FilamentMaterialInstanceAndroid)instance)._instance);
}
```

### Step 6: Material loading helper

Add to `Platforms/Android/FilamentMaterialAndroid.cs`:

```csharp
using Java.Nio;

namespace Filament.Maui;

public static class FilamentMaterialLoader
{
    /// <summary>
    /// Loads a compiled .mat file from app Assets and creates a Material.
    /// </summary>
    public static IFilamentMaterial LoadMaterial(IFilamentEngine engine, byte[] matData)
    {
        var jEngine = ((FilamentEngineAndroid)engine)._engine;
        var buffer = ByteBuffer.Wrap(matData)!;
        var material = new Com.Google.Android.Filament.Material.Builder()
            .Payload(buffer, matData.Length)
            .Build(jEngine)!;
        return new FilamentMaterialAndroid(material);
    }
}
```

### Step 7: Verify the build

```bash
cd maui/Filament.Maui
dotnet build -f net10.0-android -c Debug
```

## Acceptance Criteria

- [ ] `dotnet build -f net10.0-android` succeeds with no errors
- [ ] `FilamentFactory.CreateEngine()` returns a non-null `IFilamentEngine` on Android
- [ ] All 13 interface types are implemented in `Platforms/Android/`
- [ ] `Entity` remains as `int` throughout all Android implementations
- [ ] Material loading from a `byte[]` works via `FilamentMaterialLoader.LoadMaterial()`
- [ ] `FlushAndWait()` delegates to `engine.FlushAndWait()` on the Java binding
- [ ] No direct references to `Com.Google.Android.Filament.*` types appear outside `Platforms/Android/`

## Reference

- See `.github/skills/filament-android-binding/SKILL.md` — "Important Patterns" section
- See `.github/skills/filament-maui-api-surface/SKILL.md` — interface definitions
- See `docs/maui-binding-notes.md` — "Android Binding" critical notes
- Filament Android Java source: `android/filament-android/src/main/java/com/google/android/filament/`
