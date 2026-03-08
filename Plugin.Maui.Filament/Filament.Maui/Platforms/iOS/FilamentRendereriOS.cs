using FilamentBinding.iOS;

namespace Filament.Maui;

/// <summary>
/// iOS implementation of <see cref="IFilamentRenderer"/>.
/// Wraps <see cref="FLTRenderer"/> from the iOS binding.
/// </summary>
internal sealed class FilamentRendereriOS : IFilamentRenderer
{
    internal readonly FLTRenderer _renderer;
    private readonly FilamentEngineiOS _engine;
    private bool _disposed;

    internal FilamentRendereriOS(FLTRenderer renderer, FilamentEngineiOS engine)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    public bool BeginFrame(IFilamentSwapChain swapChain) =>
        _renderer.BeginFrame(((FilamentSwapChainiOS)swapChain)._swapChain);

    public void Render(IFilamentView view) =>
        _renderer.Render(((FilamentViewiOS)view)._view);

    public void EndFrame() => _renderer.EndFrame();

    public void SetClearColor(float r, float g, float b, float a) =>
        _renderer.SetClearColor(r, g, b, a);

    /// <remarks>Destroyed via <see cref="IFilamentEngine.DestroyRenderer"/>.</remarks>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _engine.DestroyRenderer(this);
    }
}
