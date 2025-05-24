namespace MediTrack.Frontend.Vistas.PantallasInicio;

public partial class PantallaBienvenida : ContentPage
{
    public PantallaBienvenida()
    {
        InitializeComponent();
    }

    private async void IrAInicioSesion(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//inicioSesion");
    }

    private async void IrARegistro(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///registro");
    }
}