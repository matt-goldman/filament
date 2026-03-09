using Filament.Maui;

namespace FilamentSample;

/// <summary>
/// Creates a single colored triangle entity in the Filament scene.
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
/// If the compiled material files are absent, the renderer logs a diagnostic and the
/// entity is still added to the scene so the clear-color background continues to render.
/// </remarks>
internal sealed class TriangleRenderer
{
    private readonly int _entity;
    private IFilamentMaterial? _material;
    private IFilamentMaterialInstance? _matInstance;

    // Interleaved vertex data: position (xyz) + color (rgba)
    private static readonly float[] TriangleVertices =
    {
        //  X       Y      Z      R     G     B     A
         0.0f,  0.5f,  0.0f,  1.0f, 0.0f, 0.0f, 1.0f,
        -0.5f, -0.5f,  0.0f,  0.0f, 1.0f, 0.0f, 1.0f,
         0.5f, -0.5f,  0.0f,  0.0f, 0.0f, 1.0f, 1.0f,
    };

    private static readonly ushort[] TriangleIndices = { 0, 1, 2 };

    /// <summary>
    /// Initialises a TriangleRenderer with pre-loaded material bytes.
    /// </summary>
    /// <param name="engine">The active Filament engine.</param>
    /// <param name="scene">The scene to add the entity to.</param>
    /// <param name="materialBytes">
    /// Compiled <c>.filamat</c> binary for the current platform.
    /// Pass an empty array when the material has not yet been compiled — the entity
    /// will still be added to the scene so the background colour renders.
    /// </param>
    public TriangleRenderer(IFilamentEngine engine, IFilamentScene scene, byte[] materialBytes)
    {
        _entity = engine.EntityManager.Create();

        if (materialBytes.Length > 0)
        {
            try
            {
                _material = FilamentMaterialLoader.LoadMaterial(engine, materialBytes);
                _matInstance = _material.CreateInstance();
                engine.RenderableManager.SetMaterialInstanceAt(_entity, 0, _matInstance);
            }
            catch (Exception ex)
            {
                // Material load failures are non-fatal: the scene background still renders.
                // Compile the .matc source with matc (see README.md) to enable the triangle.
                System.Diagnostics.Debug.WriteLine(
                    $"[TriangleRenderer] Material load failed — triangle will not render: {ex.Message}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine(
                "[TriangleRenderer] Material bytes are empty — compile default.matc with matc (see README.md).");
        }

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
