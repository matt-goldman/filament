using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

/// <summary>
/// Android implementation of <see cref="IFilamentEngine"/>.
/// Wraps <see cref="JFilament.Engine"/> from the Java binding.
/// All methods must be called from the same dedicated render thread.
/// </summary>
internal sealed class FilamentEngineAndroid : IFilamentEngine
{
    internal readonly JFilament.Engine _engine;
    private bool _disposed;

    internal FilamentEngineAndroid(JFilament.Engine engine) =>
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));

    public IFilamentRenderer CreateRenderer()
    {
        var renderer = _engine.CreateRenderer()
            ?? throw new InvalidOperationException("Engine.CreateRenderer() returned null.");
        return new FilamentRendererAndroid(renderer, this);
    }

    public IFilamentScene CreateScene()
    {
        var scene = _engine.CreateScene()
            ?? throw new InvalidOperationException("Engine.CreateScene() returned null.");
        return new FilamentSceneAndroid(scene);
    }

    public IFilamentView CreateView()
    {
        var view = _engine.CreateView()
            ?? throw new InvalidOperationException("Engine.CreateView() returned null.");
        return new FilamentViewAndroid(view);
    }

    public IFilamentCamera CreateCamera()
    {
        var entityId = _engine.EntityManager.Create();
        var camera = _engine.CreateCamera(entityId)
            ?? throw new InvalidOperationException("Engine.CreateCamera() returned null.");
        return new FilamentCameraAndroid(camera, entityId, this);
    }

    public IFilamentSwapChain CreateSwapChain(object nativeSurface)
    {
        var surface = nativeSurface as Android.Views.Surface
            ?? throw new ArgumentException(
                "nativeSurface must be an Android.Views.Surface on Android.", nameof(nativeSurface));
        var swapChain = _engine.CreateSwapChain(surface, 0)
            ?? throw new InvalidOperationException("Engine.CreateSwapChain() returned null.");
        return new FilamentSwapChainAndroid(swapChain, this);
    }

    public IFilamentTransformManager TransformManager =>
        new FilamentTransformManagerAndroid(_engine.TransformManager);

    public IFilamentRenderableManager RenderableManager =>
        new FilamentRenderableManagerAndroid(_engine.RenderableManager);

    public IFilamentEntityManager EntityManager =>
        new FilamentEntityManagerAndroid(_engine.EntityManager);

    public void DestroyRenderer(IFilamentRenderer renderer)
    {
        ThrowIfDisposed();
        _engine.DestroyRenderer(((FilamentRendererAndroid)renderer)._renderer);
    }

    public void DestroyScene(IFilamentScene scene)
    {
        ThrowIfDisposed();
        _engine.DestroyScene(((FilamentSceneAndroid)scene)._scene);
    }

    public void DestroyView(IFilamentView view)
    {
        ThrowIfDisposed();
        _engine.DestroyView(((FilamentViewAndroid)view)._view);
    }

    public void DestroyCamera(IFilamentCamera camera)
    {
        ThrowIfDisposed();
        var cam = (FilamentCameraAndroid)camera;
        _engine.DestroyCameraComponent(cam._entityId);
        _engine.EntityManager.Destroy(cam._entityId);
    }

    public void DestroySwapChain(IFilamentSwapChain swapChain)
    {
        ThrowIfDisposed();
        _engine.DestroySwapChain(((FilamentSwapChainAndroid)swapChain)._swapChain);
    }

    public void DestroyMaterial(IFilamentMaterial material)
    {
        ThrowIfDisposed();
        _engine.DestroyMaterial(((FilamentMaterialAndroid)material)._material);
    }

    public void DestroyMaterialInstance(IFilamentMaterialInstance instance)
    {
        ThrowIfDisposed();
        _engine.DestroyMaterialInstance(((FilamentMaterialInstanceAndroid)instance)._instance);
    }

    public void DestroyTexture(IFilamentTexture texture)
    {
        ThrowIfDisposed();
        _engine.DestroyTexture(((FilamentTextureAndroid)texture)._texture);
    }

    public void DestroySkybox(IFilamentSkybox skybox)
    {
        ThrowIfDisposed();
        _engine.DestroySkybox(((FilamentSkyboxAndroid)skybox)._skybox);
    }

    public void DestroyIndirectLight(IFilamentIndirectLight ibl)
    {
        ThrowIfDisposed();
        _engine.DestroyIndirectLight(((FilamentIndirectLightAndroid)ibl)._ibl);
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
        // Properly tear down the native Filament engine before releasing the JNI peer.
        _engine.Destroy();
        // Properly tear down the native Filament engine before releasing the JNI peer.
        _engine.Destroy();
        _engine.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FilamentEngineAndroid));
    }
}
