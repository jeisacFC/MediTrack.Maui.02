using CommunityToolkit.Maui.Views; // Para this.ShowPopupAsync()
using MediTrack.Frontend.Popups;   // Para InstruccionesEscaneoPopup
using MediTrack.Frontend.ViewModels.PantallasPrincipales; // Para ScanViewModel
using System.Diagnostics;
using ZXing.Net.Maui; // Para BarcodeDetectionEventArgs
using MediTrack.Frontend.Models; // Para IBarcodeScannerService

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaScan : ContentPage
{
    public PantallaScan(ScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // SUSCRIBIRSE A LOS EVENTOS DEL VIEWMODEL
        viewModel.MostrarResultado += OnMostrarResultado;
        viewModel.MostrarError += OnMostrarError;

    }


    // MANEJAR EVENTOS DEL VIEWMODEL
    private async void OnMostrarResultado(object sender, ResEscanearMedicamento medicamento)
    {
        await DisplayAlert(
           medicamento.NombreComercial ?? "Medicamento Escaneado",
           $"Principio: {medicamento.PrincipioActivo ?? "N/A"}",
           "OK");

        // REACTIVAR ESCANEO DESPUÉS DEL ALERT
        if (BindingContext is ScanViewModel vm)
        {
            vm.ReactivarEscaneo();
        }
    }

    private async void OnMostrarError(object sender, string mensaje)
    {
        await DisplayAlert("Error", mensaje, "OK");

        // REACTIVAR ESCANEO DESPUÉS DEL ERROR
        if (BindingContext is ScanViewModel vm)
        {
            vm.ReactivarEscaneo();
        }
    }

    // LIMPIAR EVENTOS AL SALIR
    protected override void OnDisappearing()
    {
        if (BindingContext is ScanViewModel vm)
        {
            vm.IsDetecting = false;
            vm.MostrarResultado -= OnMostrarResultado;
            vm.MostrarError -= OnMostrarError;
        }
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
        if (BindingContext is ScanViewModel vm && e.Results?.Any() == true)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var barcode = e.Results.FirstOrDefault();
                Debug.WriteLine($"Código encontrado: {barcode?.Value}");

                if (barcode != null && !string.IsNullOrEmpty(barcode.Value))
                {
                    vm.IsDetecting = false;
                    if (vm.ProcesarCodigosDetectadosCommand.CanExecute(e))
                    {
                        vm.ProcesarCodigosDetectadosCommand.Execute(e);
                    }
                }
            });
        }
    }

}