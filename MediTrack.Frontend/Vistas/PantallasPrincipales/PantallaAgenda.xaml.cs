using MediTrack.Frontend.ViewModels;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaAgenda : ContentPage
{
    public PantallaAgenda()
    {
        InitializeComponent();
        BindingContext = new AgendaViewModel();
    }

    private void AgregarEvento_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Agregar Evento", "Aqu� se abrir�a la pantalla para agregar un nuevo evento.", "OK");
    }
}