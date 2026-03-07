namespace Filament.Maui;

/// <summary>
/// Event arguments supplied to <see cref="FilamentView.FrameRendering"/>.
/// Provides the renderer and view objects for the current frame so consumers
/// can submit draw calls or update material parameters.
/// </summary>
public sealed class FilamentFrameEventArgs : EventArgs
{
    /// <summary>
    /// The renderer for the current frame.
    /// Raised before <see cref="IFilamentRenderer.BeginFrame"/> so that consumers
    /// can update scene/camera state and material parameters before rendering begins.
    /// </summary>
    public IFilamentRenderer Renderer { get; }

    /// <summary>
    /// The Filament view configured for the current frame.
    /// </summary>
    public IFilamentView View { get; }

    /// <summary>
    /// Initialises a new <see cref="FilamentFrameEventArgs"/>.
    /// </summary>
    /// <param name="renderer">The active renderer.</param>
    /// <param name="view">The active view.</param>
    public FilamentFrameEventArgs(IFilamentRenderer renderer, IFilamentView view)
    {
        Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        View = view ?? throw new ArgumentNullException(nameof(view));
    }
}
