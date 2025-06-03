using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui; // Necesario para CalendarView
using MediTrack.Frontend.Services; 
using MediTrack.Frontend.ViewModels; 

namespace MediTrack.Frontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() //Activa el Community Toolkit
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        // Registro de servicios (DEBE ir después de UseMauiApp)
        builder.Services.AddSingleton<IBarcodeScannerService, BarcodeScannerService>();
        builder.Services.AddTransient<ScanViewModels>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
