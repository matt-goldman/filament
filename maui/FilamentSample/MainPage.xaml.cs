using Filament.Maui;

namespace FilamentSample;

public partial class MainPage : ContentPage
{
    private IFilamentEngine? _engine;
    private IFilamentScene? _scene;
    private IFilamentCamera? _camera;
    private IFilamentView? _filamentView;
    private TriangleRenderer? _triangle;

    public MainPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Engine creation must happen before binding to FilamentView.
        // For the initial sample we create on the main thread; the platform
        // handler marshals all Filament calls to the dedicated render thread.
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
        _triangle = new TriangleRenderer(_engine, _scene, matBytes);

        FilamentSurface.Engine = _engine;
    }

    private void OnFrameRendering(object? sender, FilamentFrameEventArgs e)
    {
        // Cache the view so we can destroy it in OnDisappearing.
        _filamentView ??= e.View;

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

        // Detach engine first so the render loop stops before we destroy resources.
        FilamentSurface.Engine = null;

        if (_engine is not null)
        {
            _engine.FlushAndWait();

            _triangle?.Dispose(_engine);

            if (_filamentView != null) _engine.DestroyView(_filamentView);
            if (_scene != null)        _engine.DestroyScene(_scene);
            if (_camera != null)       _engine.DestroyCamera(_camera);

            _engine.Dispose();
            _engine = null;
        }
    }
}
