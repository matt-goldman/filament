namespace Filament.Maui;

/// <summary>
/// Per-draw parameter overrides for a <see cref="IFilamentMaterial"/>.
/// Each renderable primitive references one MaterialInstance.
/// </summary>
public interface IFilamentMaterialInstance : IDisposable
{
    /// <summary>Sets a named float parameter on this material instance.</summary>
    void SetParameterFloat(string name, float value);

    /// <summary>Sets a named float4 (RGBA / vec4) parameter on this material instance.</summary>
    void SetParameterFloat4(string name, float x, float y, float z, float w);

    /// <summary>Sets a named sampler/texture parameter on this material instance.</summary>
    void SetParameterTexture(string name, IFilamentTexture texture);
}
