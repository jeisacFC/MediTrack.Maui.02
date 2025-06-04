using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.PantallasInicio
{
    public partial class PantallaCarga : ContentPage
    {
        private CancellationTokenSource? _cts;

        public PantallaCarga()
        {
            InitializeComponent();
            // ELIMINAR: logo.EnableAnimations = true; ← Esta línea causaba el error
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _cts = new CancellationTokenSource();

            // Ejecutar animación y carga por separado para mejor control
            _ = AnimarLatidoOptimizado(_cts.Token);
            _ = InicializarAppRapido();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async Task AnimarLatidoOptimizado(CancellationToken token)
        {
            const uint duracion = 600; // Duración más suave

            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Animación más suave con easing mejorado
                    await logo.ScaleTo(1.08, duracion, Easing.CubicInOut);
                    if (token.IsCancellationRequested) break;

                    await logo.ScaleTo(1.0, duracion, Easing.CubicInOut);
                    if (token.IsCancellationRequested) break;
                }
            }
            catch (OperationCanceledException)
            {
                // Esperado cuando se cancela la animación
            }
            catch (Exception ex)
            {
                // Log cualquier otro error
                System.Diagnostics.Debug.WriteLine($"Error en animación: {ex.Message}");
            }
        }

        private async Task InicializarAppRapido()
        {
            try
            {
                // Delay reducido para carga más rápida
                await Task.Delay(600);

                // Navegar en el hilo principal
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//bienvenida");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en inicialización: {ex.Message}");
                // Fallback: intentar navegar directamente
                await Shell.Current.GoToAsync("//bienvenida");
            }
        }
    }
}