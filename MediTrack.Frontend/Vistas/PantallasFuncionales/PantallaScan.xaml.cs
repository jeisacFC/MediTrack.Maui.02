using CommunityToolkit.Maui.Views; // Para this.ShowPopupAsync()
using MediTrack.Frontend.Popups;   // Para InstruccionesEscaneoPopup
using MediTrack.Frontend.ViewModels; // Para ScanViewModel
using System.Diagnostics;
using ZXing.Net.Maui; // Para BarcodeDetectionEventArgs

namespace MediTrack.Frontend.Vistas.PantallasFuncionales;

public partial class PantallaScan : ContentPage
{
    public PantallaScan(ScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Verificar permisos de cámara
        var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
        Debug.WriteLine($"Permiso de cámara: {cameraStatus}");

        if (cameraStatus != PermissionStatus.Granted)
        {
            cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            Debug.WriteLine($"Permiso solicitado: {cameraStatus}");
        }

        var instruccionesPopup = new InstruccionesEscaneoPopup();
        await this.ShowPopupAsync(instruccionesPopup);

        if (BindingContext is ScanViewModel vm)
        {
            vm.IsDetecting = true;
            Debug.WriteLine("PantallaScan: OnAppearing - IsDetecting puesto a true después del popup");
        }
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is ScanViewModel vm)
        {
            vm.IsDetecting = false;
        }
        Debug.WriteLine("PantallaScan: OnDisappearing - IsDetecting puesto a false");
        base.OnDisappearing();
    }

    // Manejador para el botón Cancelar en el Header
    private async void Cancelar_Clicked_Header(object sender, EventArgs e)
    {
        if (BindingContext is ScanViewModel vm && vm.CancelarEscaneoCommand.CanExecute(null))
        {
            await vm.CancelarEscaneoCommand.ExecuteAsync(null);
        }
    }


    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Debug.WriteLine($"Códigos detectados: {e.Results?.Count() ?? 0}");

            if (e.Results?.Any() == true)
            {
                var barcode = e.Results.FirstOrDefault();
                Debug.WriteLine($"Código encontrado: {barcode?.Value} - Formato: {barcode?.Format}");

                if (barcode != null && !string.IsNullOrEmpty(barcode.Value))
                {
                    // Pasar todo el BarcodeDetectionEventArgs (no solo barcode.Value)
                    if (BindingContext is ScanViewModel vm)
                    {
                        vm.IsDetecting = false;

                        // Pasar 'e' completo, no 'barcode.Value'
                        if (vm.ProcesarCodigosDetectadosCommand.CanExecute(e))
                        {
                            vm.ProcesarCodigosDetectadosCommand.Execute(e);
                        }
                    }
                }
            }
        });
    }

}