using CommunityToolkit.Maui;
using MediTrack.Frontend.Services;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Vistas.PantallasPrincipales;
using MediTrack.Frontend.ViewModels.PantallasInicio;
using MediTrack.Frontend.Vistas.PantallasInicio;
using MediTrack.Frontend.Popups;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using System.Globalization;
using System.Net.Http.Headers;
using ZXing.Net.Maui.Controls;
using MediTrack.Frontend.ViewModels;

namespace MediTrack.Frontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

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

        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(30);
        })
.ConfigurePrimaryHttpMessageHandler(() =>
{
#if DEBUG
    return new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
#else
    return new HttpClientHandler();
#endif
});

        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // TODOS LOS VIEWMODELS 
        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();
        builder.Services.AddTransient<OlvidoContrasenaViewModel>();
        builder.Services.AddTransient<CodigoVerificacionViewModel>();
        builder.Services.AddTransient<NuevaContrasenaViewModel>();

        // PANTALLAS 
        builder.Services.AddTransient<PantallaInicioSesion>();
        builder.Services.AddTransient<PantallaRegistro>();
        builder.Services.AddTransient<PantallaPerfil>();
        builder.Services.AddTransient<PantallaScan>();
        builder.Services.AddTransient<PantallaOlvidoContrasena>();

        //mODALES
        builder.Services.AddTransient<ModalCodigoVerificacion>();
        builder.Services.AddTransient<ModalNuevaContrasena>();

        var app = builder.Build();

        // OPTIMIZACIÓN: Solo las operaciones más pesadas en background
        Task.Run(() =>
        {
            try
            {
                // Syncfusion en background con delay mínimo
                Thread.Sleep(100); // 100ms delay para que la UI aparezca
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzg5NTQ4MEAzMjM0MmUzMDJlMzBVbStPWjNqWUtHSTdwM2grYTB3Z2s5ZUtpNjhoZ0V5SlEzZFBvVnRuT0U4PQ==");
                System.Diagnostics.Debug.WriteLine("Syncfusion configurado en background");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Syncfusion background: {ex.Message}");
            }
        });

        // Cultura inmediata pero optimizada
        ConfigurarCulturaEspañola();

        return app;
    }

    private static void ConfigurarCulturaEspañola()
    {
        try
        {
            var cultura = new CultureInfo("es-ES");
            CultureInfo.CurrentCulture = cultura;
            CultureInfo.CurrentUICulture = cultura;
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