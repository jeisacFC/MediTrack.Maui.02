// En Services/BarcodeScannerService.cs
using MediTrack.Frontend.Models; // Para ResEscanearMedicamento y ReqEscanearMedicamento
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic; // Para List

namespace MediTrack.Frontend.Services
{
    public class BarcodeScannerService : IBarcodeScannerService
    {
        // Este método ya NO INICIA el escaneo visual.
        // El escaneo visual ocurre en PantallaScan.xaml con CameraBarcodeReaderView.
        // Este servicio ahora se usaría para tomar el código escaneado y obtener los datos del medicamento.
        public async Task<ResEscanearMedicamento> ObtenerDatosMedicamentoAsync(string codigoBarras)
        {
            Debug.WriteLine($"BarcodeScannerService: Buscando datos para el código: {codigoBarras}");
            await Task.Delay(500); // Simular llamada a backend

            // SIMULACIÓN: Devuelve datos de prueba basados en el código de barras
            if (!string.IsNullOrEmpty(codigoBarras))
            {
                // Aquí, en una implementación real, llamarías a tu API de backend
                // pasándole el codigoBarras y el IdUsuario (que el ViewModel debería tener).
                // Ej: return await _apiClient.GetMedicamentoInfoAsync(codigoBarras, idUsuario);
                return new ResEscanearMedicamento
                {
                    IdMedicamento = 1,
                    NombreComercial = $"Medicamento para {codigoBarras}",
                    PrincipioActivo = "Principio Activo Simulado",
                    Dosis = "100mg",
                    Fabricante = "PharmaSim MAUI",
                    Usos = new List<string> { "Uso simulado 1", "Uso simulado 2" },
                    Advertencias = new List<string> { "Advertencia simulada 1" },
                    EfectosSecundarios = new EfectosSecundariosCategorizados
                    {
                        Leve = new List<string> { "Efecto leve simulado" },
                        Moderado = new List<string>(),
                        Grave = new List<string>()
                    },
                    resultado = true
                };
            }
            else
            {
                return new ResEscanearMedicamento
                {
                    resultado = false,
                    errores = new List<Error> { new Error { Message = "Código de barras vacío (simulado)" } }
                };
            }
        }

        public async Task<string> ScanBarcodeAsync()
        {
            // Esta implementación usa el MobileBarcodeScanner que abre una pantalla separada.
            // No es lo que usaremos para la vista de cámara integrada.
            Debug.WriteLine("⚠️ BarcodeScannerService.ScanBarcodeAsync() llamado - Usa la vista de cámara integrada en su lugar.");
            await Application.Current.MainPage.DisplayAlert("Info", "Esta función de escaneo modal está obsoleta. Usa la cámara en la página de escaneo.", "OK");
            return "OBSOLETO: Usar vista de cámara integrada";
        }
    }
}