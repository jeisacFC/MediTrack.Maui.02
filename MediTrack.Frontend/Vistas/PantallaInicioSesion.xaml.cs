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
        // Aqu� deber�as validar los campos antes
        await DisplayAlert("Inicio de sesi�n", "Aqu� ir�a la l�gica de inicio de sesi�n.", "OK");
    }
}
