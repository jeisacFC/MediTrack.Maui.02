using System.Globalization;

namespace MediTrack.Frontend
{
    public partial class App : Application
    {
        public App()
        {
            // ✅ BACKUP: Configurar cultura aquí también
            ConfigurarCulturaEspañola();

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

                System.Diagnostics.Debug.WriteLine("Cultura española configurada en App");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando cultura en App: {ex.Message}");
            }
        }
    }
}