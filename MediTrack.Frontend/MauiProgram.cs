using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Syncfusion.Maui.Core.Hosting;
using System.Globalization;
using CommunityToolkit.Maui; // Necesario para el Community Toolkit
using MediTrack.Frontend.Vistas.PantallasFuncionales;
using MediTrack.Frontend.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Services.Interfaces; // <---  para ZXing.Net.MAUI

namespace MediTrack.Frontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            //NUEVA LICENCIA ESPECÍFICA PARA MAUI 24.x

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzg5NTQ4MEAzMjM0MmUzMDJlMzBVbStPWjNqWUtHSTdwM2grYTB3Z2s5ZUtpNjhoZ0V5SlEzZFBvVnRuT0U4PQ==");
            System.Diagnostics.Debug.WriteLine("Nueva licencia 24.x registrada");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        //CONFIGURAR CULTURA ESPAÑOLA AL INICIO
        ConfigurarCulturaEspañola();

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()

            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
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

    private static void ConfigurarCulturaEspañola()
    {
        try
        {
            // Configurar cultura española para toda la aplicación
            var cultura = new CultureInfo("es-ES");

            // Configurar para el hilo actual
            CultureInfo.CurrentCulture = cultura;
            CultureInfo.CurrentUICulture = cultura;

            // Configurar por defecto para nuevos hilos
            CultureInfo.DefaultThreadCurrentCulture = cultura;
            CultureInfo.DefaultThreadCurrentUICulture = cultura;

            System.Diagnostics.Debug.WriteLine("Cultura española configurada");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error configurando cultura: {ex.Message}");
        }
    }
}
