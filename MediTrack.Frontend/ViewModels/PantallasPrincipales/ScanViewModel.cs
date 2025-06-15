using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Interfaces;
using ZXing.Net.Maui;
using System.Diagnostics;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class ScanViewModel : ObservableObject
    {
        // --- Propiedades para la UI --- //
        [ObservableProperty] private bool _isDetecting = true;
        [ObservableProperty] private bool _isProcessingResult;
        [ObservableProperty] private string _scanResultText;
        [ObservableProperty] private bool _isBusy;
        [ObservableProperty] private bool _isScanning = false;

        public BarcodeReaderOptions BarcodeReaderOptions { get; private set; }
        private readonly INavigationService _navigationService;

        // --- Comandos --- //
        public IAsyncRelayCommand<BarcodeDetectionEventArgs> ProcesarCodigosDetectadosCommand { get; }
        public IAsyncRelayCommand CancelarEscaneoCommand { get; }
        public IAsyncRelayCommand BuscarManualCommand { get; }


        // --- Eventos para comunicar a la Vista --- //
        public event EventHandler<ResEscanearMedicamento> MostrarResultado;
        public event EventHandler<string> MostrarError;

        // --- Dependencias --- //
        private readonly IApiService _apiService;

        // El constructor ahora recibe IApiService (Inyección de Dependencias)
        public ScanViewModel(IApiService apiService, INavigationService navigationService)
        {
            _apiService = apiService;
            _navigationService = navigationService;

            var navInstanceId = _navigationService.GetHashCode();
            System.Diagnostics.Debug.WriteLine($"ScanViewModel recibió NavigationService ID: {navInstanceId}");

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
            var userIdStr = await SecureStorage.GetAsync("user_id");

      
            Debug.WriteLine($"ViewModel: Procesando código: {codigoEscaneado}");
            Debug.WriteLine($"ID de usuario (string): {userIdStr} - Tipo: {userIdStr?.GetType()}");


            try
            {
                var request = new ReqEscanearMedicamento
                {
                    CodigoBarras = codigoEscaneado,
                    IdUsuario = int.Parse(userIdStr),
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
                    string errorMsg = infoMedicamento?.errores?.FirstOrDefault()?.mensaje ?? "Medicamento no encontrado en el sistema. :(";
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
            IsScanning = false;
            ScanResultText = "Apunte la cámara al código de barras...";
        }

        // MÉTODO PARA DETENER ESCANEO
        public void DetenerEscaneo()
        {
            Debug.WriteLine("Deteniendo escaneo...");
            IsDetecting = false;
            IsScanning = false;
        }

        private async Task EjecutarCancelarEscaneo()
        {
            System.Diagnostics.Debug.WriteLine("=== INICIO EjecutarCancelarEscaneo ==="); // ← AGREGAR
            try
            {
                DetenerEscaneo();
                System.Diagnostics.Debug.WriteLine("Llamando a VolverAPaginaAnteriorAsync...");
                await _navigationService.VolverAPaginaAnteriorAsync();
                System.Diagnostics.Debug.WriteLine("=== FIN EjecutarCancelarEscaneo ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en navegación: {ex.Message}");
            }
        }

        private async Task EjecutarBuscarManual()
        {

            try 
            {
                System.Diagnostics.Debug.WriteLine("=== INICIO BuscarManual ===");

                // Guardar página actual por si necesitas volver
                _navigationService.GuardarPaginaActual();

                DetenerEscaneo();

                System.Diagnostics.Debug.WriteLine("Navegando a búsqueda...");
                await _navigationService.GoToAsync("//busqueda");

                System.Diagnostics.Debug.WriteLine("Navegación a búsqueda completada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a búsqueda: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task GuardarMedicamentoAsync(ResEscanearMedicamento medicamento)
        {
            if (medicamento == null)
            {
                Debug.WriteLine("Intento de guardar un medicamento nulo.");
                return;
            }

            IsBusy = true; // Mostramos un indicador de carga en la UI
            try
            {
                // Obtenemos el ID del usuario que está logueado
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (!int.TryParse(userIdStr, out int userId))
                {
                    await Shell.Current.DisplayAlert("Error", "No se pudo identificar al usuario. Por favor, inicia sesión de nuevo.", "OK");
                    return;
                }

                // Mapeamos los datos del resultado del escaneo al objeto que la API espera (ReqGuardarMedicamento)
                var request = new ReqGuardarMedicamento
                {
                    IdUsuario = userId,
                    NombreComercial = medicamento.NombreComercial,
                    PrincipioActivo = medicamento.PrincipioActivo,
                    Dosis = medicamento.Dosis,
                    Fabricante = medicamento.Fabricante,
                    Usos = medicamento.Usos,
                    Advertencias = medicamento.Advertencias,
                    EfectosSecundarios = new List<string>(), // Asumimos que el escaneo no devuelve esto
                    IdMetodoEscaneo = 1 // 1 para Escaneo, por ejemplo
                };

                // Llamamos a nuestro nuevo método del ApiService
                var resultadoGuardado = await _apiService.GuardarMedicamentoAsync(request);

                // Verificamos la respuesta del backend y mostramos el mensaje apropiado
                if (resultadoGuardado != null && resultadoGuardado.resultado)
                {
                    // Mostramos el mensaje que viene del backend (ej. "Medicamento guardado" o "Ya existe")
                    await Shell.Current.DisplayAlert("Información", resultadoGuardado.Mensaje, "OK");
                }
                else
                {
                    // Mostramos un mensaje de error genérico o el del backend si lo hay
                    var errorMsg = resultadoGuardado?.Mensaje ?? "No se pudo guardar el medicamento.";
                    await Shell.Current.DisplayAlert("Error", errorMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción al guardar medicamento: {ex.Message}");
                await Shell.Current.DisplayAlert("Error de Conexión", "Ocurrió un problema al guardar.", "OK");
            }
            finally
            {
                IsBusy = false; // Ocultamos el indicador de carga
            }
        }
    }
}
