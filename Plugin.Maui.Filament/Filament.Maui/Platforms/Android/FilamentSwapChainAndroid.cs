using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

/// <summary>
/// Android implementation of <see cref="IFilamentSwapChain"/>.
/// Wraps <see cref="JFilament.SwapChain"/> from the Java binding.
/// </summary>
internal sealed class FilamentSwapChainAndroid : IFilamentSwapChain
{
    internal readonly JFilament.SwapChain _swapChain;
    private readonly FilamentEngineAndroid _engine;

    internal FilamentSwapChainAndroid(JFilament.SwapChain swapChain, FilamentEngineAndroid engine)
    {
        _swapChain = swapChain ?? throw new ArgumentNullException(nameof(swapChain));
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    /// <summary>
    /// Returns the <see cref="IFilamentEngine"/> that owns this swap chain.
    /// Always destroy a swap chain via the same engine that created it.
    /// </summary>
    internal IFilamentEngine Engine => _engine;

    /// <remarks>Destroyed via <see cref="IFilamentEngine.DestroySwapChain"/>.</remarks>
    public void Dispose() { }
}
