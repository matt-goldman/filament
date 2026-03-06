namespace Filament.Maui;

/// <summary>
/// Attaches geometry and material components to entities, making them visible in a scene.
/// Geometry is attached via the platform-specific builder pattern.
/// </summary>
public interface IFilamentRenderableManager
{
    /// <summary>
    /// Overrides the material instance for a specific primitive on an entity.
    /// </summary>
    /// <param name="entity">The entity whose renderable component to update.</param>
    /// <param name="primitiveIndex">Zero-based index of the primitive to update.</param>
    /// <param name="instance">The new material instance to apply.</param>
    void SetMaterialInstanceAt(int entity, int primitiveIndex, IFilamentMaterialInstance instance);
}
