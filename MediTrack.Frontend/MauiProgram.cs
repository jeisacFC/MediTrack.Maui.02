using CommunityToolkit.Maui;
using MediTrack.Frontend.Popups;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels;
using MediTrack.Frontend.ViewModels.PantallasInicio;
using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Vistas.PantallasInicio;
using MediTrack.Frontend.Vistas.PantallasPrincipales;
using MediTrack.Frontend.Services;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using System.Globalization;
using System.Net.Http.Headers;

namespace MediTrack.Frontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // CONFIGURAR CULTURA ANTES DE TODO
        ConfigurarCulturaEspañola();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore() // ✅ DEBE ESTAR AQUÍ - NO EN BACKGROUND
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        // LOGGING MÍNIMO PARA MEJOR RENDIMIENTO
#if DEBUG
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
#endif

        // === SERVICIOS CRÍTICOS COMO SINGLETON ===
        builder.Services.AddSingleton<AuthHandler>();
        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // === HTTPCLIENT SÚPER OPTIMIZADO ===
        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "MediTrack-Mobile/1.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new DevHttpsConnectionHelper().GetPlatformSpecificHttpMessageHandler())
        .AddHttpMessageHandler<AuthHandler>();

        // === VIEWMODELS CRÍTICOS ===
        builder.Services.AddTransient<CargaViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<OlvidoContrasenaViewModel>();
        builder.Services.AddTransient<RecuperarContrasenaViewModel>();

        // === VIEWMODELS PRINCIPALES ===
        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<BusquedaViewModel>();
        builder.Services.AddTransient<InicioViewModel>();
        builder.Services.AddTransient<AgendaViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();
        builder.Services.AddTransient<CondicionesMedicasViewModel>();
        builder.Services.AddTransient<AlergiasViewModel>();
        builder.Services.AddTransient<AgregarEventoViewModel>();
        builder.Services.AddTransient<GestionarSintomasViewModel>();
        builder.Services.AddTransient<ActualizarPerfilPopupViewModel>();

        // === PANTALLAS ===
        builder.Services.AddTransient<PantallaCarga>();
        builder.Services.AddTransient<PantallaInicioSesion>();
        builder.Services.AddTransient<PantallaRegistro>();
        builder.Services.AddTransient<PantallaOlvidoContrasena>();
        builder.Services.AddTransient<PantallaScan>();
        builder.Services.AddTransient<PantallaBusqueda>();
        builder.Services.AddTransient<PantallaInicio>();
        builder.Services.AddTransient<PantallaAgenda>();
        builder.Services.AddTransient<PantallaPerfil>();

        // === MODALES ===
        builder.Services.AddTransient<ModalAgregarEvento>();
        builder.Services.AddTransient<ActualizarPerfilPopup>();
        builder.Services.AddTransient<ModalRecuperarContrasena>();
        builder.Services.AddTransient<GestionCondicionesMedicasPopup>();
        builder.Services.AddTransient<GestionAlergiasPopup>();
        builder.Services.AddTransient<ModalGestionarSintomas>();

        var app = builder.Build();

        // === INICIALIZACIÓN DIFERIDA SOLO DE LA LICENCIA ===
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000); // Esperar que la app termine de cargar

                // SOLO la licencia en background - no la configuración del handler
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzg5NTQ4MEAzMjM0MmUzMDJlMzBVbStPWjNqWUtHSTdwM2grYTB3Z2s5ZUtpNjhoZ0V5SlEzZFBvVnRuT0U4PQ==");
                System.Diagnostics.Debug.WriteLine("✅ Licencia Syncfusion registrada en background");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error registrando licencia Syncfusion: {ex.Message}");
            }
        });

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
            System.Diagnostics.Debug.WriteLine("✅ Cultura española configurada");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error configurando cultura: {ex.Message}");
        }
    }
}