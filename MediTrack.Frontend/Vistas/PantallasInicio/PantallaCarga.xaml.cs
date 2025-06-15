using MediTrack.Frontend.ViewModels.PantallasInicio;
using System.Threading;
using MediTrack.Frontend.Vistas.Base;

namespace MediTrack.Frontend.Vistas.PantallasInicio
{
    public partial class PantallaCarga : BaseContentPage
    {
        private CancellationTokenSource? _animationCts;
        private readonly CargaViewModel _viewModel;

        public PantallaCarga(CargaViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // INICIALIZACIÓN OPTIMIZADA
            await InicializarPantallaOptimizada();
        }

        private async Task InicializarPantallaOptimizada()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PantallaCarga] Inicializando pantalla...");

                // Iniciar animación más eficiente
                _animationCts = new CancellationTokenSource();
                var animationTask = AnimarLogoOptimizado(_animationCts.Token);

                // Inicializar ViewModel en paralelo
                var viewModelTask = InicializarViewModel();

                // Esperar ambas tareas
                await Task.WhenAll(animationTask, viewModelTask);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PantallaCarga] Error en inicialización: {ex.Message}");
                // Fallback: continuar con navegación
                if (_viewModel != null)
                {
                    await _viewModel.InitializeAsync();
                }
            }
        }

        private async Task InicializarViewModel()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PantallaCarga] Inicializando ViewModel...");
                if (_viewModel != null)
                {
                    await _viewModel.InitializeAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PantallaCarga] Error inicializando ViewModel: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Cleanup optimizado
            CleanupResources();
        }

        private void CleanupResources()
        {
            try
            {
                // Detener animación
                _animationCts?.Cancel();
                _animationCts?.Dispose();
                _animationCts = null;

                // Limpiar ViewModel
                if (_viewModel != null)
                {
                    _viewModel.Cleanup();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PantallaCarga] Error en cleanup: {ex.Message}");
            }
        }

        private async Task AnimarLogoOptimizado(CancellationToken cancellationToken)
        {
            try
            {
                // Verificación rápida de imagen
                await Task.Delay(50, cancellationToken);

                if (logoImage.Source == null)
                {
                    logoImage.IsVisible = false;
                    fallbackText.IsVisible = true;
                }

                // ANIMACIÓN MÁS RÁPIDA Y EFICIENTE
                const uint duracion = 800; // Más rápido: 800ms por palpitar
                int palpitares = 0;
                const int maxPalpitares = 3; // Máximo 3 palpitares (2.4s total)

                while (!cancellationToken.IsCancellationRequested && palpitares < maxPalpitares)
                {
                    // SOLUCIÓN: Animar cada elemento por separado
                    if (logoImage.IsVisible)
                    {
                        // Animación de escala más suave para la imagen
                        await logoImage.ScaleTo(1.1, duracion / 2, Easing.CubicOut);

                        if (cancellationToken.IsCancellationRequested) break;

                        await logoImage.ScaleTo(1.0, duracion / 2, Easing.CubicIn);
                    }
                    else
                    {
                        // Animación de escala más suave para el texto
                        await fallbackText.ScaleTo(1.1, duracion / 2, Easing.CubicOut);

                        if (cancellationToken.IsCancellationRequested) break;

                        await fallbackText.ScaleTo(1.0, duracion / 2, Easing.CubicIn);
                    }

                    palpitares++;

                    // Sin pausa entre palpitares para mayor fluidez
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("[PantallaCarga] Animación cancelada correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PantallaCarga] Error en animación: {ex.Message}");
            }
        }
    }
}