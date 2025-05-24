using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.PantallasInicio;

public partial class PantallaRegistro : ContentPage
{
    public PantallaRegistro()
    {
        InitializeComponent();
    }

    private async void IrAInicioSesion(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//inicioSesion");
    }
}