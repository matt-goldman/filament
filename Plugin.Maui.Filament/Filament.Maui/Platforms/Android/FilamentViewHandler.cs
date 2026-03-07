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
        _uiHelper.RenderCallback = new FilamentRendererCallback(this);
        _uiHelper.AttachTo(surfaceView);
        return surfaceView;
    }

    private static void MapEngine(FilamentViewHandler handler, FilamentView view)
    {
        if (view.Engine is not null)
            handler.StartRendering(view.Engine);
    }

    private void StartRendering(IFilamentEngine engine)
    {
        // Stop any existing render loop before starting a new one to prevent resource leaks
        // and ensure there is never more than one active render thread at a time.
        StopRendering();

        _currentEngine = engine;
        _renderer = engine.CreateRenderer();
        _filamentView = engine.CreateView();

        _renderThread = new HandlerThread("FilamentRenderThread");
        _renderThread.Start();
        _renderHandler = new Android.OS.Handler(_renderThread.Looper!);
        _rendering = true;

        // Kick off the first vsync-aligned frame
        ScheduleNextFrame();
    }

    /// <summary>
    /// Schedules the next frame via <see cref="Choreographer"/> on the UI thread,
    /// providing vsync-aligned frame pacing instead of rendering as fast as possible.
    /// </summary>
    private void ScheduleNextFrame()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_rendering) return;
            Choreographer.Instance!.PostFrameCallback(new FrameCallback(this));
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

        // Post cleanup work onto the render thread so renderer/view are destroyed on the
        // same thread they were used on (Filament thread-affinity requirement).
        using var done = new ManualResetEventSlim(false);
        _renderHandler?.Post(() =>
        {
            var engine = _currentEngine;
            if (engine is not null)
            {
                engine.FlushAndWait();
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
            done.Set();
        });

        // Wait for cleanup to complete before tearing down the thread (500ms safety timeout).
        done.Wait(TimeSpan.FromMilliseconds(500));

        _renderThread?.QuitSafely();
        _renderThread = null;
        _renderHandler = null;
        _currentEngine = null;
    }

    /// <inheritdoc />
    protected override void DisconnectHandler(SurfaceView platformView)
    {
        // StopRendering waits for in-flight frames and destroys renderer/view on the render thread.
        StopRendering();

        var engine = VirtualView?.Engine;
        if (engine is not null && _swapChain != null)
        {
            engine.FlushAndWait();
            engine.DestroySwapChain(_swapChain);
            _swapChain = null;
        }

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
    /// </summary>
    private sealed class FilamentRendererCallback : Java.Lang.Object, UiHelper.IRendererCallback
    {
        private readonly FilamentViewHandler _handler;

        public FilamentRendererCallback(FilamentViewHandler handler) =>
            _handler = handler;

        /// <summary>
        /// Called when the native window surface becomes available (UI thread).
        /// Marshals swapchain creation onto the render thread when rendering is active;
        /// creates it directly on the calling thread otherwise.
        /// </summary>
        public void OnNativeWindowChanged(Surface? p0)
        {
            var surface = p0;
            var engine = _handler.VirtualView?.Engine;
            if (engine is null || surface is null) return;

            void CreateChain()
            {
                if (_handler._swapChain != null)
                {
                    engine.FlushAndWait();
                    engine.DestroySwapChain(_handler._swapChain);
                }
                _handler._swapChain = engine.CreateSwapChain(surface);
            }

            var renderHandler = _handler._renderHandler;
            if (renderHandler != null)
                renderHandler.Post(CreateChain);
            else
                CreateChain();
        }

        /// <summary>
        /// Called when the native window surface is destroyed (UI thread).
        /// Marshals swapchain destruction onto the render thread when rendering is active.
        /// </summary>
        public void OnDetachedFromSurface()
        {
            var engine = _handler.VirtualView?.Engine;

            void DestroyChain()
            {
                if (engine is null || _handler._swapChain is null) return;
                engine.FlushAndWait();
                engine.DestroySwapChain(_handler._swapChain);
                _handler._swapChain = null;
            }

            var renderHandler = _handler._renderHandler;
            if (renderHandler != null)
                renderHandler.Post(DestroyChain);
            else
                DestroyChain();
        }

        /// <summary>
        /// Called when the surface is resized (UI thread).
        /// Marshals the viewport update onto the render thread when rendering is active.
        /// </summary>
        public void OnResized(int width, int height)
        {
            var engine = _handler.VirtualView?.Engine;

            void UpdateViewport()
            {
                if (engine is FilamentEngineAndroid androidEngine)
                {
                    FilamentHelper.SynchronizePendingFrames(androidEngine._engine);
                }
                _handler._filamentView?.SetViewport(0, 0, width, height);
            }

            var renderHandler = _handler._renderHandler;
            if (renderHandler != null)
                renderHandler.Post(UpdateViewport);
            else
                UpdateViewport();
        }
    }
}
