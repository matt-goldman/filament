using FilamentBinding.iOS;

namespace Filament.Maui;

/// <summary>
/// iOS implementation of <see cref="IFilamentScene"/>.
/// Wraps <see cref="FLTScene"/> from the iOS binding.
/// </summary>
internal sealed class FilamentSceneiOS : IFilamentScene
{
    internal readonly FLTScene _scene;

    internal FilamentSceneiOS(FLTScene scene) =>
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));

    // Entity IDs are uint in the FLT* API and int in the cross-platform interface.
    public void AddEntity(int entity)
    {
        if (entity < 0)
            throw new ArgumentOutOfRangeException(nameof(entity), "Entity ID must be non-negative.");

        _scene.AddEntity((uint)entity);
    }

    public void RemoveEntity(int entity)
    {
        if (entity < 0)
            throw new ArgumentOutOfRangeException(nameof(entity), "Entity ID must be non-negative.");

        _scene.RemoveEntity((uint)entity);
    }
    public void SetSkybox(IFilamentSkybox? skybox)
    {
        _scene.SetSkybox(skybox is null ? null : ((FilamentSkyboxiOS)skybox)._skybox);
    }

    public void SetIndirectLight(IFilamentIndirectLight? ibl)
    {
        _scene.SetIndirectLight(ibl is null ? null : ((FilamentIndirectLightiOS)ibl)._ibl);
    }

    /// <remarks>Destroyed via <see cref="IFilamentEngine.DestroyScene"/>.</remarks>
    public void Dispose() { }
}

/// <summary>iOS implementation of <see cref="IFilamentSkybox"/>.</summary>
internal sealed class FilamentSkyboxiOS : IFilamentSkybox
{
    internal readonly FLTSkybox _skybox;
    private readonly FilamentEngineiOS _ownerEngine;
    private bool _disposed;

    internal FilamentSkyboxiOS(FLTSkybox skybox, FilamentEngineiOS ownerEngine)
    {
        _skybox = skybox ?? throw new ArgumentNullException(nameof(skybox));
        _ownerEngine = ownerEngine ?? throw new ArgumentNullException(nameof(ownerEngine));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _ownerEngine._engine.DestroySkybox(_skybox);
    }
}

/// <summary>iOS implementation of <see cref="IFilamentIndirectLight"/>.</summary>
internal sealed class FilamentIndirectLightiOS : IFilamentIndirectLight
{
    internal readonly FLTIndirectLight _ibl;
    private readonly FilamentEngineiOS _ownerEngine;
    private bool _disposed;

    internal FilamentIndirectLightiOS(FLTIndirectLight ibl, FilamentEngineiOS ownerEngine)
    {
        _ibl = ibl ?? throw new ArgumentNullException(nameof(ibl));
        _ownerEngine = ownerEngine ?? throw new ArgumentNullException(nameof(ownerEngine));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _ownerEngine._engine.DestroyIndirectLight(_ibl);
    }
}
