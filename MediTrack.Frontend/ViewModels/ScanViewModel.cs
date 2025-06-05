
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models; 
using MediTrack.Frontend.Services; 
using System.Linq;
using System.Threading.Tasks;
using ZXing.Net.Maui; // Para BarcodeResult y BarcodeReaderOptions
using System.Diagnostics; // Para Debug.WriteLine
using CommunityToolkit.Maui.Views; // Para Popup
using MediTrack.Frontend.Popups; // Para InfoMedicamentoPopup (que crearemos después)



namespace MediTrack.Frontend.ViewModels
{
    public partial class ScanViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isDetecting = true;

        [ObservableProperty]
        private bool _isProcessingResult;

        [ObservableProperty]
        private string _scanResultText;

        // ----------------------------------  COMANDOS  ------------------------------------ //
        public BarcodeReaderOptions BarcodeReaderOptions { get; private set; }
        public IAsyncRelayCommand<BarcodeDetectionEventArgs> ProcesarCodigosDetectadosCommand { get; }
        public IAsyncRelayCommand CancelarEscaneoCommand { get; }
        public IAsyncRelayCommand SimularEscaneoCommand { get; }
        public IAsyncRelayCommand BuscarManualCommand { get; }


        // AGREGAR ESTOS EVENTOS
        public event EventHandler<ResEscanearMedicamento> MostrarResultado;
        public event EventHandler<string> MostrarError;

        // Usamos la interfaz para permitir la Inyección de Dependencias
        private readonly IBarcodeScannerService _barcodeScannerService;


        // ---------------------------------------------------------------------- //

        // El constructor ahora recibe IBarcodeScannerService (Inyección de Dependencias)
        public ScanViewModel(IBarcodeScannerService scannerService)
        {
            _barcodeScannerService = scannerService; // Asigna el servicio inyectado

            BarcodeReaderOptions = new ZXing.Net.Maui.BarcodeReaderOptions
            {
                Formats = ZXing.Net.Maui.BarcodeFormats.All,
                AutoRotate = true,
                Multiple = false
            };

            // En el constructor de ScanViewModel
            Debug.WriteLine($"ScanViewModel: BarcodeReaderOptions configurado - Formats: {BarcodeReaderOptions.Formats}");


            // --------------------------------- DEFINICION DE METODOS QUE VIENEN DE "PANTALLASCAN" ------------------------------------- //
            ProcesarCodigosDetectadosCommand = new AsyncRelayCommand<BarcodeDetectionEventArgs>(EjecutarProcesarCodigosDetectados);
            CancelarEscaneoCommand = new AsyncRelayCommand(EjecutarCancelarEscaneo);
            SimularEscaneoCommand = new AsyncRelayCommand(EjecutarSimulacionEscaneo); // <--- INICIALIZA EL NUEVO COMANDO
            BuscarManualCommand = new AsyncRelayCommand(EjecutarBuscarManual);

            ScanResultText = "Apunte la cámara al código de barras...";
        }




        // ----------------------------------- METODOS ----------------------------------- //
        private async Task EjecutarProcesarCodigosDetectados(BarcodeDetectionEventArgs args)
        {
            if (IsProcessingResult || args?.Results == null || !args.Results.Any())
                return;

            IsProcessingResult = true;
            IsDetecting = false;

            var primerResultado = args.Results[0];
            string codigoEscaneado = primerResultado.Value;
            Debug.WriteLine($"ViewModel: Código Detectado: {codigoEscaneado}");
            ScanResultText = $"Detectado: {codigoEscaneado}";

            try
            {
                ResEscanearMedicamento infoMedicamento = await _barcodeScannerService.ObtenerDatosMedicamentoAsync(codigoEscaneado);

                if (infoMedicamento != null && infoMedicamento.resultado)
                {
                    // DISPARAR EVENTO EN LUGAR DE DisplayAlert
                    MostrarResultado?.Invoke(this, infoMedicamento);
                }
                else
                {
                    string errorMsg = infoMedicamento?.errores?.FirstOrDefault()?.Message ?? "Medicamento no encontrado.";
                    // DISPARAR EVENTO EN LUGAR DE DisplayAlert
                    MostrarError?.Invoke(this, errorMsg);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error procesando en ViewModel: {ex.Message}\n{ex.StackTrace}");
                MostrarError?.Invoke(this, "Ocurrió un error procesando la información.");
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

        private async Task EjecutarSimulacionEscaneo() // El método de simulación sigue siendo útil
        {
            // ... (tu lógica de simulación que llama a EjecutarProcesarCodigosDetectados)
            // Asegúrate que cree un BarcodeDetectionEventArgs válido
            if (IsProcessingResult) return;
            Debug.WriteLine("ViewModel: Ejecutando SIMULACIÓN de escaneo...");
            var codigoSimulado = "7501055300107"; // Código de tu SP o uno que el mock service entienda
            var formatoSimulado = BarcodeFormats.OneDimensional;
            var simulatedResults = new List<BarcodeResult> { new BarcodeResult { Value = codigoSimulado, Format = formatoSimulado } };
            var simulatedArgs = new BarcodeDetectionEventArgs(simulatedResults.ToArray());
            await EjecutarProcesarCodigosDetectados(simulatedArgs);
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
