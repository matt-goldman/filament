using FilamentBinding.iOS;

namespace Filament.Maui;

/// <summary>
/// iOS implementation of <see cref="IFilamentEngine"/>.
/// Wraps <see cref="FLTEngine"/> from the iOS binding.
/// All methods must be called from the same dedicated render thread.
/// </summary>
internal sealed class FilamentEngineiOS : IFilamentEngine
{
    internal readonly FLTEngine _engine;
    private bool _disposed;

    internal FilamentEngineiOS(FLTEngine engine) =>
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));

    public IFilamentRenderer CreateRenderer()
    {
        ThrowIfDisposed();
        var renderer = _engine.CreateRenderer()
            ?? throw new InvalidOperationException("FLTEngine.CreateRenderer() returned null.");
        return new FilamentRendereriOS(renderer, this);
    }

    public IFilamentScene CreateScene()
    {
        ThrowIfDisposed();
        var scene = _engine.CreateScene()
            ?? throw new InvalidOperationException("FLTEngine.CreateScene() returned null.");
        return new FilamentSceneiOS(scene);
    }

    public IFilamentView CreateView()
    {
        ThrowIfDisposed();
        var view = _engine.CreateView()
            ?? throw new InvalidOperationException("FLTEngine.CreateView() returned null.");
        return new FilamentViewiOS(view);
    }

    public IFilamentCamera CreateCamera()
    {
        ThrowIfDisposed();
        var camera = _engine.CreateCamera()
            ?? throw new InvalidOperationException("FLTEngine.CreateCamera() returned null.");
        return new FilamentCameraiOS(camera);
    }

    public IFilamentSwapChain CreateSwapChain(object nativeSurface)
    {
        ThrowIfDisposed();
        if (nativeSurface is null) throw new ArgumentNullException(nameof(nativeSurface));

        // nativeSurface is expected to be an NSObject whose Handle points to a CAMetalLayer.
        var nsObj = nativeSurface as Foundation.NSObject
            ?? throw new ArgumentException(
                "nativeSurface must be a Foundation.NSObject wrapping a CAMetalLayer on iOS.",
                nameof(nativeSurface));

        var sc = _engine.CreateSwapChainFromLayer((IntPtr)nsObj.Handle)
            ?? throw new InvalidOperationException("FLTEngine.CreateSwapChainFromLayer() returned null.");
        return new FilamentSwapChainiOS(sc, this);
    }

    public IFilamentTransformManager TransformManager
    {
        get
        {
            ThrowIfDisposed();
            return new FilamentTransformManageriOS(_engine.TransformManager);
        }
    }

    public IFilamentRenderableManager RenderableManager
    {
        get
        {
            ThrowIfDisposed();
            return new FilamentRenderableManageriOS(_engine.RenderableManager);
        }
    }

    public IFilamentEntityManager EntityManager
    {
        get
        {
            ThrowIfDisposed();
            return new FilamentEntityManageriOS(_engine.EntityManager);
        }
    }

    public void DestroyRenderer(IFilamentRenderer renderer)
    {
        ThrowIfDisposed();
        _engine.DestroyRenderer(((FilamentRendereriOS)renderer)._renderer);
    }

    public void DestroyScene(IFilamentScene scene)
    {
        ThrowIfDisposed();
        _engine.DestroyScene(((FilamentSceneiOS)scene)._scene);
    }

    public void DestroyView(IFilamentView view)
    {
        ThrowIfDisposed();
        _engine.DestroyView(((FilamentViewiOS)view)._view);
    }

    public void DestroyCamera(IFilamentCamera camera)
    {
        ThrowIfDisposed();
        _engine.DestroyCamera(((FilamentCameraiOS)camera)._camera);
    }

    public void DestroySwapChain(IFilamentSwapChain swapChain)
    {
        ThrowIfDisposed();
        _engine.DestroySwapChain(((FilamentSwapChainiOS)swapChain)._swapChain);
    }

    public void DestroyMaterial(IFilamentMaterial material)
    {
        ThrowIfDisposed();
        _engine.DestroyMaterial(((FilamentMaterialiOS)material)._material);
    }

    public void DestroyMaterialInstance(IFilamentMaterialInstance instance)
    {
        ThrowIfDisposed();
        _engine.DestroyMaterialInstance(((FilamentMaterialInstanceiOS)instance)._instance);
    }

    public void DestroyTexture(IFilamentTexture texture)
    {
        ThrowIfDisposed();
        _engine.DestroyTexture(((FilamentTextureiOS)texture)._texture);
    }

    public void DestroySkybox(IFilamentSkybox skybox)
    {
        ThrowIfDisposed();
        _engine.DestroySkybox(((FilamentSkyboxiOS)skybox)._skybox);
    }

    public void DestroyIndirectLight(IFilamentIndirectLight ibl)
    {
        ThrowIfDisposed();
        _engine.DestroyIndirectLight(((FilamentIndirectLightiOS)ibl)._ibl);
    }

    public void FlushAndWait()
    {
        ThrowIfDisposed();
        _engine.FlushAndWait();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _engine.Destroy();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FilamentEngineiOS));
    }
}
