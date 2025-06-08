using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas;

public partial class BotonFlotanteEscaner : ContentView
{
    public BotonFlotanteEscaner()
    {
        InitializeComponent();
    }

    private async void OnFabClicked(object sender, EventArgs e)
    {
        // Animación del botón
        await FabButton.ScaleTo(0.8, 100);
        await FabButton.ScaleTo(1.0, 100);
        
        // Navegar a la pantalla de escaneo
        await Shell.Current.GoToAsync("//pantallaScan");
    }
}