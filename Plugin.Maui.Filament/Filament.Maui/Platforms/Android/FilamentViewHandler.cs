using Android.OS;
using Android.Views;
using Com.Google.Android.Filament.Android;
using Microsoft.Maui.Handlers;

namespace Filament.Maui;

/// <summary>
/// MAUI platform handler for <see cref="FilamentView"/> on Android.
/// Creates a <see cref="SurfaceView"/>, manages <see cref="UiHelper"/> for surface
/// lifecycle events, drives the render loop on a dedicated <see cref="HandlerThread"/>
/// gated by <see cref="Choreographer"/> for vsync-aligned frame pacing,
/// and wires the cross-platform <see cref="IFilamentEngine"/> to the native surface
/// via <see cref="IFilamentSwapChain"/>.
/// </summary>
public partial class FilamentViewHandler : ViewHandler<FilamentView, SurfaceView>
{
    private UiHelper? _uiHelper;
    private IFilamentSwapChain? _swapChain;
    private IFilamentRenderer? _renderer;
    private IFilamentView? _filamentView;
    private IFilamentEngine? _currentEngine;
    private HandlerThread? _renderThread;
    private Android.OS.Handler? _renderHandler;
    private volatile bool _rendering;
    // Reused across frames to avoid per-frame managed + JNI allocations.
    private FrameCallback? _frameCallback;
    // Cached native window surface so the SwapChain can be created when the Engine
    // is assigned after the surface has already become available.
    private Surface? _pendingSurface;

    /// <summary>Property mapper for the <see cref="FilamentView"/> virtual view.</summary>
    public static readonly IPropertyMapper<FilamentView, FilamentViewHandler> Mapper =
        new PropertyMapper<FilamentView, FilamentViewHandler>(ViewMapper)
        {
            [nameof(FilamentView.Engine)] = MapEngine,
        };

    /// <summary>Initialises a new <see cref="FilamentViewHandler"/>.</summary>
    public FilamentViewHandler() : base(Mapper) { }

    /// <inheritdoc />
    protected override SurfaceView CreatePlatformView()
    {
        var surfaceView = new SurfaceView(Context);
        _uiHelper = new UiHelper(UiHelper.ContextErrorPolicy.DontCheck);
        _uiHelper.RenderCallback = new FilamentSurfaceLifecycleCallback(this);
        _uiHelper.AttachTo(surfaceView);
        return surfaceView;
    }

    private static void MapEngine(FilamentViewHandler handler, FilamentView view)
    {
        var newEngine = view.Engine;

        // No-op if the engine hasn't actually changed.
        if (ReferenceEquals(newEngine, handler._currentEngine)) return;

        // StopRendering destroys the swapchain, renderer and view on the render thread
        // (Filament thread-affinity requirement) before quitting the HandlerThread.
        handler.StopRendering();

        if (newEngine is not null)
            handler.StartRendering(newEngine);
    }

    private void StartRendering(IFilamentEngine engine)
    {
        // Stop any existing render loop before starting a new one to prevent resource leaks
        // and ensure there is never more than one active render thread at a time.
        StopRendering();

        _currentEngine = engine;

        // Create the reusable frame callback once per render session.
        _frameCallback = new FrameCallback(this);

        _renderThread = new HandlerThread("FilamentRenderThread");
        _renderThread.Start();
        _renderHandler = new Android.OS.Handler(_renderThread.Looper!);
        _rendering = true;

        // All Filament resource creation must happen on the dedicated render thread to satisfy
        // Filament's thread-affinity requirement. Capture the pending surface before posting so
        // we use the surface that was current at the moment StartRendering was called.
        // The FrameCallback null-checks _renderer, _filamentView and _swapChain before rendering,
        // so frames are gracefully skipped until these resources are ready.
        var pendingSurface = _pendingSurface;
        _renderHandler.Post(() =>
        {
            // Guard: bail out if the engine was replaced between the Post and execution.
            if (!ReferenceEquals(_currentEngine, engine)) return;

            _renderer = engine.CreateRenderer();
            _filamentView = engine.CreateView();

            // If the native window surface became available before the Engine was set,
            // create the SwapChain now that we have both a surface and an engine.
            if (pendingSurface != null)
                _swapChain = engine.CreateSwapChain(pendingSurface);
        });

        // Kick off the first vsync-aligned frame
        ScheduleNextFrame();
    }

