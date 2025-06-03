
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models; 
using MediTrack.Frontend.Services; 
using System.Linq;
using System.Threading.Tasks;
using ZXing.Net.Maui; // Para BarcodeResult y BarcodeReaderOptions
using System.Diagnostics; // Para Debug.WriteLine

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

        // ---------------------------------------------------------------------- //
        public BarcodeReaderOptions BarcodeReaderOptions { get; private set; }
        public IAsyncRelayCommand<BarcodeDetectionEventArgs> ProcesarCodigosDetectadosCommand { get; }
        public IAsyncRelayCommand CancelarEscaneoCommand { get; }
        public IAsyncRelayCommand SimularEscaneoCommand { get; }

        // Usamos la interfaz para permitir la Inyección de Dependencias
        private readonly IBarcodeScannerService _barcodeScannerService;



        // ---------------------------------------------------------------------- //

        // El constructor ahora recibe IBarcodeScannerService (Inyección de Dependencias)
        public ScanViewModel(IBarcodeScannerService scannerService)
        {
            _barcodeScannerService = scannerService; // Asigna el servicio inyectado

            BarcodeReaderOptions = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.OneDimensional,
                AutoRotate = true,
                Multiple = false,
                TryHarder = true
            };


            // --------------------------------- DEFINICION DE METODOS QUE VIENEN DE "PANTALLASCAN" ------------------------------------- //
            ProcesarCodigosDetectadosCommand = new AsyncRelayCommand<BarcodeDetectionEventArgs>(EjecutarProcesarCodigosDetectados);
            CancelarEscaneoCommand = new AsyncRelayCommand(EjecutarCancelarEscaneo);
            SimularEscaneoCommand = new AsyncRelayCommand(EjecutarSimulacionEscaneo); // <--- INICIALIZA EL NUEVO COMANDO

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
            // string formatoCodigo = primerResultado.Format.ToString(); // Puedes usarlo si lo necesitas

            Debug.WriteLine($"ViewModel: Código Detectado: {codigoEscaneado}");
            ScanResultText = $"Detectado: {codigoEscaneado}";

            try
            {
                // LLAMAMOS AL MÉTODO CORRECTO DEL SERVICIO
                ResEscanearMedicamento infoMedicamento = await _barcodeScannerService.ObtenerDatosMedicamentoAsync(codigoEscaneado);

                if (infoMedicamento != null && infoMedicamento.resultado)
                {
                    // LÓGICA PARA MOSTRAR EL MODAL (siguiente paso)
                    await Application.Current.MainPage.DisplayAlert(
                       infoMedicamento.NombreComercial ?? "Medicamento Escaneado",
                       $"Principio Activo: {infoMedicamento.PrincipioActivo ?? "N/A"}\nDosis: {infoMedicamento.Dosis ?? "N/A"}",
                       "OK");

                    // Volvemos a la página anterior después de la alerta/modal
                    await EjecutarCancelarEscaneo();
                }
                else
                {
                    string errorMsg = infoMedicamento?.errores?.FirstOrDefault()?.Message ?? "Medicamento no encontrado o error desconocido.";
                    await Application.Current.MainPage.DisplayAlert("Error", errorMsg, "OK");
                    IsDetecting = true; // Permitir reintentar
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error procesando código de barras en ViewModel: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}"); // Añade esto para más detalles del error
                await Application.Current.MainPage.DisplayAlert("Error del Sistema", "Ocurrió un error procesando la información.", "OK");
                IsDetecting = true; // Permitir reintentar
            }
            finally
            {
                IsProcessingResult = false;
            }
        }

        private async Task EjecutarCancelarEscaneo()
        {
            IsDetecting = false;

            // Navegar hacia atrás si estamos en la página de escaneo
            // Es importante verificar si la página actual es la que esperamos antes de navegar ".."
            // para evitar errores si ya se navegó o si el shell está en un estado inesperado.
            var currentPageRoute = Shell.Current.CurrentState?.Location?.OriginalString;
            string rutaPantallaScan = "pantallaScan";
            if (currentPageRoute != null && currentPageRoute.EndsWith("/" + rutaPantallaScan, StringComparison.OrdinalIgnoreCase) || currentPageRoute.EndsWith("pantallaScan", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await Shell.Current.GoToAsync("..", true);
                }
                catch (Exception navEx)
                {
                    Debug.WriteLine($"Error al navegar hacia atrás: {navEx.Message}");
                    // Considera una ruta de fallback si ".." falla, como ir a la página principal
                    // await Shell.Current.GoToAsync("//rutaPaginaPrincipal");
                }
            }
            else
            {
                Debug.WriteLine($"No se navegó hacia atrás, página actual no es PantallaScan o ruta desconocida: {currentPageRoute}");
            }
        }

        private async Task EjecutarSimulacionEscaneo()
        {
            if (IsProcessingResult) return;

            Debug.WriteLine("ViewModel: Ejecutando SIMULACIÓN de escaneo...");

            // Crea un resultado de código de barras simulado
            var codigoSimulado = "1234567890120"; // Un código de barras de prueba que tu mock service pueda reconocer
            var formatoSimulado = BarcodeFormats.OneDimensional; // O el formato que quieras simular

            var simulatedResults = new List<BarcodeResult>
        {
            new BarcodeResult
            {
                Value = codigoSimulado,
                Format = formatoSimulado,
                // Otras propiedades como RawBytes, ResultPoints pueden dejarse como null o default para la simulación
            }
        };

            // Crea los EventArgs simulados
            var simulatedArgs = new BarcodeDetectionEventArgs(simulatedResults.ToArray());

            // Llama al método de procesamiento existente con los datos simulados
            await EjecutarProcesarCodigosDetectados(simulatedArgs);
        }





    }
}

#region CODIGO COMENTADO
//using System;
//using System.ComponentModel; // Para INotifyPropertyChanged
//using System.Runtime.CompilerServices; // Para CallerMemberName
//using System.Windows.Input; // Para ICommand
//using MediTrack.Frontend.Services; // Para IBarcodeScannerService
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ZXing; // Añade esta línea
//using ZXing.Mobile;

//namespace MediTrack.Frontend.ViewModels;


//public class ScanViewModel : INotifyPropertyChanged
//{
//    private readonly IBarcodeScannerService _scanner;

//    private string _scanResult;
//    public string ScanResult
//    {
//        get => _scanResult;
//        set
//        {
//            _scanResult = value;
//            OnPropertyChanged();
//        }
//    }

//    public ICommand ScanCommand { get; }

//    public ScanViewModel(IBarcodeScannerService scanner)
//    {
//        _scanner = scanner;
//        ScanCommand = new Command(async () => await ScanBarcode());
//    }

//    private async Task ScanBarcode()
//    {
//        ScanResult = "Escaneando...";
//        ScanResult = await _scanner.ScanBarcodeAsync();
//    }

//    public event PropertyChangedEventHandler PropertyChanged;
//    protected void OnPropertyChanged([CallerMemberName] string name = null)
//    {
//        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//    }
//}
#endregion 

