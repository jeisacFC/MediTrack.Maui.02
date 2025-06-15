using MediTrack.Frontend.ViewModels.PantallasInicio;
using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Popups;

public partial class ModalRecuperarContrasena : ContentPage
{
    private RecuperarContrasenaViewModel _viewModel;

    public ModalRecuperarContrasena(RecuperarContrasenaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Suscribirse a los eventos del ViewModel
        viewModel.ProcesoCompletado += OnProcesoCompletado;
        viewModel.ProcesoFallido += OnProcesoFallido;
        viewModel.ModalCerrado += OnModalCerrado;
    }

    // Eventos públicos para comunicar con la pantalla padre
    public event EventHandler<string> RecuperacionCompletada;
    public event EventHandler ModalCancelado;

    // Manejadores de eventos del ViewModel
    private async void OnProcesoCompletado(object sender, string mensaje)
    {
        // Mostrar mensaje de éxito
        await DisplayAlert("¡Proceso completado!", mensaje, "OK");

        // Cerrar este modal
        await Navigation.PopModalAsync();

        // Notificar a la pantalla padre que el proceso fue completado
        RecuperacionCompletada?.Invoke(this, mensaje);
    }

    private async void OnProcesoFallido(object sender, string mensaje)
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
            _viewModel.ProcesoCompletado -= OnProcesoCompletado;
            _viewModel.ProcesoFallido -= OnProcesoFallido;
            _viewModel.ModalCerrado -= OnModalCerrado;
        }
        base.OnDisappearing();
    }

    // Override del botón back de Android para manejar correctamente el cierre
    protected override bool OnBackButtonPressed()
    {
        // Si está en el segundo paso, volver al primero
        if (_viewModel?.EstaCambiandoContrasena == true)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await _viewModel.VolverAtrasCommand.ExecuteAsync(null);
            });
        }
        else
        {
            // Si está en el primer paso, cerrar modal
            Device.BeginInvokeOnMainThread(async () =>
            {
                await _viewModel.CerrarModalCommand.ExecuteAsync(null);
            });
        }
        return true; // Indicar que manejamos el evento
    }
}