using Java.Nio;

namespace Com.Google.Android.Filament;

/// <summary>
/// C#-friendly extension methods for Filament Android binding classes.
/// </summary>
public static class MaterialExtensions
{
    /// <summary>
    /// Creates a Material from a raw byte array (e.g., loaded from Android Assets).
    /// Wraps the byte array in a <see cref="ByteBuffer"/> and calls
    /// <see cref="Material.Builder.Payload"/> before building.
    /// </summary>
    /// <param name="builder">The material builder instance.</param>
    /// <param name="engine">The Filament engine to build the material for.</param>
    /// <param name="materialData">
    /// Compiled .mat file bytes — must be compiled for the target backend (OpenGL/Vulkan).
    /// </param>
    /// <returns>The built <see cref="Material"/>, or null if the data is invalid.</returns>
    public static Material? BuildFromBytes(
        this Material.Builder builder,
        Engine engine,
        byte[] materialData)
    {
        var buffer = ByteBuffer.Wrap(materialData)!;
        return builder.Payload(buffer, materialData.Length).Build(engine);
    }
}
