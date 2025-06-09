using MediTrack.Frontend.ViewModels.PantallasInicio;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Vistas.Base;
using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.PantallasInicio;

public partial class PantallaInicioSesion : BaseContentPage
{
    private LoginViewModel _viewModel;

    public PantallaInicioSesion(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Suscribirse a los eventos del ViewModel
        viewModel.LoginExitoso += OnLoginExitoso;
        viewModel.LoginFallido += OnLoginFallido;
    }

    // Manejadores de eventos del ViewModel
    private async void OnLoginExitoso(object sender, ResLogin respuesta)
    {
        await DisplayAlert(
            "¡Bienvenido!",
            $"Hola {respuesta.Nombre} {respuesta.Apellido1}",
            "OK");

        // TODO: Aquí deberías guardar el token y la información del usuario
        // Por ejemplo, en Preferences o en un servicio de sesión

        // Navegar a la pantalla principal
        await Shell.Current.GoToAsync("//inicio");
    }

    private async void OnLoginFallido(object sender, string mensaje)
    {
        await DisplayAlert("Error de inicio de sesión", mensaje, "OK");
    }

    // Limpiar eventos al salir de la página
    protected override void OnDisappearing()
    {
        if (_viewModel != null)
        {
            _viewModel.LoginExitoso -= OnLoginExitoso;
            _viewModel.LoginFallido -= OnLoginFallido;
        }
        base.OnDisappearing();
    }

    // MANTENER estos métodos por compatibilidad con el XAML actual
    // pero que deleguen al ViewModel
    private async void IrARegistro(object sender, EventArgs e)
    {
        if (_viewModel?.IrARegistroCommand?.CanExecute(null) == true)
            await _viewModel.IrARegistroCommand.ExecuteAsync(null);
    }

    private async void IniciarSesion(object sender, EventArgs e)
    {
        if (_viewModel?.LoginCommand?.CanExecute(null) == true)
            await _viewModel.LoginCommand.ExecuteAsync(null);
    }

    private async void IrAOlvidoContrasena(object sender, EventArgs e)
    {
        if (_viewModel?.IrAOlvidoContraseñaCommand?.CanExecute(null) == true)
            await _viewModel.IrAOlvidoContraseñaCommand.ExecuteAsync(null);
    }
}