# TASK-009: FilamentView MAUI Control — iOS Handler

**Phase:** 3 — iOS Binding
**Estimated Effort:** 3–5 days
**Depends On:** TASK-008
**Relevant Skills:** `filament-surface-integration`, `filament-ios-binding`

## Objective

Implement the iOS platform handler for the `FilamentView` MAUI control. This handler creates a `UIView` with a `CAMetalLayer` sublayer, configures the Metal pixel format, drives the render loop via `CADisplayLink`, and wires up the cross-platform `IFilamentEngine` to the native iOS surface via `SwapChain`. This is the component that makes Filament rendering appear in a MAUI iOS app.

## Prerequisites

- TASK-008 complete — all `IFilamentEngine` and related interfaces implemented for iOS
- Understanding of `CAMetalLayer`, `CADisplayLink`, and `UIView` layout on iOS
- Understanding of the `CAMetalLayer` pixel format requirement (`MTLPixelFormatBGRA8Unorm`)
- Familiarity with MAUI `ViewHandler<TView, TNativeView>` pattern

## Deliverables

- `maui/Filament.Maui/Platforms/iOS/FilamentViewHandler.cs` — MAUI `ViewHandler` creating and managing the `UIView` + `CAMetalLayer` + `CADisplayLink`
- `maui/Filament.Maui/Platforms/iOS/FilamentMetalView.cs` — custom `UIView` subclass that overrides `LayoutSubviews` to handle resize
- `maui/Filament.Maui/MauiProgram.cs` additions — handler registration for iOS (`#if IOS`)
- A Filament frame renders visibly on a physical iOS device (or Simulator with Metal support)
- Correct surface lifecycle: SwapChain created after layer is configured, destroyed on handler disconnect

## Detailed Steps

### Step 1: Create a custom UIView subclass to handle layout changes

`maui/Filament.Maui/Platforms/iOS/FilamentMetalView.cs`:

```csharp
using CoreAnimation;
using CoreGraphics;
using Metal;
using UIKit;

namespace Filament.Maui;

/// <summary>
/// UIView subclass that hosts a CAMetalLayer for Filament rendering.
/// Overrides LayoutSubviews to notify the handler of size changes.
/// </summary>
internal sealed class FilamentMetalView : UIView
{
    public CAMetalLayer MetalLayer { get; }
    public Action<int, int>? ViewportResized { get; set; }

    public FilamentMetalView()
    {
        MetalLayer = new CAMetalLayer
        {
            Device = MTLDevice.SystemDefault,
            // CRITICAL: Filament Metal backend requires BGRA8Unorm pixel format
            PixelFormat = MTLPixelFormat.BGRA8Unorm,
            Opaque = true,
            ContentsScale = UIScreen.MainScreen.NativeScale,
        };
        Layer.AddSublayer(MetalLayer);
        ContentScaleFactor = UIScreen.MainScreen.NativeScale;
    }

    public override void LayoutSubviews()
    {
        base.LayoutSubviews();
        // Keep the Metal layer frame in sync with the view bounds
        MetalLayer.Frame = Bounds;
        var w = (int)(Bounds.Width * ContentScaleFactor);
        var h = (int)(Bounds.Height * ContentScaleFactor);
        if (w > 0 && h > 0)
            ViewportResized?.Invoke(w, h);
    }
}
```

### Step 2: Implement the FilamentViewHandler

`maui/Filament.Maui/Platforms/iOS/FilamentViewHandler.cs`:

```csharp
using CoreAnimation;
using Foundation;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Filament.Maui;

public partial class FilamentViewHandler : ViewHandler<FilamentView, FilamentMetalView>
{
    private IFilamentSwapChain? _swapChain;
    private IFilamentRenderer? _renderer;
    private IFilamentView? _filamentView;
    private CADisplayLink? _displayLink;

    public static IPropertyMapper<FilamentView, FilamentViewHandler> Mapper =
        new PropertyMapper<FilamentView, FilamentViewHandler>(ViewMapper)
        {
            [nameof(FilamentView.Engine)] = MapEngine,
        };

    public FilamentViewHandler() : base(Mapper) { }

    protected override FilamentMetalView CreatePlatformView() => new FilamentMetalView();

    protected override void ConnectHandler(FilamentMetalView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ViewportResized = OnViewportResized;
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

        // Notify consumer to set up scene/camera
        VirtualView.OnFrameRendering(
            new FilamentFrameEventArgs(_renderer, _filamentView));

        // Create SwapChain: pass CAMetalLayer as NSObject to CreateSwapChain
        // The layer must be configured (pixelFormat set) before this call
        var metalLayer = PlatformView.MetalLayer;
        var nsLayer = Runtime.GetNSObject(metalLayer.Handle)!;
        _swapChain = engine.CreateSwapChain(nsLayer);

        // Drive render loop at display refresh rate (60/120 Hz) via CADisplayLink
        _displayLink = CADisplayLink.Create(this, new Selector("renderFrame"));
        _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
    }

    [Export("renderFrame")]
    private void RenderFrame()
    {
        if (_swapChain is null || _renderer is null || _filamentView is null) return;
        if (_renderer.BeginFrame(_swapChain))
        {
            _renderer.Render(_filamentView);
            _renderer.EndFrame();
        }
    }

    private void OnViewportResized(int width, int height)
    {
        // FlushAndWait before changing viewport to prevent in-flight frame corruption
        VirtualView.Engine?.FlushAndWait();
        _filamentView?.SetViewport(0, 0, width, height);
    }

    protected override void DisconnectHandler(FilamentMetalView platformView)
    {
        // Stop render loop first
        _displayLink?.Invalidate();
        _displayLink = null;

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

        platformView.ViewportResized = null;
        base.DisconnectHandler(platformView);
    }
}
```

