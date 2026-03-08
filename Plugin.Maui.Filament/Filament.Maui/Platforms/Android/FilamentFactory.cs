using Com.Google.Android.Filament;

namespace Filament.Maui;

/// <summary>
/// Android static factory for creating an <see cref="IFilamentEngine"/>.
/// Call <see cref="CreateEngine"/> once at app startup, before any other Filament API.
/// </summary>
public static class FilamentFactory
{
    private static readonly object _initLock = new();
    private static bool _initialized;

    /// <summary>
    /// Loads the Filament native library (<c>libfilament-jni.so</c>) and
    /// creates a new <see cref="IFilamentEngine"/> backed by the Java binding.
    /// </summary>
    /// <returns>A new engine instance. The caller is responsible for disposing it.</returns>
    public static IFilamentEngine CreateEngine()
    {
        lock (_initLock)
        {
            if (!_initialized)
            {
                Com.Google.Android.Filament.Filament.Init();
                _initialized = true;
            }
        }
        var javaEngine = Engine.Create()
            ?? throw new InvalidOperationException("Filament Engine.Create() returned null.");
        return new FilamentEngineAndroid(javaEngine);
    }
}
