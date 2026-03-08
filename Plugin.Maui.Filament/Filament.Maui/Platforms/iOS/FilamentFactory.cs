using FilamentBinding.iOS;

namespace Filament.Maui;

/// <summary>
/// iOS static factory for creating an <see cref="IFilamentEngine"/>.
/// Call <see cref="CreateEngine"/> once at app startup, before any other Filament API.
/// </summary>
public static class FilamentFactory
{
    /// <summary>
    /// Creates a Filament engine using the Metal backend (recommended for iOS).
    /// Must be called on the render thread — all subsequent Filament calls must
    /// originate from the same thread.
    /// </summary>
    /// <returns>A new engine instance. The caller is responsible for disposing it.</returns>
    public static IFilamentEngine CreateEngine()
    {
        var engine = FLTEngine.CreateWithBackend(FLTBackend.Metal)
            ?? throw new InvalidOperationException("FLTEngine.CreateWithBackend() returned null.");
        return new FilamentEngineiOS(engine);
    }
}
