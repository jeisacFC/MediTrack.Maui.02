using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace MediTrack.Frontend.Vistas.PantallasInicio
{
    public partial class PantallaBienvenida : ContentPage
    {
        public PantallaBienvenida()
        {
            InitializeComponent();

            ContainerFrame.SizeChanged += (s, e) =>
            {
                if (ContainerFrame.Width > 0 && ContainerFrame.Height > 0)
                {
                    ContainerClip.Rect = new Rect(
                        0, 0,
                        ContainerFrame.Width,
                        ContainerFrame.Height
                    );
                }
            };
        }

        private async void IrAInicioSesion(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//inicioSesion");

        private async void IrARegistro(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//registro");
    }
}
