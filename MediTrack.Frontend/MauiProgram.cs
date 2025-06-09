using CommunityToolkit.Maui;
using MediTrack.Frontend.Popups;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels;
using MediTrack.Frontend.ViewModels.PantallasInicio;
using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Vistas.PantallasInicio;
using MediTrack.Frontend.Vistas.PantallasPrincipales;
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

        builder.Logging.AddDebug();

        // Registrar el AuthHandler
        builder.Services.AddTransient<AuthHandler>();

        // Configurar HttpClient con el AuthHandler
        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
        })
        .AddHttpMessageHandler<AuthHandler>(); 

        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // TODOS LOS VIEWMODELS PRINCIPALES
        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<BusquedaViewModel>();
        builder.Services.AddTransient<InicioViewModel>();
        builder.Services.AddTransient<AgendaViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();

        // TODOS LOS VIEWMODELS INICIALES
        builder.Services.AddTransient<CargaViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<OlvidoContrasenaViewModel>();
        builder.Services.AddTransient<RecuperarContrasenaViewModel>();
        // PANTALLAS PRINCIPALES
        builder.Services.AddTransient<PantallaScan>();
        builder.Services.AddTransient<PantallaBusqueda>();
        builder.Services.AddTransient<PantallaInicio>();
        builder.Services.AddTransient<PantallaAgenda>();        
        builder.Services.AddTransient<PantallaPerfil>();



        // PANTALLAS INICIALES
        builder.Services.AddTransient<PantallaCarga>();
        builder.Services.AddTransient<PantallaInicioSesion>();
        builder.Services.AddTransient<PantallaRegistro>();
        builder.Services.AddTransient<PantallaOlvidoContrasena>();

        //MODALES
        builder.Services.AddTransient<ModalAgregarEvento>();
        builder.Services.AddTransient<ModalRecuperarContrasena>();


        var app = builder.Build();

        Task.Run(() =>
        {
            try
            {
                Thread.Sleep(100);
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzg5NTQ4MEAzMjM0MmUzMDJlMzBVbStPWjNqWUtHSTdwM2grYTB3Z2s5ZUtpNjhoZ0V5SlEzZFBvVnRuT0U4PQ==");
                System.Diagnostics.Debug.WriteLine("Syncfusion configurado en background");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Syncfusion background: {ex.Message}");
            }
        });

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