using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.PantallasInicio;

public partial class PantallaInicioSesion : ContentPage
{
    public PantallaInicioSesion()
    {
        InitializeComponent();
    }

    private async void IrARegistro(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///registro");
    }

    private async void IniciarSesion(object sender, EventArgs e)
    {
        // Aqu� ir�a la validaci�n real m�s adelante
        // Por ahora, simulamos un inicio exitoso
        await Shell.Current.GoToAsync("//inicio");
    }
    private async void IrAOlvidoContrasena(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//olvidoContrasena");
    }
}
