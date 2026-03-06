namespace Filament.Maui;

/// <summary>
/// Creates and destroys entity IDs. An entity is a 32-bit integer — it is NOT a class
/// with a native pointer. The same integer is used on both Android and iOS.
/// </summary>
public interface IFilamentEntityManager
{
    /// <summary>Creates a new entity ID. The entity has no components attached yet.</summary>
    int Create();

    /// <summary>Destroys an entity and releases its ID for reuse.</summary>
    void Destroy(int entity);
}
