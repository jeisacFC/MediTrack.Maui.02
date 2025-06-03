using CommunityToolkit.Maui; // Necesario para el Community Toolkit
using MediTrack.Frontend.Services;
using MediTrack.Frontend.Vistas.PantallasFuncionales;
using MediTrack.Frontend.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls; // <---  para ZXing.Net.MAUI

namespace MediTrack.Frontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() //Activa el Community Toolkit
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        //// Registro de servicios (DEBE ir después de UseMauiApp)
        
        builder.Services.AddSingleton<IBarcodeScannerService, BarcodeScannerService>();
        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<PantallaScan>();


#if DEBUG
        builder.Logging.AddDebug();
        #endif

        return builder.Build();
    }
}
