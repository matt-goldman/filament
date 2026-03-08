using CoreAnimation;
using Foundation;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Filament.Maui;

/// <summary>
/// MAUI platform handler for <see cref="FilamentView"/> on iOS.
/// Creates a <see cref="FilamentMetalView"/> (a <c>UIView</c> with a
/// <c>CAMetalLayer</c> sublayer), drives the render loop via
/// <see cref="CADisplayLink"/>, and wires the cross-platform
/// <see cref="IFilamentEngine"/> to the native Metal surface via
/// <see cref="IFilamentSwapChain"/>.
/// </summary>
    /// <remarks>
    /// Thread-safety note: <see cref="CADisplayLink"/> fires on <c>NSRunLoop.Main</c>
    /// (the UI thread) and must therefore be treated as a vsync tick only. In
    /// accordance with the repository's thread-safety rules, Filament entry points
    /// such as <c>BeginFrame</c>, <c>Render</c>, and <c>EndFrame</c> must not be
    /// invoked directly on the UI thread. Instead, the display-link callback should
    /// enqueue work to a dedicated Metal render thread or command queue, typically
    /// driven by a <c>CVDisplayLink</c> or <c>DispatchSource</c> timer and mirroring
    /// the Android handler's <c>HandlerThread</c> pattern.
    /// </remarks>
    public partial class FilamentViewHandler : ViewHandler<FilamentView, FilamentMetalView>
{
    private IFilamentSwapChain? _swapChain;
    private IFilamentRenderer? _renderer;
    private IFilamentView? _filamentView;
    private IFilamentEngine? _currentEngine;
    private CADisplayLink? _displayLink;
    private DisplayLinkTarget? _displayLinkTarget;
    private NSObject? _didEnterBackgroundObserver;
    private NSObject? _willEnterForegroundObserver;

    /// <summary>Property mapper for the <see cref="FilamentView"/> virtual view.</summary>
    public static readonly IPropertyMapper<FilamentView, FilamentViewHandler> Mapper =
        new PropertyMapper<FilamentView, FilamentViewHandler>(ViewMapper)
        {
            [nameof(FilamentView.Engine)] = MapEngine,
        };

    /// <summary>Initialises a new <see cref="FilamentViewHandler"/>.</summary>
    public FilamentViewHandler() : base(Mapper) { }

    /// <inheritdoc />
    protected override FilamentMetalView CreatePlatformView() => new FilamentMetalView();

    /// <inheritdoc />
    protected override void ConnectHandler(FilamentMetalView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ViewportResized = OnViewportResized;

        // Pause rendering while the app is invisible to avoid GPU work in the background.
        _didEnterBackgroundObserver = UIApplication.Notifications.ObserveDidEnterBackground(
            (_, _) => StopDisplayLink());
        _willEnterForegroundObserver = UIApplication.Notifications.ObserveWillEnterForeground(
            (_, _) => ResumeDisplayLink());
    }

    private static void MapEngine(FilamentViewHandler handler, FilamentView view)
    {
        var newEngine = view.Engine;

        // No-op if the engine hasn't actually changed.
        if (ReferenceEquals(newEngine, handler._currentEngine)) return;

        handler.StopRendering();

        if (newEngine is not null)
            handler.StartRendering(newEngine);
    }

    private void StartRendering(IFilamentEngine engine)
    {
        // Guard against double-start; StopRendering is idempotent.
        StopRendering();

        _currentEngine = engine;
        _renderer = engine.CreateRenderer();
        _filamentView = engine.CreateView();

        // CAMetalLayer is an NSObject subclass; pass it directly so the iOS binding
        // can extract the native Handle and create the Filament SwapChain.
        // The layer must be fully configured (PixelFormat = BGRA8Unorm) before this call.
        _swapChain = engine.CreateSwapChain(PlatformView.MetalLayer);

        StartDisplayLink();
    }

