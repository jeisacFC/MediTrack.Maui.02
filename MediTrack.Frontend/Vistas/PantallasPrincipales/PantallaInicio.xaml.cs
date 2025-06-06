using MediTrack.Frontend.ViewModels.PantallasPrincipales;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaInicio : ContentPage
{
    public PantallaInicio()
    {
        InitializeComponent();
        BindingContext = new InicioViewModel();
    }

    
}