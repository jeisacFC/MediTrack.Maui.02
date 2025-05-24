namespace MediTrack.Frontend.Vistas;

public partial class PantallaBienvenida : ContentPage
{
    public PantallaBienvenida()
    {
        InitializeComponent();
    }

    private async void IrAInicioSesion(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///inicio-sesion");
    }

    private async void IrARegistro(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///registro");
    }
}