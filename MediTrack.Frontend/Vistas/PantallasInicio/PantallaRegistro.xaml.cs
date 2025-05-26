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

        // Evento atado al bot�n �Registrarse�
        private async void OnRegistrarseClicked(object sender, EventArgs e)
        {
            // Aqu� ir�a la l�gica de validaci�n/registro,
            // llamando a un servicio o ViewModel:
            //
            // var �xito = await _viewModel.RegistrarUsuarioAsync();
            // if (!�xito) { /* mostrar alerta */ return; }

            // Una vez registrado, redirige a la pantalla de inicio de sesi�n:
            await Shell.Current.GoToAsync("//inicioSesion");
        }

        // Evento atado al bot�n �Iniciar sesi�n� en la parte inferior
        private async void IrAInicioSesion(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//inicioSesion");
        }
    }
}
