namespace MediTrack.Frontend.Vistas;

public partial class PantallaCarga : ContentPage
{
    public PantallaCarga()
    {
        InitializeComponent();
        IniciarTemporizador();
    }

    private async void IniciarTemporizador()
    {
        await Task.Delay(2000);
        await Shell.Current.GoToAsync("//bienvenida");
    }
}