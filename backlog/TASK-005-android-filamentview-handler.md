# TASK-005: FilamentView MAUI Control — Android Handler

**Phase:** 1 — Android Binding
**Estimated Effort:** 3–5 days
**Depends On:** TASK-004
**Relevant Skills:** `filament-surface-integration`, `filament-android-binding`

## Objective

Implement the Android platform handler for the `FilamentView` MAUI control. This handler creates a `SurfaceView`, manages `UiHelper` for surface lifecycle callbacks, drives the render loop on a dedicated `HandlerThread`, and wires up the cross-platform `IFilamentEngine` to the native Android surface via `SwapChain`. This is the component that makes Filament rendering appear in a MAUI Android app.

## Prerequisites

- TASK-004 complete — all `IFilamentEngine` and related interfaces implemented for Android
- Understanding of Android `SurfaceView`, `UiHelper`, and `HandlerThread` patterns
- Familiarity with MAUI `ViewHandler<TView, TNativeView>` pattern

## Deliverables

- `maui/Filament.Maui/Platforms/Android/FilamentViewHandler.cs` — MAUI `ViewHandler` that creates and manages the `SurfaceView` + `UiHelper`
- `maui/Filament.Maui/MauiProgram.cs` additions — handler registration (`handlers.AddHandler<FilamentView, FilamentViewHandler>()`)
- A Filament frame renders visibly on a physical Android device or emulator
- Correct surface lifecycle: create SwapChain on surface ready, destroy on detach, resize on `onResized`

## Detailed Steps

### Step 1: Implement the FilamentViewHandler

`maui/Filament.Maui/Platforms/Android/FilamentViewHandler.cs`:

```csharp
using Android.OS;
using Android.Views;
using Com.Google.Android.Filament.Android;
using Microsoft.Maui.Handlers;

namespace Filament.Maui;

public partial class FilamentViewHandler : ViewHandler<FilamentView, SurfaceView>
{
    private UiHelper? _uiHelper;
    private IFilamentSwapChain? _swapChain;
    private IFilamentRenderer? _renderer;
    private IFilamentView? _filamentView;
    private HandlerThread? _renderThread;
    private Android.OS.Handler? _renderHandler;
    private bool _rendering;

    public static IPropertyMapper<FilamentView, FilamentViewHandler> Mapper =
        new PropertyMapper<FilamentView, FilamentViewHandler>(ViewMapper)
        {
            [nameof(FilamentView.Engine)] = MapEngine,
        };

    public FilamentViewHandler() : base(Mapper) { }

    protected override SurfaceView CreatePlatformView()
    {
        var surfaceView = new SurfaceView(Context);
        SetupUiHelper(surfaceView);
        return surfaceView;
    }

    private void SetupUiHelper(SurfaceView surfaceView)
    {
        _uiHelper = new UiHelper(UiHelper.ContextErrorPolicy.DontCheck);
        _uiHelper.SetRenderCallback(new FilamentRendererCallback(this));
        _uiHelper.AttachTo(surfaceView);
    }

    private static void MapEngine(FilamentViewHandler handler, FilamentView view)
    {
        if (view.Engine is not null)
            handler.StartRendering(view.Engine);
    }

    private void StartRendering(IFilamentEngine engine)
    {
        _renderer = engine.CreateRenderer();
        _filamentView = engine.CreateView();

        // Notify consumer so they can set up scene/camera on the FilamentView
        VirtualView.OnFrameRendering(
            new FilamentFrameEventArgs(_renderer, _filamentView));

        _renderThread = new HandlerThread("FilamentRenderThread");
        _renderThread.Start();
        _renderHandler = new Android.OS.Handler(_renderThread.Looper!);
        _rendering = true;
        PostFrame();
    }

    private void PostFrame()
    {
        _renderHandler?.Post(() =>
        {
            if (!_rendering) return;
            if (_swapChain != null)
            {
                if (_renderer!.BeginFrame(_swapChain))
                {
                    _renderer.Render(_filamentView!);
                    _renderer.EndFrame();
                }
            }
            PostFrame();
        });
    }

    protected override void DisconnectHandler(SurfaceView platformView)
    {
        _rendering = false;
        _renderThread?.QuitSafely();
        _renderThread = null;
        _renderHandler = null;

        var engine = VirtualView.Engine;
        if (engine is not null)
        {
            engine.FlushAndWait();
            if (_swapChain != null)
            {
                engine.DestroySwapChain(_swapChain);
                _swapChain = null;
            }
            if (_renderer != null)
            {
                engine.DestroyRenderer(_renderer);
                _renderer = null;
            }
            if (_filamentView != null)
            {
                engine.DestroyView(_filamentView);
                _filamentView = null;
            }
        }

        _uiHelper?.Detach();
        _uiHelper = null;

        base.DisconnectHandler(platformView);
    }

    private sealed class FilamentRendererCallback : Java.Lang.Object, UiHelper.IRendererCallback
    {
        private readonly FilamentViewHandler _handler;

        public FilamentRendererCallback(FilamentViewHandler handler) => _handler = handler;

        public void OnNativeWindowChanged(Surface surface)
        {
            // Called on the UI thread — create SwapChain from the ready surface
            var engine = _handler.VirtualView.Engine;
            if (engine is null) return;

            if (_handler._swapChain != null)
            {
                engine.FlushAndWait();
                engine.DestroySwapChain(_handler._swapChain);
            }
            _handler._swapChain = engine.CreateSwapChain(surface);
        }

        public void OnDetachedFromSurface()
        {
            var engine = _handler.VirtualView.Engine;
            if (engine is null || _handler._swapChain is null) return;
            engine.FlushAndWait();
            engine.DestroySwapChain(_handler._swapChain);
            _handler._swapChain = null;
        }

        public void OnResized(int width, int height)
        {
            // Synchronize before changing viewport to avoid in-flight frame corruption
            var engine = _handler.VirtualView.Engine;
            if (engine is not null)
            {
                // FilamentHelper.SynchronizePendingFrames requires the Java engine
                var jEngine = ((FilamentEngineAndroid)engine)._engine;
                Com.Google.Android.Filament.Android.FilamentHelper.SynchronizePendingFrames(jEngine);
            }
            _handler._filamentView?.SetViewport(0, 0, width, height);
        }
    }
}
```

