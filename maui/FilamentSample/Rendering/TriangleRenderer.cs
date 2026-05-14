using Filament.Maui;

namespace FilamentSample;

/// <summary>
/// Creates a triangle entity with a colored material in the Filament scene.
/// Uses platform-compiled materials loaded from app resources.
/// </summary>
/// <remarks>
/// Material binaries must be compiled from <c>Materials/default.matc</c> using the
/// <c>matc</c> tool (included in the Filament release package) and placed at:
/// <list type="bullet">
///   <item><description><c>Resources/Raw/materials/default.mat.android</c> — OpenGL ES + Vulkan</description></item>
///   <item><description><c>Resources/Raw/materials/default.mat.ios</c> — Metal</description></item>
/// </list>
/// See <c>README.md</c> for compilation instructions.
/// <para>
/// Full renderable construction (vertex + index buffer geometry) requires builder APIs
/// not yet exposed in <see cref="IFilamentRenderableManager"/>.
/// On Android the material instance is assigned via <c>SetMaterialInstanceAt</c> and the
/// entity is added to the scene.
/// On iOS, runtime material assignment is not supported via the current cross-platform
/// interface (<c>FLTRenderableManagerBuilder.MaterialAtIndex</c> is needed but not yet
/// exposed), so the entity is not added to the scene; the clear-color background still renders.
/// </para>
/// </remarks>
internal sealed class TriangleRenderer
{
    private readonly int _entity;
    private IFilamentMaterial? _material;
    private IFilamentMaterialInstance? _matInstance;

    /// <summary>
    /// Initialises a TriangleRenderer with pre-loaded material bytes.
    /// </summary>
    /// <param name="engine">The active Filament engine.</param>
    /// <param name="scene">The scene to add the entity to.</param>
    /// <param name="materialBytes">
    /// Compiled <c>.filamat</c> binary for the current platform, or an empty array when
    /// the material has not yet been compiled (the entity will not be added to the scene).
    /// </param>
    public TriangleRenderer(IFilamentEngine engine, IFilamentScene scene, byte[] materialBytes)
    {
        _entity = engine.EntityManager.Create();

        bool renderableReady = false;

        if (materialBytes.Length > 0)
        {
            try
            {
                _material = FilamentMaterialLoader.LoadMaterial(engine, materialBytes);
                _matInstance = _material.CreateInstance();

                // SetMaterialInstanceAt is only supported on Android via the current
                // cross-platform interface.  On iOS, materials must be assigned at
                // renderable construction time using FLTRenderableManagerBuilder.MaterialAtIndex(),
                // which is not yet exposed in IFilamentRenderableManager.
#if ANDROID
                engine.RenderableManager.SetMaterialInstanceAt(_entity, 0, _matInstance);
                renderableReady = true;
#endif
            }
            catch (Exception ex)
            {
                // Material load failures are non-fatal: the scene background still renders.
                // Compile the .matc source with matc (see README.md) to enable the triangle.
                System.Diagnostics.Debug.WriteLine(
                    $"[TriangleRenderer] Material load failed — entity will not be added to the scene: {ex.Message}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine(
                "[TriangleRenderer] Material bytes are empty — compile default.matc with matc (see README.md).");
        }

        // Only add the entity to the scene once its renderable component is fully configured.
        if (renderableReady)
            scene.AddEntity(_entity);
    }

    public void Dispose(IFilamentEngine engine)
    {
        engine.EntityManager.Destroy(_entity);
        if (_matInstance is not null) engine.DestroyMaterialInstance(_matInstance);
        if (_material is not null)    engine.DestroyMaterial(_material);
    }

    /// <summary>
    /// Asynchronously loads the platform-appropriate compiled material binary.
    /// </summary>
    public static async Task<byte[]> LoadMaterialBytesAsync()
    {
        // Choose the platform-appropriate compiled material binary (.filamat format).
#if ANDROID
        const string assetPath = "materials/default.mat.android";
#elif IOS
        const string assetPath = "materials/default.mat.ios";
#else
        return [];
#endif
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(assetPath);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[TriangleRenderer] Could not open material asset '{assetPath}': {ex.Message}");
            return [];
        }
    }
}
