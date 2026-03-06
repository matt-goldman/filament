using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

/// <summary>
/// Android implementation of <see cref="IFilamentRenderer"/>.
/// Wraps <see cref="JFilament.Renderer"/> from the Java binding.
/// </summary>
internal sealed class FilamentRendererAndroid : IFilamentRenderer
{
    internal readonly JFilament.Renderer _renderer;
    private readonly FilamentEngineAndroid _engine;

    internal FilamentRendererAndroid(JFilament.Renderer renderer, FilamentEngineAndroid engine)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    public bool BeginFrame(IFilamentSwapChain swapChain)
    {
        var sc = ((FilamentSwapChainAndroid)swapChain)._swapChain;
        return _renderer.BeginFrame(sc, 0);
    }

    public void Render(IFilamentView view) =>
        _renderer.Render(((FilamentViewAndroid)view)._view);

    public void EndFrame() =>
        _renderer.EndFrame();

    public void SetClearColor(float r, float g, float b, float a)
    {
        var opts = _renderer.GetClearOptions();
        opts.ClearColor = new float[] { r, g, b, a };
        opts.Clear = true;
        _renderer.SetClearOptions(opts);
    }

    public void Dispose() =>
        _engine.DestroyRenderer(this);
}
