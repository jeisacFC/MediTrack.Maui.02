using CommunityToolkit.Maui.Views; 
using MediTrack.Frontend.Popups;   
using MediTrack.Frontend.ViewModels.PantallasPrincipales; 
using System.Diagnostics;
using ZXing.Net.Maui; 
using MediTrack.Frontend.Models.Response; 
using MediTrack.Frontend.Vistas.Base;
using System.ComponentModel;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaScan : ContentPage
{
    private ScanViewModel _viewModel;
    private bool _isAnimating = false;
    private bool _isLoadingAnimating = false;



    public PantallaScan(ScanViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // SUSCRIBIRSE A LOS EVENTOS DEL VIEWMODEL
        viewModel.MostrarResultado += OnMostrarResultado;
        viewModel.MostrarError += OnMostrarError;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }



    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var instruccionesPopup = new InstruccionesEscaneoPopup();
        await this.ShowPopupAsync(instruccionesPopup);

        if (_viewModel != null && !_viewModel.IsDetecting)
        {
            _viewModel.ReactivarEscaneo();
        }
    }



    // Manejador para cambios en el ViewModel (para la animaci�n)
    private async void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScanViewModel.IsScanning))
        {
            var viewModel = sender as ScanViewModel;
            if (viewModel?.IsScanning == true)
            {
                await StartScanningAnimation();
            }
            else
            {
                StopScanningAnimation();
            }
        }
        else if (e.PropertyName == nameof(ScanViewModel.IsBusy))
        {
            var viewModel = sender as ScanViewModel;
            if (viewModel?.IsBusy == true)
            {
                await StartLoadingAnimation();
            }
            else
            {
                StopLoadingAnimation();
            }
        }
    }

    // M�todo para iniciar la animaci�n
    private async Task StartScanningAnimation()
    {
        _isAnimating = true;
        while (_isAnimating)
        {
            await ScanLine.TranslateTo(0, 200, 1000, Easing.Linear); // Mueve la l�nea de arriba a abajo
            await ScanLine.TranslateTo(0, 0, 0); // Regresa al inicio
            if (!_isAnimating) break;
        }
    }

    // M�todo para detener la animaci�n
    private void StopScanningAnimation()
    {
        _isAnimating = false;
        ScanLine.TranslationY = 0;
    }

    private async Task StartLoadingAnimation()
    {
        _isLoadingAnimating = true;
        while (_isLoadingAnimating)
        {
            await Dot1.ScaleTo(1.5, 500);
            await Dot2.ScaleTo(1.5, 500);
            await Dot3.ScaleTo(1.5, 500);
            await Dot1.ScaleTo(1.0, 500);
            await Dot2.ScaleTo(1.0, 500);
            await Dot3.ScaleTo(1.0, 500);
            if (!_isLoadingAnimating) break;
        }
    }

    private void StopLoadingAnimation()
    {
        _isLoadingAnimating = false;
        Dot1.Scale = 1.0;
        Dot2.Scale = 1.0;
        Dot3.Scale = 1.0;
    }

    // MANEJAR EVENTOS DEL VIEWMODEL
    private async void OnMostrarResultado(object sender, ResEscanearMedicamento medicamento)
    {
        var infoPopup = new InformacionMedicamentoEscaneo(medicamento);


        // --- INICIO DE LA L�GICA DE INTEGRACI�N --- //
        async void ManejadorMedicamentoAgregado(object s, ResEscanearMedicamento med)
        { 
            if (_viewModel?.GuardarMedicamentoCommand != null)
            {
                await _viewModel.GuardarMedicamentoCommand.ExecuteAsync(med);
            }
        }

        infoPopup.MedicamentoAgregado += ManejadorMedicamentoAgregado;

        await this.ShowPopupAsync(infoPopup);

        infoPopup.MedicamentoAgregado -= ManejadorMedicamentoAgregado;

        _viewModel?.ReactivarEscaneo();

    }
    private async void OnMostrarError(object sender, string mensaje)
    {
        await DisplayAlert("Error", mensaje, "OK");

        // REACTIVAR ESCANEO DESPU�S DEL ERROR
        _viewModel?.ReactivarEscaneo();
    }


   
    // MANEJADOR MEJORADO PARA DETECCI�N DE C�DIGOS
    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        // Verificar que tenemos un ViewModel y resultados v�lidos
        if (_viewModel == null || e?.Results == null || !e.Results.Any())
        {
            Debug.WriteLine("No hay ViewModel o resultados de escaneo");
            return;
        }

        // Verificar si ya estamos procesando
        if (_viewModel.IsProcessingResult)
        {
            Debug.WriteLine("Ya se est� procesando un resultado, ignorando...");
            return;
        }

        var barcode = e.Results.FirstOrDefault();
        if (barcode == null || string.IsNullOrEmpty(barcode.Value))
        {
            Debug.WriteLine("C�digo de barras vac�o o nulo");
            return;
        }

        Debug.WriteLine($"C�digo detectado: {barcode.Value}");

        // Activar estado de escaneo cuando se detecta un c�digo
        if (_viewModel.IsScanning != true)
        {
            _viewModel.IsScanning = true;
        }


        // DETENER DETECCI�N ANTES DE PROCESAR
        _viewModel.DetenerEscaneo();

        // EJECUTAR EN EL HILO PRINCIPAL
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_viewModel.ProcesarCodigosDetectadosCommand.CanExecute(e))
            {
                await _viewModel.ProcesarCodigosDetectadosCommand.ExecuteAsync(e);
                // Desactivar estado de escaneo despu�s del procesamiento
                _viewModel.IsScanning = false;
            }
            else
            {
                Debug.WriteLine("No se puede ejecutar el comando de procesamiento");

                // Desactivar estado de escaneo si no se pudo procesar
                _viewModel.IsScanning = false;
                _viewModel.ReactivarEscaneo(); // Reactivar si no se pudo procesar
            }
        });
    }


   
    // MANEJADOR PARA EL BOT�N CANCELAR EN EL HEADER
    private async void Cancelar_Clicked_Header(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== BOT�N CANCELAR PRESIONADO (HEADER) ===");

        try
        {
            if (_viewModel?.CancelarEscaneoCommand != null)
            {
                System.Diagnostics.Debug.WriteLine("Ejecutando CancelarEscaneoCommand...");

                // CORRECCI�N: Usar Execute() en lugar de ExecuteAsync()
                _viewModel.CancelarEscaneoCommand.Execute(null);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("CancelarEscaneoCommand es null");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error ejecutando comando cancelar: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    // M�TODO PARA REACTIVAR MANUALMENTE EL ESCANEO (si necesitas un bot�n)
    private void ReactivarEscaneo_Clicked(object sender, EventArgs e)
    {
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
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
        base.OnDisappearing();
    }
}