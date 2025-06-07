using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;

namespace MediTrack.Frontend.ViewModels.PantallasInicio
{
    public partial class NuevaContrasenaViewModel : ObservableObject
    {
        // --- Propiedades para la UI --- //
        [ObservableProperty] private string _nuevaContrasena = string.Empty;
        [ObservableProperty] private string _confirmarContrasena = string.Empty;
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private string _mensajeEstado = string.Empty;
        [ObservableProperty] private string _emailUsuario = string.Empty;

        // --- Comandos --- //
        public IAsyncRelayCommand CambiarContrasenaCommand { get; }
        public IAsyncRelayCommand CerrarModalCommand { get; }

        // --- Eventos para comunicar a la Vista --- //
        public event EventHandler<string> ContrasenaActualizada; // Pasa mensaje de éxito
        public event EventHandler ModalCerrado;
        public event EventHandler<string> ActualizacionFallida;

        // Constructor
        public NuevaContrasenaViewModel()
        {
            // Inicializar comandos
            CambiarContrasenaCommand = new AsyncRelayCommand(EjecutarCambiarContrasena, PuedeCambiarContrasena);
            CerrarModalCommand = new AsyncRelayCommand(EjecutarCerrarModal);
        }

        // --- Métodos de los comandos --- //
        private bool PuedeCambiarContrasena()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(NuevaContrasena) &&
                   !string.IsNullOrWhiteSpace(ConfirmarContrasena) &&
                   NuevaContrasena.Length >= 6;
        }

        private async Task EjecutarCambiarContrasena()
        {
            if (IsLoading) return;

            // Validar que las contraseñas coincidan
            if (NuevaContrasena != ConfirmarContrasena)
            {
                ActualizacionFallida?.Invoke(this, "Las contraseñas no coinciden");
                return;
            }

            // Validar longitud mínima
            if (NuevaContrasena.Length < 6)
            {
                ActualizacionFallida?.Invoke(this, "La contraseña debe tener al menos 6 caracteres");
                return;
            }

            IsLoading = true;
            MensajeEstado = "Actualizando contraseña...";

            try
            {
                // Simular actualización de contraseña
                await Task.Delay(2000); // Simular llamada al backend

                MensajeEstado = "¡Contraseña actualizada!";
                await Task.Delay(500);

                // Disparar evento de contraseña actualizada exitosamente
                ContrasenaActualizada?.Invoke(this, "Tu contraseña ha sido actualizada correctamente");
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al actualizar";
                ActualizacionFallida?.Invoke(this, "Error al actualizar la contraseña. Intente nuevamente.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EjecutarCerrarModal()
        {
            ModalCerrado?.Invoke(this, EventArgs.Empty);
        }

        // --- Métodos públicos --- //
        public void InicializarConEmail(string email)
        {
            EmailUsuario = email;
            LimpiarFormulario();
        }

        public void LimpiarFormulario()
        {
            NuevaContrasena = string.Empty;
            ConfirmarContrasena = string.Empty;
            MensajeEstado = string.Empty;
            IsLoading = false;
        }

        // --- Validación en tiempo real --- //
        public string ValidarContrasenas()
        {
            if (string.IsNullOrWhiteSpace(NuevaContrasena))
                return string.Empty;

            if (NuevaContrasena.Length < 6)
                return "Mínimo 6 caracteres";

            if (!string.IsNullOrWhiteSpace(ConfirmarContrasena))
            {
                if (NuevaContrasena != ConfirmarContrasena)
                    return "Las contraseñas no coinciden";
                else
                    return "✓ Las contraseñas coinciden";
            }

            return string.Empty;
        }

        // Métodos parciales para notificar cambios en CanExecute
        partial void OnNuevaContrasenaChanged(string value)
        {
            CambiarContrasenaCommand?.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(MensajeValidacion));
        }

        partial void OnConfirmarContrasenaChanged(string value)
        {
            CambiarContrasenaCommand?.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(MensajeValidacion));
        }

        partial void OnIsLoadingChanged(bool value)
        {
            CambiarContrasenaCommand?.NotifyCanExecuteChanged();
        }

        // Propiedad computada para mostrar validación en tiempo real
        public string MensajeValidacion => ValidarContrasenas();
    }
}