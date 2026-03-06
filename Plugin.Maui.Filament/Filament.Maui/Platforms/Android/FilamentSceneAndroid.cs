using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

/// <summary>
/// Android implementation of <see cref="IFilamentScene"/>.
/// Wraps <see cref="JFilament.Scene"/> from the Java binding.
/// </summary>
internal sealed class FilamentSceneAndroid : IFilamentScene
{
    internal readonly JFilament.Scene _scene;

    internal FilamentSceneAndroid(JFilament.Scene scene) =>
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));

    public void AddEntity(int entity) =>
        _scene.AddEntity(entity);

    public void RemoveEntity(int entity) =>
        _scene.RemoveEntity(entity);

    public void SetSkybox(IFilamentSkybox? skybox) =>
        _scene.Skybox = skybox is null ? null : ((FilamentSkyboxAndroid)skybox)._skybox;

    public void SetIndirectLight(IFilamentIndirectLight? ibl) =>
        _scene.IndirectLight = ibl is null ? null : ((FilamentIndirectLightAndroid)ibl)._ibl;

    /// <remarks>Destroyed via <see cref="IFilamentEngine.DestroyScene"/>.</remarks>
    public void Dispose() { }
}

/// <summary>Android implementation of <see cref="IFilamentSkybox"/>.</summary>
internal sealed class FilamentSkyboxAndroid : IFilamentSkybox
{
    internal readonly JFilament.Skybox _skybox;
    internal FilamentSkyboxAndroid(JFilament.Skybox skybox) =>
        _skybox = skybox ?? throw new ArgumentNullException(nameof(skybox));
    public void Dispose() { }
}

/// <summary>Android implementation of <see cref="IFilamentIndirectLight"/>.</summary>
internal sealed class FilamentIndirectLightAndroid : IFilamentIndirectLight
{
    internal readonly JFilament.IndirectLight _ibl;
    internal FilamentIndirectLightAndroid(JFilament.IndirectLight ibl) =>
        _ibl = ibl ?? throw new ArgumentNullException(nameof(ibl));
    public void Dispose() { }
}
