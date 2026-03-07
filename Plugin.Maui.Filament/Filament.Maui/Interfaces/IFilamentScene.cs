namespace Filament.Maui;

/// <summary>
/// Flat container of renderable entities, lights, skybox and indirect light.
/// Entities are integer IDs — they are not class instances.
/// </summary>
public interface IFilamentScene : IDisposable
{
    /// <summary>
    /// Adds a renderable entity to the scene.
    /// The entity must have a renderable component attached via
    /// <see cref="IFilamentRenderableManager"/>.
    /// </summary>
    /// <param name="entity">Entity ID — an <see langword="int"/> on both platforms.</param>
    void AddEntity(int entity);

    /// <summary>Removes a previously added entity from the scene.</summary>
    void RemoveEntity(int entity);

    /// <summary>Sets the skybox (environment background). Pass <see langword="null"/> to clear.</summary>
    void SetSkybox(IFilamentSkybox? skybox);

    /// <summary>Sets the image-based lighting. Pass <see langword="null"/> to clear.</summary>
    void SetIndirectLight(IFilamentIndirectLight? ibl);
}
