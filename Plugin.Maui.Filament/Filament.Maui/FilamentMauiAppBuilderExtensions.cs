using Microsoft.Maui.Hosting;

namespace Filament.Maui;

/// <summary>
/// Extension methods for <see cref="MauiAppBuilder"/> to register Filament services
/// and platform handlers for supported platforms (Android and iOS).
/// </summary>
public static class FilamentMauiAppBuilderExtensions
{
    /// <summary>
    /// Registers the <see cref="FilamentView"/> platform handler so that
    /// <see cref="FilamentView"/> controls render correctly on supported platforms
    /// (Android and iOS). Call this in <c>MauiProgram.cs</c>:
    /// <code>
    /// builder.UseFilament();
    /// </code>
    /// </summary>
    public static MauiAppBuilder UseFilament(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
#if ANDROID
            handlers.AddHandler<FilamentView, FilamentViewHandler>();
#elif IOS
            handlers.AddHandler<FilamentView, FilamentViewHandler>();
#endif
        });
        return builder;
    }
}
