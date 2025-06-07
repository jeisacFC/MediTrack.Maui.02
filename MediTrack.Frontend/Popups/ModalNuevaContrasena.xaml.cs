using MediTrack.Frontend.ViewModels.PantallasInicio;
using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Popups;

public partial class ModalNuevaContrasena : ContentPage
{
    private NuevaContrasenaViewModel _viewModel;

    public ModalNuevaContrasena(NuevaContrasenaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Suscribirse a los eventos del ViewModel
        viewModel.ContrasenaActualizada += OnContrasenaActualizada;
        viewModel.ActualizacionFallida += OnActualizacionFallida;
        viewModel.ModalCerrado += OnModalCerrado;
    }

    // Eventos públicos para comunicar con la pantalla padre
    public event EventHandler<string> ContrasenaActualizadaExitosamente;
    public event EventHandler ModalCancelado;

    // Manejadores de eventos del ViewModel
    private async void OnContrasenaActualizada(object sender, string mensaje)
    {
        // Mostrar mensaje de éxito
        await DisplayAlert("¡Éxito!", mensaje, "OK");

        // Notificar a la pantalla padre que la contraseña fue actualizada
        ContrasenaActualizadaExitosamente?.Invoke(this, mensaje);

        // Cerrar este modal
        await Navigation.PopModalAsync();
    }

    private async void OnActualizacionFallida(object sender, string mensaje)
    {
        await DisplayAlert("Error", mensaje, "OK");
    }

    private async void OnModalCerrado(object sender, EventArgs e)
    {
        // Notificar a la pantalla padre que se canceló
        ModalCancelado?.Invoke(this, EventArgs.Empty);

        // Cerrar modal
        await Navigation.PopModalAsync();
    }

    // Método público para inicializar el modal con el email
    public void InicializarConEmail(string email)
    {
        _viewModel?.InicializarConEmail(email);
    }

    // Limpiar eventos al salir de la página
    protected override void OnDisappearing()
    {
        if (_viewModel != null)
        {
            _viewModel.ContrasenaActualizada -= OnContrasenaActualizada;
            _viewModel.ActualizacionFallida -= OnActualizacionFallida;
            _viewModel.ModalCerrado -= OnModalCerrado;
        }
        base.OnDisappearing();
    }

    // MANTENER estos métodos por compatibilidad con el XAML actual
    // pero que deleguen al ViewModel
    private async void CambiarContrasena(object sender, EventArgs e)
    {
        if (_viewModel?.CambiarContrasenaCommand?.CanExecute(null) == true)
            await _viewModel.CambiarContrasenaCommand.ExecuteAsync(null);
    }

    private async void CerrarModal(object sender, EventArgs e)
    {
        if (_viewModel?.CerrarModalCommand?.CanExecute(null) == true)
            await _viewModel.CerrarModalCommand.ExecuteAsync(null);
    }

    // Override del botón back de Android para manejar correctamente el cierre
    protected override bool OnBackButtonPressed()
    {
        // Ejecutar el comando de cerrar modal
        if (_viewModel?.CerrarModalCommand?.CanExecute(null) == true)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await _viewModel.CerrarModalCommand.ExecuteAsync(null);
            });
        }
        return true; // Indicar que manejamos el evento
    }
}