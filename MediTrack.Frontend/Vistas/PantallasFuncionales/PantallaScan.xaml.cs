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
            Formats = ZXing.Net.Maui.BarcodeFormat.Ean13, // Para c�digos de barras (EAN, UPC, Code 128, etc.)
            AutoRotate = true,
            Multiple = false, // Intenta detectar solo uno y luego se detiene (m�s �til para este flujo)
            TryHarder = true  // Intenta un poco m�s fuerte si la detecci�n es dif�cil
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




        //// Es importante ejecutar actualizaciones de UI o navegaci�n en el hilo principal
        //MainThread.BeginInvokeOnMainThread(async () =>
        //{
        //    // Detener el escaneo una vez que se detecta un c�digo para evitar m�ltiples detecciones
        //    barcodeReader.IsDetecting = false;

        //    Debug.WriteLine($"C�digo de Barras Detectado: {firstResult.Value}, Formato: {firstResult.Format}");

        //    await DisplayAlert("C�digo Detectado", $"Valor: {firstResult.Value}\nFormato: {firstResult.Format}", "OK");

        //    // Aqu� es donde, despu�s de mostrar la alerta (o en lugar de ella),
        //    // har�as algo con 'firstResult.Value' (el c�digo de barras).
        //    // Por ejemplo, podr�as navegar a otra p�gina pas�ndole este valor,
        //    // o llamar a un ViewModel si estuvieras usando MVVM m�s estrictamente aqu�.

        //    // Ejemplo: Navegar hacia atr�s despu�s de la detecci�n
        //    // await Shell.Current.GoToAsync("..");

        //    // Opcional: Si quieres volver a escanear sin salir, podr�as reactivar la detecci�n:
        //    // barcodeReader.IsDetecting = true;
        //});
    }

    //private async void Cancelar_Clicked(object sender, EventArgs e)
    //{
    //    // Detener el escaneo y volver a la p�gina anterior
    //    barcodeReader.IsDetecting = false;
    //    await Shell.Current.GoToAsync("..");
    //}

    //// Opcional: Manejar el ciclo de vida de la p�gina para la c�mara
    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();
    //    barcodeReader.IsDetecting = true; // Asegura que la detecci�n est� activa al mostrar la p�gina
    //}

    //protected override void OnDisappearing()
    //{
    //    barcodeReader.IsDetecting = false; // Detiene la detecci�n al salir de la p�gina
    //    base.OnDisappearing();
    //}
}