    private void StartDisplayLink()
    {
        if (_displayLink is not null) return;
        _displayLinkTarget = new DisplayLinkTarget(this);
        _displayLink = CADisplayLink.Create(_displayLinkTarget, new Selector("renderFrame"));
        _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
    }

    private void StopDisplayLink()
    {
        _displayLink?.Invalidate();
        _displayLink = null;
        _displayLinkTarget = null;
    }

    private void ResumeDisplayLink()
    {
        // Only restart the display link when all Filament resources are ready.
        // Checking all three resources (in addition to _currentEngine) guards against
        // the unlikely case where foregrounding occurs between StopRendering and a
        // subsequent StartRendering call.
        if (_displayLink is null &&
            _currentEngine is not null &&
            _swapChain is not null &&
            _renderer is not null &&
            _filamentView is not null)
        {
            StartDisplayLink();
        }
    }

    /// <summary>
    /// Called by <see cref="DisplayLinkTarget"/> each display-refresh cycle.
    /// Raises <see cref="FilamentView.FrameRendering"/> so consumers can update
    /// scene/camera state, then executes the Filament frame.
    /// </summary>
    private void RenderFrame()
    {
        if (_swapChain is null || _renderer is null || _filamentView is null) return;

        // Raise FrameRendering each frame so consumers can update scene/camera state.
        // Cache VirtualView to avoid redundant allocation if the view is gone.
        var virtualView = VirtualView;
        if (virtualView is not null)
            virtualView.OnFrameRendering(new FilamentFrameEventArgs(_renderer, _filamentView));

        if (_renderer.BeginFrame(_swapChain))
        {
            _renderer.Render(_filamentView);
            _renderer.EndFrame();
        }
    }

    private void OnViewportResized(int width, int height)
    {
        // FlushAndWait before changing the viewport to prevent in-flight frame corruption.
        VirtualView?.Engine?.FlushAndWait();
        _filamentView?.SetViewport(0, 0, width, height);
    }

    private void StopRendering()
    {
        StopDisplayLink();

        var engine = _currentEngine;
        if (engine is not null)
        {
            // FlushAndWait ensures no frames are in flight before destroying resources.
            engine.FlushAndWait();

            // Destroy resources in the correct order: SwapChain → Renderer → View.
            if (_swapChain is not null)
            {
                engine.DestroySwapChain(_swapChain);
                _swapChain = null;
            }
            if (_renderer is not null)
            {
                engine.DestroyRenderer(_renderer);
                _renderer = null;
            }
            if (_filamentView is not null)
            {
                engine.DestroyView(_filamentView);
                _filamentView = null;
            }
        }

        _currentEngine = null;
    }

    /// <inheritdoc />
    protected override void DisconnectHandler(FilamentMetalView platformView)
    {
        // Stop the render loop and destroy all Filament resources before detaching.
        StopRendering();

        // Remove app lifecycle observers to prevent callbacks on a detached handler.
        if (_didEnterBackgroundObserver is not null)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_didEnterBackgroundObserver);
            _didEnterBackgroundObserver = null;
        }
        if (_willEnterForegroundObserver is not null)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_willEnterForegroundObserver);
            _willEnterForegroundObserver = null;
        }

        platformView.ViewportResized = null;
        base.DisconnectHandler(platformView);
    }

    /// <summary>
    /// Dedicated <see cref="NSObject"/> subclass used as the <see cref="CADisplayLink"/>
    /// target. <c>CADisplayLink</c> requires an <c>NSObject</c> with a selector-based
    /// callback; this wrapper forwards the call to the owning handler.
    /// </summary>
    private sealed class DisplayLinkTarget : NSObject
    {
        private readonly FilamentViewHandler _handler;

        public DisplayLinkTarget(FilamentViewHandler handler) => _handler = handler;

        [Export("renderFrame")]
        public void RenderFrame() => _handler.RenderFrame();
    }
}
