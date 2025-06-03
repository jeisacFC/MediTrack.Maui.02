using System.Diagnostics; // Para Debug.WriteLine
using ZXing.Net.Maui; // Para BarcodeFormat y BarcodeReaderOptions

namespace MediTrack.Frontend.Vistas.PantallasFuncionales;

public partial class PantallaScan : ContentPage
{
    public PantallaScan()
    {
        InitializeComponent();

        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = ZXing.Net.Maui.BarcodeFormat.Ean13, // Para códigos de barras (EAN, UPC, Code 128, etc.)
            AutoRotate = true,
            Multiple = false, // Intenta detectar solo uno y luego se detiene (más útil para este flujo)
            TryHarder = true  // Intenta un poco más fuerte si la detección es difícil
        };
    }
    private void barcodeReader_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (e.Results == null || !e.Results.Any())
            return;

        var first = e.Results?.FirstOrDefault();
        if (first is null)
        {
            return;
        }

        Dispatcher.DispatchAsync(async () =>
        {
            await DisplayAlert("Barcode Detected", first.Value, "OK");
        });




        //// Es importante ejecutar actualizaciones de UI o navegación en el hilo principal
        //MainThread.BeginInvokeOnMainThread(async () =>
        //{
        //    // Detener el escaneo una vez que se detecta un código para evitar múltiples detecciones
        //    barcodeReader.IsDetecting = false;

        //    Debug.WriteLine($"Código de Barras Detectado: {firstResult.Value}, Formato: {firstResult.Format}");

        //    await DisplayAlert("Código Detectado", $"Valor: {firstResult.Value}\nFormato: {firstResult.Format}", "OK");

        //    // Aquí es donde, después de mostrar la alerta (o en lugar de ella),
        //    // harías algo con 'firstResult.Value' (el código de barras).
        //    // Por ejemplo, podrías navegar a otra página pasándole este valor,
        //    // o llamar a un ViewModel si estuvieras usando MVVM más estrictamente aquí.

        //    // Ejemplo: Navegar hacia atrás después de la detección
        //    // await Shell.Current.GoToAsync("..");

        //    // Opcional: Si quieres volver a escanear sin salir, podrías reactivar la detección:
        //    // barcodeReader.IsDetecting = true;
        //});
    }

    //private async void Cancelar_Clicked(object sender, EventArgs e)
    //{
    //    // Detener el escaneo y volver a la página anterior
    //    barcodeReader.IsDetecting = false;
    //    await Shell.Current.GoToAsync("..");
    //}

    //// Opcional: Manejar el ciclo de vida de la página para la cámara
    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();
    //    barcodeReader.IsDetecting = true; // Asegura que la detección esté activa al mostrar la página
    //}

    //protected override void OnDisappearing()
    //{
    //    barcodeReader.IsDetecting = false; // Detiene la detección al salir de la página
    //    base.OnDisappearing();
    //}
}