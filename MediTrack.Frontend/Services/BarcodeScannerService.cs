using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing.Mobile;
using ZXing; // Añade esta línea
using System.Diagnostics; // Para Debug.WriteLine

namespace MediTrack.Frontend.Services
{
    public class BarcodeScannerService : IBarcodeScannerService
    {
        public async Task<string> ScanBarcodeAsync()
        {

            // ✅ SIMULACIÓN SOLO PARA EMULADOR (comenta esto después)
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
            {
                Debug.WriteLine("⚠️ Modo emulador: Simulando código de barras");
                return "7501055300107"; // Código EAN-13 de prueba
            }

            // ⚠️ Código normal (se ejecuta en dispositivo físico)



            try
            {
                var scanner = new MobileBarcodeScanner()
                {
                    TopText = "Escanea código de medicamento",
                    BottomText = "Alinea el código dentro del marco"
                };

                var options = new MobileBarcodeScanningOptions
                {
                    PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.CODE_128
                },
                    TryHarder = true
                };

                var result = await scanner.Scan(options);
                return result?.Text ?? "No se detectó código";
            }
            catch
            {
                return "Error en escáner";
            }
        }
    }
}
