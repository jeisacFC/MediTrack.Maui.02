using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using MediTrack.Frontend.Vistas.Base;

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
            System.Diagnostics.Debug.WriteLine("=== INICIO IrAPantallaEscaneo ===");
            try
            {
                System.Diagnostics.Debug.WriteLine($"Página actual antes de guardar: {Shell.Current.CurrentState.Location}");
                NavigationService.GuardarPaginaActual();

                System.Diagnostics.Debug.WriteLine("Navegando a escaneo...");
                await NavigationService.GoToAsync("///pantallaScan");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando: {ex.Message}");
            }
        }
    }
}
