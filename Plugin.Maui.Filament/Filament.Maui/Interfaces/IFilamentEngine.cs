namespace Filament.Maui;

/// <summary>
/// Cross-platform Filament engine — creates and destroys all GPU resources.
/// Entity is an <see langword="int"/> on both platforms
/// (Android: <c>int</c>; iOS: <c>uint32_t</c> wrapped as <c>int</c>).
/// </summary>
public interface IFilamentEngine : IDisposable
{
    /// <summary>Creates a new Renderer for this engine.</summary>
    IFilamentRenderer CreateRenderer();

    /// <summary>Creates a new Scene that holds renderable entities.</summary>
    IFilamentScene CreateScene();

    /// <summary>Creates a new View (viewport + scene + camera + post-processing).</summary>
    IFilamentView CreateView();

    /// <summary>Creates a new Camera entity.</summary>
    IFilamentCamera CreateCamera();

    /// <summary>
    /// Creates a SwapChain bound to a platform-native surface.
    /// Android: pass an <c>android.view.Surface</c> object.
    /// iOS: pass a handle to a <c>CAMetalLayer</c> via <c>ObjCRuntime.Runtime</c>.
    /// </summary>
    IFilamentSwapChain CreateSwapChain(object nativeSurface);

    /// <summary>The engine's TransformManager for entity position/rotation/scale.</summary>
    IFilamentTransformManager TransformManager { get; }

    /// <summary>The engine's RenderableManager for attaching mesh and material components.</summary>
    IFilamentRenderableManager RenderableManager { get; }

    /// <summary>The engine's EntityManager for creating and destroying entity IDs.</summary>
    IFilamentEntityManager EntityManager { get; }

    /// <summary>Destroys a Renderer created by this engine.</summary>
    void DestroyRenderer(IFilamentRenderer renderer);

    /// <summary>Destroys a Scene created by this engine.</summary>
    void DestroyScene(IFilamentScene scene);

    /// <summary>Destroys a View created by this engine.</summary>
    void DestroyView(IFilamentView view);

    /// <summary>Destroys a Camera created by this engine.</summary>
    void DestroyCamera(IFilamentCamera camera);

    /// <summary>Destroys a SwapChain created by this engine.</summary>
    void DestroySwapChain(IFilamentSwapChain swapChain);

    /// <summary>
    /// Blocks until all pending GPU work is complete.
    /// Must be called before resizing or destroying resources to prevent in-flight frame corruption.
    /// </summary>
    void FlushAndWait();
}
