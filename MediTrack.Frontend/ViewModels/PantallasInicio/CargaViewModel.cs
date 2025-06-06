using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.ViewModels.Base;
using System.Threading;

namespace MediTrack.Frontend.ViewModels.PantallasInicio
{
    public partial class CargaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private bool isAnimating = true;

        private CancellationTokenSource? _transitionCts;

        public CargaViewModel()
        {
            Title = "Cargando MediTrack";
        }

        public override async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                _transitionCts = new CancellationTokenSource();
                await IniciarTransicion(_transitionCts.Token);
            });
        }

        private async Task IniciarTransicion(CancellationToken cancellationToken)
        {
            try
            {
                // Tiempo de carga más largo para ver al menos 2-3 palpitares
                // 1 palpitar = ~2.2 segundos (1s subir + 1s bajar + 0.2s pausa)
                // 3 palpitares = ~6.6 segundos
                await Task.Delay(5000, cancellationToken); // 7 segundos total

                if (!cancellationToken.IsCancellationRequested)
                {
                    // Detener animación
                    IsAnimating = false;

                    // Navegar a bienvenida
                    await NavegarABienvenida();
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Transición cancelada correctamente");
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
            }
        }

        private async Task NavegarABienvenida()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//bienvenida");
                });

                System.Diagnostics.Debug.WriteLine("Navegación a bienvenida completada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a bienvenida: {ex.Message}");
                // Fallback: intentar navegación directa
                await Shell.Current.GoToAsync("//bienvenida");
            }
        }

        // Método para limpiar recursos cuando se destruye el ViewModel
        public void Cleanup()
        {
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = null;
        }

        // Comando para saltar la carga (opcional)
        [RelayCommand]
        private async Task SaltarCarga()
        {
            _transitionCts?.Cancel();
            await NavegarABienvenida();
        }
    }
}