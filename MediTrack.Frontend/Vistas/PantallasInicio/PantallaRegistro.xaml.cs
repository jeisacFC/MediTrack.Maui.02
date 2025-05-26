using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.PantallasInicio
{
    public partial class PantallaRegistro : ContentPage
    {
        public PantallaRegistro()
        {
            InitializeComponent();
        }

        // Evento atado al botón “Registrarse”
        private async void OnRegistrarseClicked(object sender, EventArgs e)
        {
            // Aquí iría la lógica de validación/registro,
            // llamando a un servicio o ViewModel:
            //
            // var éxito = await _viewModel.RegistrarUsuarioAsync();
            // if (!éxito) { /* mostrar alerta */ return; }

            // Una vez registrado, redirige a la pantalla de inicio de sesión:
            await Shell.Current.GoToAsync("//inicioSesion");
        }

        // Evento atado al botón “Iniciar sesión” en la parte inferior
        private async void IrAInicioSesion(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//inicioSesion");
        }
    }
}