### Step 3: Register the iOS handler

In `FilamentMauiAppBuilderExtensions.cs`, the `UseFilament()` extension method should conditionally register the handler:

```csharp
// This file is already in the shared layer — the platform handler type resolves correctly
// because MAUI's multi-targeting compiles the handler for each platform.
builder.ConfigureMauiHandlers(handlers =>
{
    handlers.AddHandler<FilamentView, FilamentViewHandler>();
});
```

If using an `#if` guard (not recommended — prefer platform folders):
```csharp
#if IOS
handlers.AddHandler<FilamentView, FilamentViewHandler>();
#endif
```

### Step 4: Handle the CAMetalLayer → SwapChain bridge

The `IFilamentEngine.CreateSwapChain(object nativeSurface)` contract on iOS expects the `CAMetalLayer` to be passed as an `NSObject`. In `FilamentEngineiOS`, the implementation casts to `NSObject` and gets the `Handle` pointer:

```csharp
// In FilamentEngineiOS.CreateSwapChain:
var nsObj = (Foundation.NSObject)nativeSurface;
var sc = _engine.CreateSwapChainFromLayer(nsObj.Handle.ToPointer());
```

Verify that `ObjCRuntime.Runtime.GetNSObject(metalLayer.Handle)` returns the same object correctly when the layer is passed back from `FilamentMetalView`.

### Step 5: Handle app lifecycle (foreground/background)

When the app backgrounds, `CADisplayLink` will continue firing. To avoid rendering to a hidden surface, observe `UIApplicationDelegate` notifications:

```csharp
private NSObject? _didEnterBackgroundObserver;
private NSObject? _willEnterForegroundObserver;

protected override void ConnectHandler(FilamentMetalView platformView)
{
    base.ConnectHandler(platformView);
    _didEnterBackgroundObserver = UIApplication.Notifications.ObserveDidEnterBackground(
        (_, _) => _displayLink?.Invalidate());
    _willEnterForegroundObserver = UIApplication.Notifications.ObserveWillEnterForeground(
        (_, _) => ResumeDisplayLink());
}

private void ResumeDisplayLink()
{
    if (_displayLink is null && VirtualView.Engine is not null)
    {
        _displayLink = CADisplayLink.Create(this, new Selector("renderFrame"));
        _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
    }
}
```

Remove observers in `DisconnectHandler`.

### Step 6: Thread safety note

On iOS, `CADisplayLink` fires on the `NSRunLoop.Main` (UI thread). This means `BeginFrame`/`Render`/`EndFrame` run on the main thread. This is acceptable for the initial implementation. For production use, migrate to a background Metal render thread and drive the loop with a `CVDisplayLink` or `DispatchSource.MakeTimerSource`.

Document this limitation in a comment in `FilamentViewHandler.cs`.

### Step 7: Manual integration test

Create a minimal test page in the sample app (TASK-010) that:
1. Creates a `FilamentFactory.CreateEngine()` (on main thread for initial impl)
2. Binds it to a `FilamentView`
3. Sets a blue clear color on the view
4. Verifies a colored rectangle appears on a physical iOS device

```csharp
protected override void OnAppearing()
{
    base.OnAppearing();
    var engine = FilamentFactory.CreateEngine();
    var scene = engine.CreateScene();
    var camera = engine.CreateCamera();

    // Set up a minimal FrameRendering handler
    FilamentSurface.FrameRendering += (_, args) =>
    {
        args.View.SetScene(scene);
        args.View.SetCamera(camera);
        args.View.SetClearColor(0.1f, 0.3f, 0.6f, 1.0f); // Blue
    };

    FilamentSurface.Engine = engine;
}
```

## Acceptance Criteria

- [ ] `FilamentMetalView` creates a `CAMetalLayer` with `MTLPixelFormatBGRA8Unorm` before any SwapChain creation
- [ ] `FilamentViewHandler` registers `CADisplayLink` that calls `RenderFrame` each display cycle
- [ ] `LayoutSubviews` updates `MetalLayer.Frame` and calls `FlushAndWait()` + `SetViewport()` on resize
- [ ] `DisconnectHandler` invalidates `CADisplayLink`, destroys `SwapChain`, `Renderer`, and `View` in order
- [ ] `FlushAndWait()` is called before viewport resize and before SwapChain destruction
- [ ] A solid clear color renders visibly on a physical iOS device or Metal-capable simulator
- [ ] Handler registration (`UseFilament()`) is documented and works in a sample app
- [ ] App backgrounding stops rendering (no GPU activity when app is invisible)

## Reference

- See `.github/skills/filament-surface-integration/SKILL.md` — full iOS handler example and thread safety rules
- See `.github/skills/filament-ios-binding/SKILL.md` — CAMetalLayer setup and known gotchas
- See `docs/maui-binding-notes.md` — "Cross-Platform Library" section on render thread management
- MAUI custom handlers: `https://learn.microsoft.com/en-us/dotnet/maui/user-interface/handlers/create`
- CADisplayLink docs: `https://developer.apple.com/documentation/quartzcore/cadisplaylink`
- Filament iOS sample: `ios/samples/HelloCocoaPods/HelloCocoaPods/ViewController.mm`
