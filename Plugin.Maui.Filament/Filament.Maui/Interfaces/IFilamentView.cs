namespace Filament.Maui;

/// <summary>
/// Defines a viewport and links a <see cref="IFilamentScene"/> and
/// <see cref="IFilamentCamera"/> for rendering. Also controls post-processing.
/// </summary>
public interface IFilamentView : IDisposable
{
    /// <summary>Sets the scene this view will render.</summary>
    void SetScene(IFilamentScene scene);

    /// <summary>Sets the camera used to render this view.</summary>
    void SetCamera(IFilamentCamera camera);

    /// <summary>
    /// Sets the viewport dimensions in physical pixels.
    /// Call after <c>FlushAndWait()</c> when the surface is resized.
    /// </summary>
    void SetViewport(int left, int bottom, int width, int height);

    /// <summary>Enables or disables post-processing (tone mapping, AA, bloom, etc.).</summary>
    void SetPostProcessingEnabled(bool enabled);
}