    /// <summary>
    /// Schedules the next frame via <see cref="Choreographer"/> on the UI thread,
    /// providing vsync-aligned frame pacing instead of rendering as fast as possible.
    /// The same <see cref="FrameCallback"/> instance is reused each vsync to avoid
    /// per-frame managed + JNI allocations.
    /// </summary>
    private void ScheduleNextFrame()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_rendering || _frameCallback is null) return;
            Choreographer.Instance!.PostFrameCallback(_frameCallback);
        });
    }

    /// <summary>
    /// Stops the render loop, waits for any in-flight frame on the render thread to
    /// complete (so resources can be safely destroyed), then disposes renderer/view.
    /// Safe to call if no loop is currently running.
    /// </summary>
    private void StopRendering()
    {
        if (!_rendering) return;
        _rendering = false;

        // Capture the last posted callback and unregister it from the Choreographer on the
        // UI thread before clearing the reference.  This prevents DoFrame from being invoked
        // at the next vsync even when a frame was already queued before StopRendering ran.
        var lastCallback = _frameCallback;
        _frameCallback = null;
        if (lastCallback != null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                Choreographer.Instance?.RemoveFrameCallback(lastCallback));
        }

        // Capture all Filament resources into locals before posting the cleanup lambda.
        // This guarantees cleanup runs with the correct references even if the 500 ms timeout
        // fires before the lambda begins executing and instance fields are subsequently cleared
        // by a racing StartRendering call or the null-out at the bottom of this method.
        var engine = _currentEngine;
        var swapChain = _swapChain;
        var renderer = _renderer;
        var filamentView = _filamentView;

        // Clear instance fields now so concurrent readers (e.g. FrameCallback, which already
        // sees _rendering == false) observe a consistent "stopped" state immediately.
        _swapChain = null;
        _renderer = null;
        _filamentView = null;

        // Post cleanup work onto the render thread so all Filament resources are destroyed
        // on the same thread they were used on (Filament thread-affinity requirement).
        using var done = new ManualResetEventSlim(false);
        _renderHandler?.Post(() =>
        {
            if (engine is not null)
            {
                engine.FlushAndWait();
                // Destroy the swapchain on the render thread (must match the thread used
                // for rendering and the engine that created it).
                if (swapChain != null)
                {
                    var ownerEngine = (swapChain as FilamentSwapChainAndroid)?.Engine ?? engine;
                    ownerEngine.DestroySwapChain(swapChain);
                }
                if (renderer != null)
                    engine.DestroyRenderer(renderer);
                if (filamentView != null)
                    engine.DestroyView(filamentView);
            }
            done.Set();
        });

        // Wait for cleanup to complete before tearing down the thread (500ms safety timeout).
        bool cleaned = done.Wait(TimeSpan.FromMilliseconds(500));
        if (!cleaned)
        {
            System.Diagnostics.Debug.WriteLine(
                "[FilamentViewHandler] StopRendering timed out waiting for render-thread cleanup. " +
                "This may indicate a deadlock or a slow cleanup operation. " +
                "Filament resources may not have been fully released on the render thread.");
        }

        _renderThread?.QuitSafely();
        _renderThread = null;
        _renderHandler = null;
        _currentEngine = null;
    }

    /// <inheritdoc />
    protected override void DisconnectHandler(SurfaceView platformView)
    {
        // StopRendering destroys the swapchain, renderer and view on the render thread
        // before quitting the HandlerThread, satisfying Filament's thread-affinity requirement.
        StopRendering();

        _uiHelper?.Detach();
        _uiHelper = null;

        base.DisconnectHandler(platformView);
    }

    /// <summary>
    /// Choreographer frame callback — runs on the UI thread at vsync.
    /// Posts the actual rendering work to the dedicated render thread.
    /// </summary>
    private sealed class FrameCallback : Java.Lang.Object, Choreographer.IFrameCallback
    {
        private readonly FilamentViewHandler _handler;

        public FrameCallback(FilamentViewHandler handler) => _handler = handler;

        public void DoFrame(long frameTimeNanos)
        {
            _handler._renderHandler?.Post(() =>
            {
                if (!_handler._rendering) return;

                var swapChain = _handler._swapChain;
                var renderer = _handler._renderer;
                var filamentView = _handler._filamentView;

                if (swapChain != null && renderer != null && filamentView != null)
                {
                    // Raise FrameRendering each frame so consumers can update scene/camera state.
                    // Cache VirtualView to avoid a redundant allocation if the view is gone.
                    var virtualView = _handler.VirtualView;
                    if (virtualView != null)
                        virtualView.OnFrameRendering(new FilamentFrameEventArgs(renderer, filamentView));

                    if (renderer.BeginFrame(swapChain))
                    {
                        renderer.Render(filamentView);
                        renderer.EndFrame();
                    }
                }

                // Schedule the next vsync-aligned frame
                _handler.ScheduleNextFrame();
            });
        }
    }

    /// <summary>
    /// Surface lifecycle callback bridging <see cref="UiHelper"/> events to this handler.
    /// Swapchain and viewport operations are marshalled to the render thread when one is
    /// active, satisfying Filament's thread-affinity requirement.
    /// Named <c>FilamentSurfaceLifecycleCallback</c> to avoid ambiguity with the public
    /// <c>FilamentRendererCallback</c> type from the Android binding project.
    /// </summary>
    private sealed class FilamentSurfaceLifecycleCallback : Java.Lang.Object, UiHelper.IRendererCallback
    {
        private readonly FilamentViewHandler _handler;

        public FilamentSurfaceLifecycleCallback(FilamentViewHandler handler) =>
            _handler = handler;

        /// <summary>
        /// Called when the native window surface becomes available (UI thread).
        /// Caches the surface so that the SwapChain can be created even if the Engine was
        /// not yet assigned when this callback fired. Marshals swapchain creation onto the
        /// render thread; if the render thread is not yet running the cached surface will
        /// be picked up by <see cref="FilamentViewHandler.StartRendering"/> when the Engine
        /// is later assigned.
        /// </summary>
        public void OnNativeWindowChanged(Surface? p0)
        {
            var surface = p0;
            // Always cache the latest surface so StartRendering can pick it up if the
            // Engine is assigned after this callback fires.
            _handler._pendingSurface = surface;

            var engineAtCallback = _handler.VirtualView?.Engine;
            if (engineAtCallback is null || surface is null) return;

            // Only post to the render thread when one is running. If _renderHandler is
            // null the render thread has not started yet; StartRendering will create the
            // SwapChain from _pendingSurface when the Engine is eventually assigned.
            var renderHandler = _handler._renderHandler;
            if (renderHandler is null) return;

            renderHandler.Post(() =>
            {
                // Guard: bail out if the engine changed between when this was posted and
                // when it actually runs on the render thread.
                if (!ReferenceEquals(_handler._currentEngine, engineAtCallback)) return;

                if (_handler._swapChain != null)
                {
                    // Destroy via the engine that originally created the swapchain.
                    var ownerEngine = (_handler._swapChain as FilamentSwapChainAndroid)?.Engine ?? engineAtCallback;
                    ownerEngine.FlushAndWait();
                    ownerEngine.DestroySwapChain(_handler._swapChain);
                }
                _handler._swapChain = engineAtCallback.CreateSwapChain(surface);
            });
        }

        /// <summary>
        /// Called when the native window surface is destroyed (UI thread).
        /// Clears the cached surface and marshals swapchain destruction onto the render thread.
        /// If the render thread is not running there is no SwapChain to destroy.
        /// </summary>
        public void OnDetachedFromSurface()
        {
            // Clear the cached surface so StartRendering does not try to create a
            // SwapChain against a destroyed surface.
            _handler._pendingSurface = null;

            // Only post to the render thread when one is running. If _renderHandler is
            // null there is no render thread and therefore no SwapChain to destroy.
            var renderHandler = _handler._renderHandler;
            if (renderHandler is null) return;

            renderHandler.Post(() =>
            {
                var swapChain = _handler._swapChain;
                if (swapChain is null) return;
                // Destroy via the engine that originally created the swapchain.
                var ownerEngine = (swapChain as FilamentSwapChainAndroid)?.Engine;
                if (ownerEngine is null) return;
                ownerEngine.FlushAndWait();
                ownerEngine.DestroySwapChain(swapChain);
                _handler._swapChain = null;
            });
        }

        /// <summary>
        /// Called when the surface is resized (UI thread).
        /// Marshals the viewport update onto the render thread. If the render thread is
        /// not yet running there is no <see cref="IFilamentView"/> to update.
        /// </summary>
        public void OnResized(int width, int height)
        {
            var engineAtCallback = _handler.VirtualView?.Engine;

            // Only post to the render thread when one is running. If _renderHandler is
            // null there is no render thread and therefore no IFilamentView to update.
            var renderHandler = _handler._renderHandler;
            if (renderHandler is null) return;

            renderHandler.Post(() =>
            {
                // Guard: bail out if the engine changed between when this was posted and
                // when it actually runs on the render thread.
                if (!ReferenceEquals(_handler._currentEngine, engineAtCallback)) return;

                if (engineAtCallback is FilamentEngineAndroid androidEngine)
                {
                    FilamentHelper.SynchronizePendingFrames(androidEngine._engine);
                }
                _handler._filamentView?.SetViewport(0, 0, width, height);
            });
        }
    }
}
