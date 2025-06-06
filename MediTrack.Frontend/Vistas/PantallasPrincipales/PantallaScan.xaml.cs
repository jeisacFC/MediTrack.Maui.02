using CommunityToolkit.Maui.Views; // Para this.ShowPopupAsync()
using MediTrack.Frontend.Popups;   // Para InstruccionesEscaneoPopup
using MediTrack.Frontend.ViewModels.PantallasPrincipales; // Para ScanViewModel
using System.Diagnostics;
using ZXing.Net.Maui; // Para BarcodeDetectionEventArgs
using MediTrack.Frontend.Models.Response; // Para IBarcodeScannerService

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaScan : ContentPage
{

    private ScanViewModel _viewModel;
    public PantallaScan(ScanViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
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
        _viewModel?.ReactivarEscaneo();
    }

    private async void OnMostrarError(object sender, string mensaje)
    {
        await DisplayAlert("Error", mensaje, "OK");

        // REACTIVAR ESCANEO DESPUÉS DEL ERROR
        _viewModel?.ReactivarEscaneo();
    }


    // LIMPIAR EVENTOS AL SALIR
    protected override void OnDisappearing()
    {
        if (_viewModel != null)
        {
            _viewModel.DetenerEscaneo();
            _viewModel.MostrarResultado -= OnMostrarResultado;
            _viewModel.MostrarError -= OnMostrarError;
        }
        base.OnDisappearing();
    }


    // MANEJADOR PARA EL BOTÓN CANCELAR EN EL HEADER
    private async void Cancelar_Clicked_Header(object sender, EventArgs e)
    {
        if (_viewModel?.CancelarEscaneoCommand?.CanExecute(null) == true)
        {
            await _viewModel.CancelarEscaneoCommand.ExecuteAsync(null);
        }
    }


    // MANEJADOR MEJORADO PARA DETECCIÓN DE CÓDIGOS
    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        // Verificar que tenemos un ViewModel y resultados válidos
        if (_viewModel == null || e?.Results == null || !e.Results.Any())
        {
            Debug.WriteLine("No hay ViewModel o resultados de escaneo");
            return;
        }

        // Verificar si ya estamos procesando
        if (_viewModel.IsProcessingResult)
        {
            Debug.WriteLine("Ya se está procesando un resultado, ignorando...");
            return;
        }

        var barcode = e.Results.FirstOrDefault();
        if (barcode == null || string.IsNullOrEmpty(barcode.Value))
        {
            Debug.WriteLine("Código de barras vacío o nulo");
            return;
        }

        Debug.WriteLine($"Código detectado: {barcode.Value}");

        // DETENER DETECCIÓN ANTES DE PROCESAR
        _viewModel.DetenerEscaneo();

        // EJECUTAR EN EL HILO PRINCIPAL
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_viewModel.ProcesarCodigosDetectadosCommand.CanExecute(e))
            {
                await _viewModel.ProcesarCodigosDetectadosCommand.ExecuteAsync(e);
            }
            else
            {
                Debug.WriteLine("No se puede ejecutar el comando de procesamiento");
                _viewModel.ReactivarEscaneo(); // Reactivar si no se pudo procesar
            }
        });
    }


    // MÉTODO PARA REACTIVAR MANUALMENTE EL ESCANEO (si necesitas un botón)
    private void ReactivarEscaneo_Clicked(object sender, EventArgs e)
    {
        _viewModel?.ReactivarEscaneo();
    }

}