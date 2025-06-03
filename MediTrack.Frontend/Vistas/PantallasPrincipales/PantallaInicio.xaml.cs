using MediTrack.Frontend.ViewModels;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaInicio : ContentPage
{
    public PantallaInicio()
    {
        InitializeComponent();
        BindingContext = new PantallaInicioViewModel();
    }

    private async void BotonEscanear_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("pantallaScan"); // Usa la ruta definida en AppShell
    }
}