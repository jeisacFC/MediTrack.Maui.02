using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas;

public partial class PantallaInicioSesion : ContentPage
{
    public PantallaInicioSesion()
    {
        InitializeComponent();
    }

    private async void IrARegistro(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//registro");
    }

    private async void IniciarSesion(object sender, EventArgs e)
    {
        // Aquí deberías validar los campos antes
        await DisplayAlert("Inicio de sesión", "Aquí iría la lógica de inicio de sesión.", "OK");
    }
}
