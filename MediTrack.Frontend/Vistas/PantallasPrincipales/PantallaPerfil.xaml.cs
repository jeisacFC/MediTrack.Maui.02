using System;
using System.Linq;
using MediTrack.Modelos;
using MediTrack.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using MediTrack.Vistas.PantallasPrincipales;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales
{
    public partial class PantallaPerfil : ContentPage
    {
        public PantallaPerfil()
        {
            InitializeComponent();
            BindingContext = new PerfilViewModel();
        }

        private void OnPadecimientosSeleccionados(object sender, SelectionChangedEventArgs e)
        {
            var seleccionados = e.CurrentSelection.Cast<ElementoSeleccionable>().ToList();
            // Procesa la selección...
        }

        private void OnAlergiasSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            var seleccionados = e.CurrentSelection.Cast<ElementoSeleccionable>().ToList();
            // Procesa la selección...
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Cerrar sesión",
                                                "¿Deseas salir de tu cuenta?",
                                                "Sí", "Cancelar");
            if (!confirmar)
                return;

            // Limpia datos de sesión si usas Preferences o SecureStorage
            // Preferences.Clear();

            // Navega a la ruta de login definida en tu AppShell
            await Shell.Current.GoToAsync("//inicioSesion");
        }
    }
}