using MediTrack.Frontend.ViewModels;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaInicio : ContentPage
{
    public PantallaInicio()
    {
        InitializeComponent();
        BindingContext = new PantallaInicioViewModel();
    }
}