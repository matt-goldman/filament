using FilamentBinding.iOS;
using Foundation;

namespace Filament.Maui;

/// <summary>
/// iOS implementation of <see cref="IFilamentMaterial"/>.
/// Wraps <see cref="FLTMaterial"/> from the iOS binding.
/// </summary>
internal sealed class FilamentMaterialiOS : IFilamentMaterial
{
    internal readonly FLTMaterial _material;
    private readonly FilamentEngineiOS _ownerEngine;
    private bool _disposed;

    internal FilamentMaterialiOS(FLTMaterial material, FilamentEngineiOS ownerEngine)
    {
        _material = material ?? throw new ArgumentNullException(nameof(material));
        _ownerEngine = ownerEngine ?? throw new ArgumentNullException(nameof(ownerEngine));
    }

    public IFilamentMaterialInstance CreateInstance()
    {
        var instance = _material.CreateInstance()
            ?? throw new InvalidOperationException("FLTMaterial.CreateInstance() returned null.");
        return new FilamentMaterialInstanceiOS(instance, _ownerEngine);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _ownerEngine._engine.DestroyMaterial(_material);
    }
}

/// <summary>
/// iOS implementation of <see cref="IFilamentMaterialInstance"/>.
/// Wraps <see cref="FLTMaterialInstance"/> from the iOS binding.
/// </summary>
internal sealed class FilamentMaterialInstanceiOS : IFilamentMaterialInstance
{
    internal readonly FLTMaterialInstance _instance;
    private readonly FilamentEngineiOS _ownerEngine;
    private bool _disposed;

    internal FilamentMaterialInstanceiOS(FLTMaterialInstance instance, FilamentEngineiOS ownerEngine)
    {
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        _ownerEngine = ownerEngine ?? throw new ArgumentNullException(nameof(ownerEngine));
    }

    public void SetParameterFloat(string name, float value) =>
        _instance.SetFloatParameter(name, value);

    public void SetParameterFloat4(string name, float x, float y, float z, float w) =>
        _instance.SetFloat4Parameter(name, new VectorFloat4(x, y, z, w));

    public void SetParameterTexture(string name, IFilamentTexture texture) =>
        _instance.SetTextureParameter(name, ((FilamentTextureiOS)texture)._texture);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _ownerEngine._engine.DestroyMaterialInstance(_instance);
    }
}

/// <summary>
/// iOS implementation of <see cref="IFilamentTexture"/>.
/// Wraps <see cref="FLTTexture"/> from the iOS binding.
/// </summary>
internal sealed class FilamentTextureiOS : IFilamentTexture
{
    internal readonly FLTTexture _texture;
    private readonly FilamentEngineiOS _ownerEngine;
    private bool _disposed;

    internal FilamentTextureiOS(FLTTexture texture, FilamentEngineiOS ownerEngine)
    {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _ownerEngine = ownerEngine ?? throw new ArgumentNullException(nameof(ownerEngine));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _ownerEngine._engine.DestroyTexture(_texture);
    }
}

/// <summary>
/// Helper for loading compiled Filament materials from byte arrays on iOS.
/// </summary>
public static class FilamentMaterialLoader
{
    /// <summary>
    /// Creates a <see cref="IFilamentMaterial"/> from a compiled Filament material binary
    /// (<c>.filamat</c> file) loaded as a byte array (e.g., from the app bundle via
    /// <c>NSBundle.MainBundle</c>).
    /// </summary>
    /// <param name="engine">The Filament engine to build the material for.</param>
    /// <param name="matData">
    /// Bytes of a compiled Filament material (<c>.filamat</c>) — must target the Metal
    /// backend. Use the <c>matc</c> tool to compile <c>.mat</c> source files into
    /// <c>.filamat</c> binaries.
    /// </param>
    /// <returns>A new <see cref="IFilamentMaterial"/> owned by the caller.</returns>
    public static IFilamentMaterial LoadMaterial(IFilamentEngine engine, byte[] matData)
    {
        if (engine is null) throw new ArgumentNullException(nameof(engine));
        if (matData is null) throw new ArgumentNullException(nameof(matData));
        if (matData.Length == 0) throw new ArgumentException("Material data must not be empty.", nameof(matData));

        if (engine is not FilamentEngineiOS iosEngine)
            throw new ArgumentException("On iOS, FilamentMaterialLoader requires a FilamentEngineiOS instance.", nameof(engine));

        var nsData = NSData.FromArray(matData)
            ?? throw new InvalidOperationException("NSData.FromArray() returned null.");
        var material = FLTMaterial.BuildWithEngine(iosEngine._engine, nsData)
            ?? throw new InvalidOperationException("FLTMaterial.BuildWithEngine() returned null.");
        return new FilamentMaterialiOS(material, iosEngine);
    }
}
