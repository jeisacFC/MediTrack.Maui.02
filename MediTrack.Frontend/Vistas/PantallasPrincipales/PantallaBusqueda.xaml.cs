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
        // ¡ESTA ES LA LÍNEA QUE FALTABA!
        BindingContext = _viewModel;

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Suscribirse a los eventos del ViewModel cuando la página aparece
        _viewModel.BusquedaExitosa += OnBusquedaExitosa;
        _viewModel.BusquedaFallida += OnBusquedaFallida;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Des-suscribirse de los eventos para evitar fugas de memoria
        _viewModel.BusquedaExitosa -= OnBusquedaExitosa;
        _viewModel.BusquedaFallida -= OnBusquedaFallida;
    }

    private async void OnBusquedaExitosa(object sender, ResBuscarMedicamento resultado)
    {
        // Aquí es donde mostramos el modal con la información del medicamento
        // Por ahora, usaremos una alerta para confirmar que funciona.
        // En el siguiente paso, crearemos y mostraremos el popup.
        await DisplayAlert(
            resultado.Medicamento.NombreComercial,
            $"Se encontró el medicamento. Principio Activo: {resultado.Medicamento.PrincipioActivo}",
            "OK");
    }

    private async void OnBusquedaFallida(object sender, string mensajeError)
    {
        await DisplayAlert("Error de Búsqueda", mensajeError, "OK");
    }
}