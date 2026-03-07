namespace Filament.Maui;

/// <summary>
/// Attaches position, rotation, and scale components to entities via 4×4 column-major
/// transform matrices. Entities must be created before adding a transform component.
/// </summary>
public interface IFilamentTransformManager
{
    /// <summary>
    /// Creates a transform component for the given entity (sets it to the identity transform).
    /// Must be called before <see cref="SetTransform"/>.
    /// </summary>
    void Create(int entity);

    /// <summary>
    /// Sets the local transform of an entity using a 4×4 column-major matrix.
    /// The array must have exactly 16 elements (mat4 in column-major order).
    /// </summary>
    void SetTransform(int entity, float[] mat4ColumnMajor);
}
