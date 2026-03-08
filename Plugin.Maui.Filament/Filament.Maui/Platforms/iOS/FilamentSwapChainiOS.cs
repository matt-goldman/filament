using FilamentBinding.iOS;

namespace Filament.Maui;

/// <summary>
/// iOS implementation of <see cref="IFilamentSwapChain"/>.
/// Wraps <see cref="FLTSwapChain"/> from the iOS binding.
/// </summary>
internal sealed class FilamentSwapChainiOS : IFilamentSwapChain
{
    internal readonly FLTSwapChain _swapChain;
    private readonly FilamentEngineiOS _engine;

    internal FilamentSwapChainiOS(FLTSwapChain swapChain, FilamentEngineiOS engine)
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
