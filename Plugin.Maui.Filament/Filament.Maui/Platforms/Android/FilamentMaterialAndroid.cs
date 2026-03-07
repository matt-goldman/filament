using Java.Nio;
using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

/// <summary>
/// Android implementation of <see cref="IFilamentMaterial"/>.
/// Wraps <see cref="JFilament.Material"/> from the Java binding.
/// </summary>
internal sealed class FilamentMaterialAndroid : IFilamentMaterial
{
    internal readonly JFilament.Material _material;
    private readonly FilamentEngineAndroid _ownerEngine;

    internal FilamentMaterialAndroid(JFilament.Material material, FilamentEngineAndroid ownerEngine)
    {
        _material = material ?? throw new ArgumentNullException(nameof(material));
        _ownerEngine = ownerEngine ?? throw new ArgumentNullException(nameof(ownerEngine));
    }

    public IFilamentMaterialInstance CreateInstance()
    {
        var instance = _material.CreateInstance()
            ?? throw new InvalidOperationException("Material.CreateInstance() returned null.");
        return new FilamentMaterialInstanceAndroid(instance, _ownerEngine);
    }

    public void Dispose() => _ownerEngine._engine.DestroyMaterial(_material);
}

/// <summary>
/// Android implementation of <see cref="IFilamentMaterialInstance"/>.
/// Wraps <see cref="JFilament.MaterialInstance"/> from the Java binding.
/// </summary>
internal sealed class FilamentMaterialInstanceAndroid : IFilamentMaterialInstance
{
    internal readonly JFilament.MaterialInstance _instance;
    private readonly FilamentEngineAndroid _ownerEngine;

    // Reuse a default sampler for this instance to avoid per-call allocation on hot paths
    // (e.g. per-frame material updates). Instance-level to avoid cross-thread mutability concerns.
    private readonly JFilament.TextureSampler _defaultSampler = new JFilament.TextureSampler();

    internal FilamentMaterialInstanceAndroid(JFilament.MaterialInstance instance, FilamentEngineAndroid ownerEngine)
    {
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        _ownerEngine = ownerEngine ?? throw new ArgumentNullException(nameof(ownerEngine));
    }

    public void SetParameterFloat(string name, float value) =>
        _instance.SetParameter(name, value);

    public void SetParameterFloat4(string name, float x, float y, float z, float w) =>
        _instance.SetParameter(name, x, y, z, w);

    public void SetParameterTexture(string name, IFilamentTexture texture) =>
        _instance.SetParameter(
            name,
            ((FilamentTextureAndroid)texture)._texture,
            _defaultSampler);

    public void Dispose() => _ownerEngine._engine.DestroyMaterialInstance(_instance);
}

/// <summary>
/// Android implementation of <see cref="IFilamentTexture"/>.
/// Wraps <see cref="JFilament.Texture"/> from the Java binding.
/// </summary>
internal sealed class FilamentTextureAndroid : IFilamentTexture
{
    internal readonly JFilament.Texture _texture;
    private readonly FilamentEngineAndroid _ownerEngine;

    internal FilamentTextureAndroid(JFilament.Texture texture, FilamentEngineAndroid ownerEngine)
    {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _ownerEngine = ownerEngine ?? throw new ArgumentNullException(nameof(ownerEngine));
    }

    public void Dispose() => _ownerEngine._engine.DestroyTexture(_texture);
}

/// <summary>
/// Helper for loading compiled Filament materials from byte arrays on Android.
/// </summary>
public static class FilamentMaterialLoader
{
    /// <summary>
    /// Creates a <see cref="IFilamentMaterial"/> from a compiled Filament material binary
    /// (<c>.filamat</c> file) loaded as a byte array (e.g., from Android Assets via
    /// <c>AssetManager</c>).
    /// </summary>
    /// <param name="engine">The Filament engine to build the material for.</param>
    /// <param name="matData">
    /// Bytes of a compiled Filament material (<c>.filamat</c>) — must target the Android
    /// backend (OpenGL/Vulkan). Use the <c>matc</c> tool to compile <c>.mat</c> source
    /// files into <c>.filamat</c> binaries.
    /// </param>
    /// <returns>A new <see cref="IFilamentMaterial"/> owned by the caller.</returns>
    public static IFilamentMaterial LoadMaterial(IFilamentEngine engine, byte[] matData)
    {
        if (engine is null) throw new ArgumentNullException(nameof(engine));
        if (matData is null) throw new ArgumentNullException(nameof(matData));
        if (matData.Length == 0) throw new ArgumentException("Material data must not be empty.", nameof(matData));

        var jEngine = ((FilamentEngineAndroid)engine)._engine;
        var buffer = ByteBuffer.Wrap(matData)!;
        var material = new JFilament.Material.Builder()
            .Payload(buffer, matData.Length)
            .Build(jEngine)
            ?? throw new InvalidOperationException("Material.Builder.Build() returned null.");
        return new FilamentMaterialAndroid(material, (FilamentEngineAndroid)engine);
    }
}
