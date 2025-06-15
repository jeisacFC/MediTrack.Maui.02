using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.ViewModels;
using System.Diagnostics;

namespace MediTrack.Frontend.Popups
{
    public partial class ActualizarPerfilPopup : Popup
    {
        private readonly ActualizarPerfilPopupViewModel _viewModel;

        public ActualizarPerfilPopup(ActualizarPerfilPopupViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Suscribirse al evento de actualizaci�n completada
            _viewModel.OnActualizacionCompletada += OnActualizacionCompletada;
        }

        private async void OnActualizacionCompletada(object sender, bool exitoso)
        {
            try
            {
                if (exitoso)
                {
                    Debug.WriteLine("=== ACTUALIZACI�N COMPLETADA EXITOSAMENTE ===");

                    // Mostrar mensaje de �xito
                    await Application.Current.MainPage.DisplayAlert("�xito",
                        "Tu perfil ha sido actualizado correctamente", "OK");

                    // Cerrar el popup y devolver true para indicar que se actualiz�
                    await CloseAsync(true);
                }
                else
                {
                    Debug.WriteLine("=== ACTUALIZACI�N FALL� ===");
                    // El error ya se mostr� en el ViewModel, no cerramos el popup
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnActualizacionCompletada: {ex.Message}");
            }
        }

        private async void OnCerrarClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("=== CERRANDO POPUP SIN GUARDAR ===");
                await CloseAsync(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cerrar popup: {ex.Message}");
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("=== CANCELANDO EDICI�N ===");

                // Verificar si hay cambios pendientes
                bool hayCambios = _viewModel.Nombre != (_viewModel.GetUsuarioOriginal()?.nombre ?? string.Empty) ||
                                 _viewModel.Apellido1 != (_viewModel.GetUsuarioOriginal()?.apellido1 ?? string.Empty) ||
                                 _viewModel.Apellido2 != (_viewModel.GetUsuarioOriginal()?.apellido2 ?? string.Empty) ||
                                 _viewModel.FechaNacimiento != _viewModel.GetUsuarioOriginal()?.fecha_nacimiento ||
                                 _viewModel.GeneroSeleccionado?.Id != _viewModel.GetUsuarioOriginal()?.id_genero;

                if (hayCambios)
                {
                    bool confirmar = await Application.Current.MainPage.DisplayAlert(
                        "Confirmar",
                        "�Est�s seguro de que deseas cancelar? Se perder�n los cambios realizados.",
                        "S�, cancelar",
                        "No");

                    if (!confirmar) return;
                }

                await CloseAsync(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cancelar: {ex.Message}");
                await CloseAsync(false);
            }
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
        }

        // Cleanup cuando se destruye el popup
        ~ActualizarPerfilPopup()
        {
            if (_viewModel != null)
            {
                _viewModel.OnActualizacionCompletada -= OnActualizacionCompletada;
            }
        }
    }
}