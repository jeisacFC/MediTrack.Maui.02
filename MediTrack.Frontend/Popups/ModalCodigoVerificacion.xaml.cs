using MediTrack.Frontend.ViewModels.PantallasInicio;
using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Popups;

public partial class ModalCodigoVerificacion : ContentPage
{
    private CodigoVerificacionViewModel _viewModel;

    public ModalCodigoVerificacion(CodigoVerificacionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Suscribirse a los eventos del ViewModel
        viewModel.CodigoVerificado += OnCodigoVerificado;
        viewModel.VerificacionFallida += OnVerificacionFallida;
        viewModel.ModalCerrado += OnModalCerrado;
    }

    // Eventos p�blicos para comunicar con la pantalla padre
    public event EventHandler<string> CodigoVerificadoExitosamente;
    public event EventHandler ModalCancelado;

    // Manejadores de eventos del ViewModel
    // Manejadores de eventos del ViewModel - MODIFICAR ESTE M�TODO
    private async void OnCodigoVerificado(object sender, string email)
    {
        System.Diagnostics.Debug.WriteLine("OnCodigoVerificado: Cerrando modal de c�digo...");

        // Primero cerrar este modal
        await Navigation.PopModalAsync();

        System.Diagnostics.Debug.WriteLine("Modal de c�digo cerrado, disparando evento externo...");

        // Peque�o delay para que la UI se actualice
        await Task.Delay(100);

        // DESPU�S notificar a la pantalla padre que el c�digo fue verificado
        CodigoVerificadoExitosamente?.Invoke(this, email);

        System.Diagnostics.Debug.WriteLine("Evento CodigoVerificadoExitosamente enviado");
    }

    private async void OnVerificacionFallida(object sender, string mensaje)
    {
        await DisplayAlert("Error de verificaci�n", mensaje, "OK");
    }

    private async void OnModalCerrado(object sender, EventArgs e)
    {
        // Notificar a la pantalla padre que se cancel�
        ModalCancelado?.Invoke(this, EventArgs.Empty);

        // Cerrar modal
        await Navigation.PopModalAsync();
    }

    // M�todo p�blico para inicializar el modal con el email
    public void InicializarConEmail(string email)
    {
        _viewModel?.InicializarConEmail(email);
    }

    // Limpiar eventos al salir de la p�gina
    protected override void OnDisappearing()
    {
        if (_viewModel != null)
        {
            _viewModel.CodigoVerificado -= OnCodigoVerificado;
            _viewModel.VerificacionFallida -= OnVerificacionFallida;
            _viewModel.ModalCerrado -= OnModalCerrado;
        }
        base.OnDisappearing();
    }

    // MANTENER estos m�todos por compatibilidad con el XAML actual
    // pero que deleguen al ViewModel
    private async void VerificarCodigo(object sender, EventArgs e)
    {
        if (_viewModel?.VerificarCodigoCommand?.CanExecute(null) == true)
            await _viewModel.VerificarCodigoCommand.ExecuteAsync(null);
    }

    private async void ReenviarCodigo(object sender, EventArgs e)
    {
        if (_viewModel?.ReenviarCodigoCommand?.CanExecute(null) == true)
            await _viewModel.ReenviarCodigoCommand.ExecuteAsync(null);
    }

    private async void CerrarModal(object sender, EventArgs e)
    {
        if (_viewModel?.CerrarModalCommand?.CanExecute(null) == true)
            await _viewModel.CerrarModalCommand.ExecuteAsync(null);
    }

    // Override del bot�n back de Android para manejar correctamente el cierre
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