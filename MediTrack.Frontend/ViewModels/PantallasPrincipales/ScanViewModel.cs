
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Models.Request; 
using MediTrack.Frontend.Models.Response; 
using MediTrack.Frontend.Services.Interfaces; // Necesario para IApiService
using System.Linq;
using System.Threading.Tasks;
using ZXing.Net.Maui;
using System.Diagnostics;
using CommunityToolkit.Maui.Views; // Para Popup
using MediTrack.Frontend.Popups;




namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class ScanViewModel : ObservableObject
    {
        // --- Propiedades para la UI --- //
        [ObservableProperty] private bool _isDetecting = true;
        [ObservableProperty] private bool _isProcessingResult;
        [ObservableProperty] private string _scanResultText;

        public BarcodeReaderOptions BarcodeReaderOptions { get; private set; }
        
        // --- Comandos --- //
        public IAsyncRelayCommand<BarcodeDetectionEventArgs> ProcesarCodigosDetectadosCommand { get; }
        public IAsyncRelayCommand CancelarEscaneoCommand { get; }
        public IAsyncRelayCommand SimularEscaneoCommand { get; }
        public IAsyncRelayCommand BuscarManualCommand { get; }


        // --- Eventos para comunicar a la Vista --- //
        public event EventHandler<ResEscanearMedicamento> MostrarResultado;
        public event EventHandler<string> MostrarError;

        // --- Dependencias --- //
        private readonly IApiService _apiService;


        // ---------------------------------------------------------------------- //

        // El constructor ahora recibe IApiService (Inyección de Dependencias)
        public ScanViewModel(IApiService apiService)
        {
            _apiService = apiService; // Asigna el servicio inyectado

            BarcodeReaderOptions = new ZXing.Net.Maui.BarcodeReaderOptions
            {
                Formats = ZXing.Net.Maui.BarcodeFormats.All,
                AutoRotate = true,
                Multiple = false
            };

           

            // --------------------------------- DEFINICION DE METODOS QUE VIENEN DE "PANTALLASCAN" ------------------------------------- //
            ProcesarCodigosDetectadosCommand = new AsyncRelayCommand<BarcodeDetectionEventArgs>(EjecutarProcesarCodigosDetectados);
            CancelarEscaneoCommand = new AsyncRelayCommand(EjecutarCancelarEscaneo);
            BuscarManualCommand = new AsyncRelayCommand(EjecutarBuscarManual);

            ScanResultText = "Apunte la cámara al código de barras...";
        }




        // ----------------------------------- METODOS DE LOS COMANDOS ----------------------------------- //
        private async Task EjecutarProcesarCodigosDetectados(BarcodeDetectionEventArgs args)
        {
            if (IsProcessingResult || args?.Results == null || !args.Results.Any())
                return;

            IsProcessingResult = true;
            IsDetecting = false;

            string codigoEscaneado = args.Results[0].Value;
            Debug.WriteLine($"ViewModel: Código Detectado: {codigoEscaneado}. Llamando al API Service...");

            try
            {

                // Prepara el objeto Request para enviar al backend
                var request = new ReqEscanearMedicamento
                {
                    CodigoBarras = codigoEscaneado,
                    IdUsuario = 1, // TEMPORAL: Esto debería venir de un servicio de sesión de usuario
                    IdMetodoEscaneo = 1 // Asumiendo 1 = Código de Barras
                };

                // LLAMADA REAL AL BACKEND a través del ApiService
                ResEscanearMedicamento infoMedicamento = await _apiService.EscanearMedicamentoAsync(request);

                if (infoMedicamento != null && infoMedicamento.resultado)
                {
                    // Dispara el evento para que la Vista (code-behind) muestre el resultado
                    MostrarResultado?.Invoke(this, infoMedicamento);
                }
                else
                {
                    // Si la API devuelve resultado = false o nulo, dispara el evento de error
                    string errorMsg = infoMedicamento?.errores?.FirstOrDefault()?.Message ?? "Medicamento no encontrado en el sistema.";
                    MostrarError?.Invoke(this, errorMsg);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al llamar al ApiService: {ex.Message}");
                MostrarError?.Invoke(this, "Error de conexión. No se pudo comunicar con el servidor.");
            }
            finally
            {
                IsProcessingResult = false;
            }
        }


        // MÉTODO PARA REACTIVAR ESCANEO (llamar desde code-behind)
        public void ReactivarEscaneo()
        {
            IsDetecting = true;
        }

        private async Task EjecutarCancelarEscaneo()
        {
            IsDetecting = false;
            // Navegar a la página anterior (o a la página principal si es más apropiado)
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
            {
                await Shell.Current.GoToAsync("..", true);
            }
            else
            {
                // Si no hay a dónde ir "hacia atrás", quizás ir a una página de inicio
                // await Shell.Current.GoToAsync("//RutaPaginaPrincipal");
                Debug.WriteLine("No se pudo navegar hacia atrás, ya estamos en la raíz o es la única página.");
            }
        }

   
        private async Task EjecutarBuscarManual()
        {
            IsDetecting = false; // Detener la cámara si estaba activa
                                 // Navegar a la página de búsqueda manual (necesitarás crear esta página y su ruta)
                                 // Ejemplo: await Shell.Current.GoToAsync("//PantallaBusquedaManual");
            await Application.Current.MainPage.DisplayAlert("Navegación", "Ir a Búsqueda Manual (pendiente)", "OK");
        }

    }
}
