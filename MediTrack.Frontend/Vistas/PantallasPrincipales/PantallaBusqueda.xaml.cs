using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Popups; // Asumiendo que crear�s un popup para los resultados
using MediTrack.Frontend.ViewModels.PantallasPrincipales;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaBusqueda : ContentPage
{
    private BusquedaViewModel _viewModel;

    public PantallaBusqueda(BusquedaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel; // Usar directamente el par�metro como en PantallaScan

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
        // Aqu� es donde mostramos el modal con la informaci�n del medicamento
        // Por ahora, usaremos una alerta para confirmar que funciona.
        // En el siguiente paso, crearemos y mostraremos el popup.
        await DisplayAlert(
            resultado.Medicamento.NombreComercial,
            $"Se encontr� el medicamento. Principio Activo: {resultado.Medicamento.PrincipioActivo}",
            "OK");
    }

    private async void OnBusquedaFallida(object sender, string mensajeError)
    {
        await DisplayAlert("Error de B�squeda", mensajeError, "OK");
    }
}