using Filament.Maui;
using Microsoft.Extensions.Logging;

namespace FilamentSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseFilament();   // Registers FilamentViewHandler for Android and iOS

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
