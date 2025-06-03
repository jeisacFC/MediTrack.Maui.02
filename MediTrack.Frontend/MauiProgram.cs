using CommunityToolkit.Maui; // Necesario para el Community Toolkit
//using MediTrack.Frontend.Services;
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
        //#region
        //    .ConfigureMauiHandlers(h =>
        //    {
        //        h.AddHandler(typeof
        //            (ZXing.Net.Maui.Controls.CameraBarcodeReaderView),
        //            typeof(CameraBarcodeReaderViewHandIer));
        //        h.AddHandter(typeof
        //            (ZXing.Net.Maui.Controls.CameraView), typeof
        //            (CameraViewHandIer));
        //        h.AddHandter(typeof
        //            (ZXing.Net.Maui.Controls.BarcodeGeneratorView),
        //            typeof(BarcodeGeneratorViewHandIer));
        //    });
        //#endregion
        

        //// Registro de servicios (DEBE ir después de UseMauiApp)
        //builder.Services.AddSingleton<IBarcodeScannerService, BarcodeScannerService>();
        //builder.Services.AddTransient<ScanViewModel>();

     

#if DEBUG
        builder.Logging.AddDebug();
        #endif

        return builder.Build();
    }
}
