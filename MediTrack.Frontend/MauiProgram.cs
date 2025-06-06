using CommunityToolkit.Maui;
using MediTrack.Frontend.Services;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Vistas.PantallasPrincipales;
using MediTrack.Frontend.ViewModels.PantallasInicio;
using MediTrack.Frontend.Vistas.PantallasInicio;
using Microsoft.Extensions.Http; // Necesario para AddHttpClient
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using System.Globalization;
using System.Net.Http.Headers;
using ZXing.Net.Maui.Controls;

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



#if DEBUG
        builder.Logging.AddDebug();
#endif

        // CONFIGURACIÓN DE HTTPCLIENT Y SERVICIOS

        // 1. Registra el helper para la conexión HTTPS en desarrollo.
        builder.Services.AddSingleton<DevHttpsConnectionHelper>();

        // 2. Registra el HttpClient usando el helper.
        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            // Usa el helper para obtener el manejador que confía en el certificado de desarrollo.
            return sp.GetRequiredService<DevHttpsConnectionHelper>().GetPlatformSpecificHttpMessageHandler();
        });

        // 3. Registra el ApiService y su interfaz.
        builder.Services.AddSingleton<IApiService, ApiService>();

        // 4. Registra los ViewModels y Páginas.
        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<PantallaScan>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<PantallaInicioSesion>();


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
