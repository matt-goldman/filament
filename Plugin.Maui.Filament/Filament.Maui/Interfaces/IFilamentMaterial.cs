namespace Filament.Maui;

/// <summary>
/// A compiled Filament PBR material program. Materials are loaded from precompiled
/// <c>.mat</c> binary blobs (produced by the <c>matc</c> tool).
/// Use <see cref="CreateInstance"/> to get a per-draw parameter set.
/// </summary>
public interface IFilamentMaterial : IDisposable
{
    /// <summary>Creates a new <see cref="IFilamentMaterialInstance"/> for this material.</summary>
    IFilamentMaterialInstance CreateInstance();
}
