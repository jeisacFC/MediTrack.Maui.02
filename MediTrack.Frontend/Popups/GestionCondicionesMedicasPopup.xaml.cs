using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.ViewModels;
using MediTrack.Frontend.Models.Model;
using System.Diagnostics;
using System.Threading;

namespace MediTrack.Frontend.Popups
{
    public partial class GestionCondicionesMedicasPopup : Popup
    {
        private CondicionesMedicasViewModel _viewModel;
        private bool _cambiosRealizados = false;

        public GestionCondicionesMedicasPopup(CondicionesMedicasViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Suscribirse a eventos del ViewModel para detectar cambios
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // Lógica que antes estaba en OnOpened
            _ = InitializeViewModelAsync();
        }

        private async Task InitializeViewModelAsync()
        {
            try
            {
                Debug.WriteLine("=== GestionCondicionesMedicasPopup Opened ===");

                await _viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir popup: {ex.Message}");
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Error al cargar condiciones médicas", "OK");
                }
            }
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CondicionesMedicasViewModel.CondicionesUsuario))
            {
                _cambiosRealizados = true;
                Debug.WriteLine("=== Cambios detectados en condiciones del usuario ===");
            }
        }

        private void OnCondicionesSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Debug.WriteLine("=== Condiciones seleccionadas en popup ===");

                _viewModel.OnCondicionesSeleccionadas(sender, e);

                Debug.WriteLine($"Condiciones seleccionadas: {e.CurrentSelection.Count}");
                foreach (CondicionesMedicas condicion in e.CurrentSelection)
                {
                    Debug.WriteLine($"- {condicion.nombre_condicion}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnCondicionesSeleccionadas: {ex.Message}");
            }
        }

        private async void OnCerrarClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("=== Cerrando popup de condiciones médicas ===");

                if (_cambiosRealizados)
                {
                    Debug.WriteLine("=== Cambios realizados - cerrando con éxito ===");
                    await CloseAsync(true);
                }
                else
                {
                    Debug.WriteLine("=== Sin cambios - cerrando sin actualizar ===");
                    await CloseAsync(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cerrar popup: {ex.Message}");
                await CloseAsync(false);
            }
        }

        protected override async Task OnClosed(object? result, bool wasDismissedByTappingOutsideOfPopup, CancellationToken token)
        {
            base.OnClosed(result, wasDismissedByTappingOutsideOfPopup, token);

            try
            {
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                }

                Debug.WriteLine("=== GestionCondicionesMedicasPopup Closed ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnClosed: {ex.Message}");
            }
        }

        public async Task<bool> MostrarConfirmacionAsync(string titulo, string mensaje, string aceptar = "Sí", string cancelar = "No")
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(titulo, mensaje, aceptar, cancelar);
            }
            return false;
        }

        public async Task MostrarAlertaAsync(string titulo, string mensaje, string boton = "OK")
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(titulo, mensaje, boton);
            }
        }
    }
}
