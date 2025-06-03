using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.PantallasInicio
{
    public partial class PantallaCarga : ContentPage
    {
        private CancellationTokenSource _cts;

        public PantallaCarga()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _cts = new();
            _ = AnimarLatido(_cts.Token);  // lanza sin bloquear
            _ = CargarDatos();             // simulación de carga
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cts?.Cancel();                // detiene la animación si salgo antes
        }

        /// <summary>Escala el logo de 1.0 → 1.15 → 1.0 en bucle.</summary>
        private async Task AnimarLatido(CancellationToken token)
        {
            const uint dur = 450;          // ms por medio ciclo

            while (!token.IsCancellationRequested)
            {
                await logo.ScaleTo(1.15, dur, Easing.SinInOut);
                await logo.ScaleTo(1.00, dur, Easing.SinInOut);
            }
        }

        /// <summary>Simula carga y navega a la pantalla de bienvenida.</summary>
        private async Task CargarDatos()
        {
            await Task.Delay(3000);                         // aquí tu lógica real
            await Shell.Current.GoToAsync("//bienvenida");  // ruta de ejemplo
        }
    }
}
