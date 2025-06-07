using MediTrack.Frontend.ViewModels.PantallasInicio;
using System.Threading;

namespace MediTrack.Frontend.Vistas.PantallasInicio
{
    public partial class PantallaCarga : BaseContentPage
    {
        private CancellationTokenSource? _animationCts;

        public PantallaCarga()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Iniciar animación del logo
            _animationCts = new CancellationTokenSource();
            _ = AnimarLogo(_animationCts.Token);

            // Inicializar ViewModel cuando aparece la pantalla
            if (BindingContext is CargaViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Detener animación
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;

            // Limpiar recursos del ViewModel
            if (BindingContext is CargaViewModel viewModel)
            {
                viewModel.Cleanup();
            }
        }

        private async Task AnimarLogo(CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si la imagen existe, si no mostrar texto fallback
                await Task.Delay(100); // Dar tiempo para que cargue la imagen

                if (logoImage.Source == null)
                {
                    logoImage.IsVisible = false;
                    fallbackText.IsVisible = true;
                }

                const uint duracion = 1000; // Más lento: 1 segundo por palpitar
                int palpitares = 0;
                const int minPalpitares = 3; // Al menos 3 palpitares

                while (!cancellationToken.IsCancellationRequested && palpitares < minPalpitares)
                {
                    // Palpitar hacia arriba (más grande y más lento)
                    if (logoImage.IsVisible)
                    {
                        await logoImage.ScaleTo(1.15, duracion, Easing.CubicInOut);
                    }
                    else
                    {
                        await fallbackText.ScaleTo(1.15, duracion, Easing.CubicInOut);
                    }

                    if (cancellationToken.IsCancellationRequested) break;

                    // Palpitar hacia abajo (tamaño normal)
                    if (logoImage.IsVisible)
                    {
                        await logoImage.ScaleTo(1.0, duracion, Easing.CubicInOut);
                    }
                    else
                    {
                        await fallbackText.ScaleTo(1.0, duracion, Easing.CubicInOut);
                    }

                    palpitares++;

                    // Pequeña pausa entre palpitares
                    await Task.Delay(200, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Esperado cuando se cancela la animación
                System.Diagnostics.Debug.WriteLine("Animación cancelada correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en animación: {ex.Message}");
            }
        }
    }
}