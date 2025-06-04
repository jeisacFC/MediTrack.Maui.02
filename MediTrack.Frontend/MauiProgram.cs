using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Syncfusion.Maui.Core.Hosting;
using System.Globalization;

namespace MediTrack.Frontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // ✅ CONFIGURAR CULTURA ESPAÑOLA AL INICIO
        ConfigurarCulturaEspañola();

        try
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF1cWWhPYVJ3WmFZfVtgdVdMYVlbRnJPIiBoS35Rc0VlWXtfcnVQRGReUU1yVEBU");
            System.Diagnostics.Debug.WriteLine("✅ Licencia de Syncfusion registrada");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error registrando licencia: {ex.Message}");
        }

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

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

            System.Diagnostics.Debug.WriteLine("✅ Cultura española configurada");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error configurando cultura: {ex.Message}");
        }
    }
}