### Step 2: Register the handler in MauiProgram.cs

In the MAUI app's `MauiProgram.cs` (and in the library's service registration if using a MAUI library):

```csharp
builder.ConfigureMauiHandlers(handlers =>
{
    handlers.AddHandler<FilamentView, FilamentViewHandler>();
});
```

If registering inside the library, add a `FilamentMauiAppBuilderExtensions.cs`:

```csharp
namespace Filament.Maui;

public static class FilamentMauiAppBuilderExtensions
{
    public static MauiAppBuilder UseFilament(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<FilamentView, FilamentViewHandler>();
        });
        return builder;
    }
}
```

### Step 3: Handle UiHelper.IRendererCallback interface

In MAUI .NET Android bindings, Java interfaces are exposed as C# interfaces prefixed with `I`. Verify the generated name for `UiHelper.RendererCallback`:

```csharp
// Check which name the binding generates — it may be one of:
// UiHelper.IRendererCallback (interface)
// UiHelper.RendererCallback  (abstract class if transformed in Metadata.xml)
```

If `Metadata.xml` marks it as `abstract`, implement as:

```csharp
private sealed class FilamentRendererCallback : UiHelper.RendererCallback { ... }
```

If it remains an interface, implement `UiHelper.IRendererCallback` and wrap with `Java.Lang.Object`.

### Step 4: Surface lifecycle edge cases

Handle the case where `SetEngine` is called before the surface is ready:

```csharp
// In OnNativeWindowChanged: if engine is already set, create SwapChain immediately.
// If engine is set later (after surface is ready), ensure SwapChain is created then.
// This requires the handler to check both conditions.
```

Handle app backgrounding: override `OnWindowVisibilityChanged` or observe MAUI lifecycle events to pause/resume the render loop when the app moves to background/foreground.

### Step 5: Manual integration test

Create a minimal test page in the sample app (TASK-010) that:
1. Creates a `FilamentFactory.CreateEngine()` on a background thread
2. Binds it to a `FilamentView` with a solid clear color
3. Verifies that a colored rectangle appears on screen

```csharp
// TestPage.xaml.cs
protected override void OnAppearing()
{
    base.OnAppearing();
    Task.Run(() =>
    {
        var engine = FilamentFactory.CreateEngine();
        var renderer = engine.CreateRenderer();
        var view = engine.CreateView();
        var scene = engine.CreateScene();
        var camera = engine.CreateCamera();

        view.SetScene(scene);
        view.SetCamera(camera);
        view.SetClearColor(0.2f, 0.4f, 0.8f, 1.0f); // Blue background

        MainThread.BeginInvokeOnMainThread(() =>
        {
            FilamentSurface.Engine = engine; // Bind to the FilamentView
        });
    });
}
```

## Acceptance Criteria

- [ ] `FilamentViewHandler` is registered and `FilamentView` renders on Android
- [ ] `SurfaceView` is created and attached via `UiHelper`
- [ ] `OnNativeWindowChanged` creates a `SwapChain` successfully
- [ ] `OnDetachedFromSurface` destroys the `SwapChain` and calls `FlushAndWait()` before release
- [ ] `OnResized` calls `FilamentHelper.SynchronizePendingFrames()` and updates the viewport
- [ ] Render loop runs on a dedicated `HandlerThread` (not the UI thread)
- [ ] `DisconnectHandler` cleans up the render thread, SwapChain, Renderer, and View
- [ ] A solid clear color renders visibly on device/emulator

## Reference

- See `.github/skills/filament-surface-integration/SKILL.md` — full Android handler example and render loop
- See `.github/skills/filament-android-binding/SKILL.md` — `UiHelper` usage and thread safety rules
- See `docs/maui-binding-notes.md` — render thread management notes
- MAUI custom handlers: `https://learn.microsoft.com/en-us/dotnet/maui/user-interface/handlers/create`
- Filament Android sample: `android/samples/hello-triangle/` (reference for UiHelper usage)
