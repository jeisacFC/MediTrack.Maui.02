using MediTrack.Frontend.ViewModels.PantallasInicio;
using MediTrack.Frontend.Popups;
using MediTrack.Frontend.Vistas.Base;
using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.PantallasInicio;

public partial class PantallaOlvidoContrasena : BaseContentPage
{
    private OlvidoContrasenaViewModel _viewModel;

    public PantallaOlvidoContrasena(OlvidoContrasenaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Suscribirse a los eventos del ViewModel
        viewModel.CodigoEnviado += OnCodigoEnviado;
        viewModel.EnvioFallido += OnEnvioFallido;
    }

    // Manejadores de eventos del ViewModel
    private async void OnCodigoEnviado(object sender, string email)
    {
        // Mostrar mensaje de éxito brevemente
        await DisplayAlert("¡Token enviado!", $"Se ha enviado un token de verificación a {email}", "OK");

        // Abrir modal unificado de recuperación
        await AbrirModalRecuperacion(email);
    }

    private async void OnEnvioFallido(object sender, string mensaje)
    {
        await DisplayAlert("Error", mensaje, "OK");
    }

    // Método para abrir el modal unificado de recuperación
    private async Task AbrirModalRecuperacion(string email)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== AbrirModalRecuperacion con email: {email} ===");

            var serviceProvider = Application.Current?.Handler?.MauiContext?.Services;

            if (serviceProvider == null)
            {
                await DisplayAlert("Error", "No se pudo acceder a los servicios", "OK");
                return;
            }

            var recuperarViewModel = serviceProvider.GetService<RecuperarContrasenaViewModel>();

            if (recuperarViewModel == null)
            {
                await DisplayAlert("Error", "No se pudo crear el ViewModel", "OK");
                return;
            }

            // Crear el modal unificado
            var modalRecuperacion = new ModalRecuperarContrasena(recuperarViewModel);

            // Inicializar el modal con el email
            modalRecuperacion.InicializarConEmail(email);

            // Suscribirse a los eventos del modal
            modalRecuperacion.RecuperacionCompletada += OnRecuperacionCompletada;
            modalRecuperacion.ModalCancelado += OnModalRecuperacionCancelado;

            // Mostrar el modal
            await Navigation.PushModalAsync(modalRecuperacion);

            System.Diagnostics.Debug.WriteLine("Modal de recuperación mostrado exitosamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en AbrirModalRecuperacion: {ex.Message}");
            await DisplayAlert("Error", $"No se pudo abrir el modal: {ex.Message}", "OK");
        }
    }

    // Manejadores de eventos del modal unificado
    private async void OnRecuperacionCompletada(object sender, string mensaje)
    {
        // Mostrar mensaje de éxito
        await DisplayAlert("¡Contraseña actualizada!",
            "Tu contraseña ha sido actualizada correctamente.",
            "OK");

        // Reiniciar la app para ir a login fresco
        System.Diagnostics.Debug.WriteLine("Reiniciando aplicación después de cambio de contraseña...");

        // Limpiar cualquier dato almacenado si es necesario
        try
        {
            SecureStorage.Remove("jwt_token");
            SecureStorage.Remove("refresh_token");
            SecureStorage.Remove("user_id");
            SecureStorage.Remove("user_email");
            SecureStorage.Remove("user_name");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error limpiando storage: {ex.Message}");
        }

        // Reiniciar la app
        Application.Current.MainPage = new AppShell();

        // Asegurar que vaya a login
        await Shell.Current.GoToAsync("//inicioSesion");
    }

    private void OnModalRecuperacionCancelado(object sender, EventArgs e)
    {
        // El usuario canceló el modal de recuperación
        System.Diagnostics.Debug.WriteLine("Modal de recuperación cancelado");
        // El modal ya se cerró automáticamente, no necesitamos hacer nada más
    }

    // Limpiar eventos al salir de la página
    protected override void OnDisappearing()
    {
        if (_viewModel != null)
        {
            _viewModel.CodigoEnviado -= OnCodigoEnviado;
            _viewModel.EnvioFallido -= OnEnvioFallido;
        }
        base.OnDisappearing();
    }

    // MANTENER estos métodos por compatibilidad con el XAML actual
    // pero que deleguen al ViewModel
    private async void EnviarEnlace(object sender, EventArgs e)
    {
        if (_viewModel?.EnviarCodigoCommand?.CanExecute(null) == true)
            await _viewModel.EnviarCodigoCommand.ExecuteAsync(null);
    }

    private async void VolverInicioSesion(object sender, EventArgs e)
    {
        if (_viewModel?.VolverInicioSesionCommand?.CanExecute(null) == true)
            await _viewModel.VolverInicioSesionCommand.ExecuteAsync(null);
    }
}