using Filament.Maui;

namespace FilamentSample;

public partial class MainPage : ContentPage
{
    private IFilamentEngine? _engine;
    private IFilamentScene? _scene;
    private IFilamentCamera? _camera;
    private TriangleRenderer? _triangle;
    // Used to abort the async OnAppearing continuation if OnDisappearing runs first.
    private CancellationTokenSource? _cts;

    public MainPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Cancel and replace any prior async initialisation that may still be in-flight
        // (e.g. repeated OnAppearing/OnDisappearing from fast navigation).
        _cts?.Cancel();
        _cts?.Dispose();
        var cts = _cts = new CancellationTokenSource();

        // Engine, scene, and camera are created on the UI thread before the render
        // loop starts. FilamentSurface.Engine is set last, which triggers StartRendering
        // on the handler and starts the platform render thread. All three resources
        // therefore exist before any render-thread access.
        _engine = FilamentFactory.CreateEngine();
        _scene  = _engine.CreateScene();
        _camera = _engine.CreateCamera();

        _camera.SetProjection(
            fovDegrees: 60.0,  // vertical field of view in degrees
            aspect:      1.0,  // width / height (updated per frame in production)
            near:        0.1,  // near clipping plane (metres)
            far:       100.0); // far clipping plane (metres)
        _camera.LookAt(
            eyeX: 0, eyeY: 0, eyeZ: 3,   // camera position
            centerX: 0, centerY: 0, centerZ: 0,  // look-at target
            upX: 0, upY: 1, upZ: 0);      // up vector

        // Load material bytes asynchronously before constructing the renderer
        // to avoid blocking the render thread on file I/O.
        var matBytes = await TriangleRenderer.LoadMaterialBytesAsync();

        // Guard: if OnDisappearing ran while we were awaiting (fast navigation /
        // app backgrounding), abort here and let OnDisappearing's teardown stand.
        if (cts.IsCancellationRequested || _engine is null || _scene is null)
            return;

        _triangle = new TriangleRenderer(_engine, _scene, matBytes);

        // Assign the engine last — this is what triggers the platform handler to
        // start the render loop.
        FilamentSurface.Engine = _engine;
    }

    private void OnFrameRendering(object? sender, FilamentFrameEventArgs e)
    {
        // Configure the view each frame (idempotent — safe to call repeatedly).
        e.View.SetScene(_scene!);
        e.View.SetCamera(_camera!);
        e.View.SetPostProcessingEnabled(false);

        // SetClearColor lives on IFilamentRenderer, not IFilamentView.
        e.Renderer.SetClearColor(0.15f, 0.15f, 0.2f, 1.0f);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Signal any in-flight OnAppearing continuation to abort before we tear down.
        var cts = _cts;
        _cts = null;
        cts?.Cancel();
        cts?.Dispose();

        // Detach engine first so the render loop stops before we destroy resources.
        // The handler's StopRendering call destroys the Filament View it owns internally;
        // we must not call engine.DestroyView here to avoid a double-destroy.
        FilamentSurface.Engine = null;

        if (_engine is not null)
        {
            _engine.FlushAndWait();

            _triangle?.Dispose(_engine);

            if (_scene != null)  _engine.DestroyScene(_scene);
            if (_camera != null) _engine.DestroyCamera(_camera);

            _engine.Dispose();
            _engine = null;
        }
    }
}
