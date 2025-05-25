using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.PantallasInicio;

public partial class PantallaOlvidoContrasena : ContentPage
{
    public PantallaOlvidoContrasena()
    {
        InitializeComponent();
    }

    private async void EnviarEnlace(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(entryCorreo.Text))
        {
            await DisplayAlert("Campo vacío", "Por favor, ingrese su correo electrónico.", "OK");
            return;
        }

        // Aquí se implementaría la lógica real para enviar el enlace
        await DisplayAlert("¡Listo!", $"Se ha enviado un enlace a {entryCorreo.Text}.", "OK");
    }

    private async void VolverInicioSesion(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//inicioSesion");
    }
}
