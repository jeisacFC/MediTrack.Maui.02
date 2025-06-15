namespace MediTrack.Frontend
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // INICIALIZACIÓN MÍNIMA - Solo crear la Shell
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // MOVER INICIALIZACIONES PESADAS AQUÍ (después del startup)
            await Task.Run(async () =>
            {
                try
                {
                    // Dar tiempo para que la UI termine de cargar
                    await Task.Delay(500);

                    // Pre-cargar datos críticos en background
                    await PreloadCriticalDataAsync();

                    System.Diagnostics.Debug.WriteLine("Inicialización background completada");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en inicialización background: {ex.Message}");
                }
            });
        }

        private async Task PreloadCriticalDataAsync()
        {
            try
            {
                // Verificar conectividad
                var connectivity = Connectivity.Current;
                if (connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    System.Diagnostics.Debug.WriteLine("Sin conexión a internet - saltando precarga");
                    return;
                }

                // Pre-verificar token si existe
                var token = await SecureStorage.GetAsync("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("Token encontrado - usuario podría estar logueado");
                    // Aquí podrías validar el token si fuera necesario
                }

                System.Diagnostics.Debug.WriteLine("Precarga de datos críticos completada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en precarga: {ex.Message}");
            }
        }

        protected override void OnSleep()
        {
            // App duerme - pausar operaciones no críticas
            System.Diagnostics.Debug.WriteLine("App entrando en suspensión");
            base.OnSleep();
        }

        protected override void OnResume()
        {
            // App resume - reanudar operaciones
            System.Diagnostics.Debug.WriteLine("App resumiendo");
            base.OnResume();
        }
    }
}