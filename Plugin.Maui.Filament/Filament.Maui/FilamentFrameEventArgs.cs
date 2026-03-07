namespace Filament.Maui;

/// <summary>
/// Event arguments supplied to <see cref="FilamentView.FrameRendering"/>.
/// Provides the renderer and view objects for the current frame so consumers
/// can submit draw calls or update material parameters.
/// </summary>
public sealed class FilamentFrameEventArgs : EventArgs
{
    /// <summary>
    /// The active renderer associated with the current frame.
    /// Exposed to <see cref="FilamentView.FrameRendering"/> handlers before
    /// <see cref="IFilamentRenderer.BeginFrame"/> is called so callers can
    /// configure per-frame state (for example, clear color) prior to rendering.
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
