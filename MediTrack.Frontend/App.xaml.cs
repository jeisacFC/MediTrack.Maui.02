using System.Globalization;

namespace MediTrack.Frontend
{
    public partial class App : Application
    {
        public App()
        {
            // ✅ BACKUP: Configurar cultura aquí también
            ConfigurarCulturaEspañola();

            try
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF1cWWhPYVJ3WmFZfVtgdVdMYVlbRnJPIiBoS35Rc0VlWXtfcnVQRGReUU1yVEBU");
                System.Diagnostics.Debug.WriteLine("✅ Licencia backup registrada en App.xaml.cs");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en licencia backup: {ex.Message}");
            }

            InitializeComponent();
            MainPage = new AppShell();
        }

        private void ConfigurarCulturaEspañola()
        {
            try
            {
                var cultura = new CultureInfo("es-ES");
                CultureInfo.CurrentCulture = cultura;
                CultureInfo.CurrentUICulture = cultura;

                System.Diagnostics.Debug.WriteLine("✅ Cultura española configurada en App");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error configurando cultura en App: {ex.Message}");
            }
        }
    }
}