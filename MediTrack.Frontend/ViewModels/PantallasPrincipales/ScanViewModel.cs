

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using ZXing.Net.Maui;
using System.Diagnostics;

=======
=======
>>>>>>> Stashed changes
using System.Linq;
using System.Threading.Tasks;
using ZXing.Net.Maui; // Para BarcodeResult y BarcodeReaderOptions
using System.Diagnostics; // Para Debug.WriteLine
using CommunityToolkit.Maui.Views; // Para Popup
using MediTrack.Frontend.Popups;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.Models.Response; // Para InfoMedicamentoPopup (que crearemos después)
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes



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
            _apiService = apiService;

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


            // En el ViewModel, al inicio de EjecutarProcesarCodigosDetectados:
            Debug.WriteLine($"Estado inicial - IsDetecting: {IsDetecting}, IsProcessingResult: {IsProcessingResult}");

            // Verificar si ya estamos procesando o no hay resultados
            if (IsProcessingResult || args?.Results == null || !args.Results.Any())
            {
                Debug.WriteLine("Ya procesando o sin resultados válidos");
                return;
            }

            IsProcessingResult = true;


            string codigoEscaneado = args.Results[0].Value;
            Debug.WriteLine($"ViewModel: Procesando código: {codigoEscaneado}");


            try
            {
                var request = new ReqEscanearMedicamento
                {
                    CodigoBarras = codigoEscaneado,
                    IdUsuario = 1,
                    IdMetodoEscaneo = 1
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
                    string errorMsg = infoMedicamento?.errores?.FirstOrDefault()?.Message ?? "Medicamento no encontrado en el sistema. :(";
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
                // IMPORTANTE: Siempre liberar el bloqueo
                IsProcessingResult = false;
                Debug.WriteLine("Procesamiento completado, IsProcessingResult = false");
            }
        }


        // MÉTODO MEJORADO PARA REACTIVAR ESCANEO
        public void ReactivarEscaneo()
        {
            Debug.WriteLine("Reactivando escaneo...");
            IsProcessingResult = false; // Asegurar que no esté bloqueado
            IsDetecting = true;
            ScanResultText = "Apunte la cámara al código de barras...";
        }

        // MÉTODO PARA DETENER ESCANEO
        public void DetenerEscaneo()
        {
            Debug.WriteLine("Deteniendo escaneo...");
            IsDetecting = false;
        }


        private async Task EjecutarCancelarEscaneo()
        {
            DetenerEscaneo();

            if (Shell.Current.Navigation.NavigationStack.Count > 1)
            {
                await Shell.Current.GoToAsync("..", true);
            }
            else
            {
                Debug.WriteLine("No se pudo navegar hacia atrás, ya estamos en la raíz o es la única página.");
            }
        }


        private async Task EjecutarBuscarManual()
        {
            DetenerEscaneo();
            await Application.Current.MainPage.DisplayAlert("Navegación", "Ir a Búsqueda Manual (pendiente)", "OK");
        }

    }
}
