---
name: filament-surface-integration
description: >
  Guidance for integrating Filament's SwapChain with platform-native surfaces in
  .NET MAUI. Covers Android SurfaceView/TextureView with UiHelper, iOS CAMetalLayer
  with CADisplayLink, the FilamentView MAUI control pattern, render thread management,
  and resize/lifecycle handling on both platforms.
  USE FOR: "filament swapchain", "filament surface", "filament SurfaceView",
  "filament CAMetalLayer", "filament render loop", "filament FilamentView",
  "filament resize", "filament render thread", "filament window integration".
  DO NOT USE FOR: binding project setup (use filament-android-binding or
  filament-ios-binding), API surface design (use filament-maui-api-surface).
---

# Filament Surface Integration in .NET MAUI

The `SwapChain` is the single most platform-divergent part of the Filament API.
It binds the renderer to a native window surface and must receive a platform-native
pointer. Each platform requires different setup.

---

## Platform Comparison

| Concern | Android | iOS |
|---|---|---|
| Native surface type | `android.view.Surface` | `CAMetalLayer` |
| MAUI UI element | `SurfaceView` or `TextureView` | `UIView` with `CAMetalLayer` |
| SwapChain creation | `engine.createSwapChain(surface)` | `engine->createSwapChain((__bridge void*)layer)` |
| Render loop driver | `HandlerThread` (background) | `CADisplayLink` (display sync) |
| Resize notification | `UiHelper.RendererCallback.onResized()` | `MTKViewDelegate.drawableSizeWillChange` or `viewDidLayoutSubviews` |
| Lifecycle | `onNativeWindowChanged` / `onDetachedFromSurface` | App foreground/background via `UIApplicationDelegate` |

---

## Android: UiHelper + SurfaceView

`UiHelper` is the recommended Android surface manager. It abstracts
`SurfaceView`, `TextureView`, and `SurfaceHolder` and provides callbacks
for surface lifecycle events.

### Example (MAUI Android Handler)

```csharp
// Platforms/Android/FilamentViewHandler.cs
using Android.Views;
using Com.Google.Android.Filament.Android;

public class FilamentViewHandler : ViewHandler<FilamentView, SurfaceView>
{
    private UiHelper? _uiHelper;
    private Com.Google.Android.Filament.SwapChain? _swapChain;
    private readonly HandlerThread _renderThread = new("FilamentRenderThread");

    protected override SurfaceView CreatePlatformView()
    {
        var surfaceView = new SurfaceView(Context);
        _uiHelper = new UiHelper(UiHelper.ContextErrorPolicy.DontCheck);
        _uiHelper.SetRenderCallback(new FilamentRendererCallback(this, _engine));
        _uiHelper.AttachTo(surfaceView);
        return surfaceView;
    }

    private class FilamentRendererCallback : Java.Lang.Object, IUiHelperRendererCallback
    {
        private readonly FilamentViewHandler _parent;
        private readonly IFilamentEngine _engine;

        public FilamentRendererCallback(FilamentViewHandler parent, IFilamentEngine engine)
        {
            _parent = parent;
            _engine = engine;
        }

        public void OnNativeWindowChanged(Surface surface)
        {
            // Called when surface is ready — create or recreate SwapChain
            _parent._swapChain = _engine.CreateSwapChain(
                surface, _parent._uiHelper!.SwapChainFlags);
        }

        public void OnDetachedFromSurface()
        {
            // Destroy SwapChain before surface is released
            if (_parent._swapChain != null) {
                _engine.DestroySwapChain(_parent._swapChain);
                _engine.FlushAndWait();
                _parent._swapChain = null;
            }
        }

        public void OnResized(int width, int height)
        {
            // Synchronize pending frames before resizing
            FilamentHelper.SynchronizePendingFrames((Com.Google.Android.Filament.Engine)_engine);
            _parent.OnViewportResized(width, height);
        }
    }
}
```

### Render Loop (Android)

```csharp
_renderThread.Start();
var handler = new Android.OS.Handler(_renderThread.Looper);

void PostFrame()
{
    handler.Post(() =>
    {
        if (_swapChain != null && _renderer.BeginFrame(_swapChain))
        {
            _renderer.Render(_view);
            _renderer.EndFrame();
        }
        PostFrame();  // Schedule next frame
    });
}
PostFrame();
```

---

## iOS: CAMetalLayer + CADisplayLink

On iOS, Filament renders directly to a `CAMetalLayer`. The layer must be configured
before `createSwapChain()` is called.

### Example (MAUI iOS Handler)

```csharp
// Platforms/iOS/FilamentViewHandler.cs
using CoreAnimation;
using Foundation;
using Metal;
using UIKit;

public class FilamentViewHandler : ViewHandler<FilamentView, UIView>
{
    private CAMetalLayer? _metalLayer;
    private CADisplayLink? _displayLink;
    private FLTSwapChain? _swapChain;

    protected override UIView CreatePlatformView()
    {
        var view = new UIView();
        view.ContentScaleFactor = UIScreen.MainScreen.NativeScale;

        // Set up CAMetalLayer
        _metalLayer = new CAMetalLayer
        {
            Device = MTLDevice.SystemDefault,
            PixelFormat = MTLPixelFormat.BGRA8Unorm,   // Required by Filament Metal backend
            Frame = view.Bounds,
            ContentsScale = UIScreen.MainScreen.NativeScale,
            Opaque = true,
        };
        view.Layer.AddSublayer(_metalLayer);

        // Create SwapChain
        _swapChain = _engine.CreateSwapChainFromLayer(
            ObjCRuntime.Runtime.GetNSObject(_metalLayer.Handle));

        // Start render loop
        _displayLink = CADisplayLink.Create(this, new ObjCRuntime.Selector("renderFrame"));
        _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);

        return view;
    }

    [Foundation.Export("renderFrame")]
    private void RenderFrame()
    {
        if (_renderer.BeginFrame(_swapChain!))
        {
            _renderer.Render(_view);
            _renderer.EndFrame();
        }
    }

    // Layout changes are handled by subclassing UIView to override layoutSubviews,
    // or by observing bounds changes via a custom UIView subclass.
    private void OnBoundsChanged()
    {
        if (_metalLayer != null && PlatformView != null)
        {
            _metalLayer.Frame = PlatformView.Bounds;
            OnViewportResized(
                (int)PlatformView.Bounds.Width,
                (int)PlatformView.Bounds.Height);
        }
    }
}
```

---

## Thread Safety Rules

These rules apply to both platforms:

1. **Create `Engine` on the render thread** — the same thread that calls
   `BeginFrame`/`Render`/`EndFrame`.
2. **Never call Filament methods from the UI thread** unless using a thread-safe
   command queue (Filament does not provide one — this must be implemented by the caller).
3. **`FlushAndWait()` before resizing** — always call before changing viewport
   dimensions to prevent in-flight frame corruption.
4. **Destroy in order** — destroy resources before the `Engine` is destroyed:
   `Renderer` → `View` → `Scene` → `Camera` → `SwapChain` → `Engine`.

---

## FilamentView MAUI Control Pattern

The `FilamentView` control is the public entry point for the cross-platform library.
It is a MAUI `View` subclass backed by platform handlers.

```csharp
// FilamentView.cs (shared)
namespace Filament.Maui;

public class FilamentView : View
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
```

Register platform handlers in `MauiProgram.cs`:

```csharp
builder.ConfigureMauiHandlers(handlers =>
{
    handlers.AddHandler<FilamentView, FilamentViewHandler>();
});
```
