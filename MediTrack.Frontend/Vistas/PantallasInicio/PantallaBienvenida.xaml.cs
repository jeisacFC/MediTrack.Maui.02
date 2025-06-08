using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace MediTrack.Frontend.Vistas.PantallasInicio
{
    public partial class PantallaBienvenida : BaseContentPage
    {
        public PantallaBienvenida()
        {
            InitializeComponent();
        }

        private async void IrAInicioSesion(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//inicioSesion");

        private async void IrARegistro(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//registro");

        private async void IrAPantallaEscaneo(object sender, EventArgs e)
        {
            // "pantalla-escaneo" debe ser el nombre de la RUTA (Route)
            // que definiste para PantallaScan.xaml en tu AppShell.xaml
            await Shell.Current.GoToAsync("pantallaScan");

        }
    }
}
