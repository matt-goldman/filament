using Android.OS;
using Android.Views;
using Com.Google.Android.Filament.Android;
using Microsoft.Maui.Handlers;

namespace Filament.Maui;

/// <summary>
/// MAUI platform handler for <see cref="FilamentView"/> on Android.
/// Creates a <see cref="SurfaceView"/>, manages <see cref="UiHelper"/> for surface
/// lifecycle events, drives the render loop on a dedicated <see cref="HandlerThread"/>,
/// and wires the cross-platform <see cref="IFilamentEngine"/> to the native surface
/// via <see cref="IFilamentSwapChain"/>.
/// </summary>
public partial class FilamentViewHandler : ViewHandler<FilamentView, SurfaceView>
{
    private UiHelper? _uiHelper;
    private IFilamentSwapChain? _swapChain;
    private IFilamentRenderer? _renderer;
    private IFilamentView? _filamentView;
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
        _renderer = engine.CreateRenderer();
        _filamentView = engine.CreateView();

        // Notify consumer so they can set up scene/camera
        VirtualView.OnFrameRendering(new FilamentFrameEventArgs(_renderer, _filamentView));

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

    /// <inheritdoc />
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

    /// <summary>
    /// Surface lifecycle callback bridging <see cref="UiHelper"/> events to this handler.
    /// </summary>
    private sealed class FilamentRendererCallback : Java.Lang.Object, UiHelper.IRendererCallback
    {
        private readonly FilamentViewHandler _handler;

        public FilamentRendererCallback(FilamentViewHandler handler) =>
            _handler = handler;

        /// <summary>
        /// Called when the native window surface becomes available.
        /// Creates a new <see cref="IFilamentSwapChain"/> from the ready surface.
        /// </summary>
        public void OnNativeWindowChanged(Surface? p0)
        {
            var surface = p0;
            var engine = _handler.VirtualView.Engine;
            if (engine is null || surface is null) return;

            // Destroy any previous SwapChain before creating a new one
            if (_handler._swapChain != null)
            {
                engine.FlushAndWait();
                engine.DestroySwapChain(_handler._swapChain);
            }
            _handler._swapChain = engine.CreateSwapChain(surface);
        }

        /// <summary>
        /// Called when the native window surface is destroyed.
        /// Destroys the <see cref="IFilamentSwapChain"/> and flushes pending work.
        /// </summary>
        public void OnDetachedFromSurface()
        {
            var engine = _handler.VirtualView.Engine;
            if (engine is null || _handler._swapChain is null) return;
            engine.FlushAndWait();
            engine.DestroySwapChain(_handler._swapChain);
            _handler._swapChain = null;
        }

        /// <summary>
        /// Called when the surface is resized.
        /// Synchronizes pending frames and updates the viewport.
        /// </summary>
        public void OnResized(int width, int height)
        {
            var engine = _handler.VirtualView.Engine;
            if (engine is FilamentEngineAndroid androidEngine)
            {
                FilamentHelper.SynchronizePendingFrames(androidEngine._engine);
            }
            _handler._filamentView?.SetViewport(0, 0, width, height);
        }
    }
}
