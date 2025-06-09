using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Popups; // Asumiendo que crearás un popup para los resultados
using MediTrack.Frontend.ViewModels.PantallasPrincipales;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaBusqueda : ContentPage
{
    private BusquedaViewModel _viewModel;

    public PantallaBusqueda(BusquedaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel; // Usar directamente el parámetro como en PantallaScan

        // SUSCRIBIRSE A LOS EVENTOS DEL VIEWMODEL (como en PantallaScan)
        viewModel.BusquedaExitosa += OnBusquedaExitosa;
        viewModel.BusquedaFallida += OnBusquedaFallida;
    }

    // LIMPIAR EVENTOS AL SALIR (como en PantallaScan)
    protected override void OnDisappearing()
    {
        if (_viewModel != null)
        {
            _viewModel.BusquedaExitosa -= OnBusquedaExitosa;
            _viewModel.BusquedaFallida -= OnBusquedaFallida;
        }
        base.OnDisappearing();
    }

    private async void OnBusquedaExitosa(object sender, ResBuscarMedicamento resultado)
    {
        var infoPopup = new InfoBusquedaManualPopup(resultado);

        async void ManejadorMedicamentoAgregado(object s, ResBuscarMedicamento medParaGuardar)
        {
            // 3. Llamamos al nuevo comando del ViewModel para que haga la magia
            if (_viewModel?.GuardarMedicamentoCommand != null)
            {
                await _viewModel.GuardarMedicamentoCommand.ExecuteAsync(medParaGuardar);
            }
        }
        infoPopup.MedicamentoAgregado += ManejadorMedicamentoAgregado;

        await this.ShowPopupAsync(infoPopup);

        infoPopup.MedicamentoAgregado -= ManejadorMedicamentoAgregado;

    }

    private async void OnBusquedaFallida(object sender, string mensajeError)
    {
        await DisplayAlert("Error de Búsqueda", mensajeError, "OK");
    }
}