using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Interfaces;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace MediTrack.Frontend.ViewModels.PantallasInicio
{
    public enum EstadoRecuperacion
    {
        IngresandoToken,
        CambiandoContrasena
    }

    public partial class RecuperarContrasenaViewModel : ObservableObject
    {
        // --- Propiedades para el estado actual --- //
        [ObservableProperty] private EstadoRecuperacion _estadoActual = EstadoRecuperacion.IngresandoToken;
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private string _mensajeEstado = string.Empty;
        [ObservableProperty] private string _emailUsuario = string.Empty;

        // --- Propiedades para verificación de token --- //
        [ObservableProperty] private string _token = string.Empty;

        // --- Propiedades para nueva contraseña --- //
        [ObservableProperty] private string _nuevaContrasena = string.Empty;
        [ObservableProperty] private string _confirmarContrasena = string.Empty;

        // --- Comandos --- //
        public IAsyncRelayCommand ContinuarCommand { get; }
        public IAsyncRelayCommand RestablecerContrasenaCommand { get; }
        public IAsyncRelayCommand ReenviarTokenCommand { get; }
        public IAsyncRelayCommand CerrarModalCommand { get; }
        public IAsyncRelayCommand VolverAtrasCommand { get; }

        // --- Eventos para comunicar a la Vista --- //
        public event EventHandler<string> ProcesoCompletado; // Contraseña cambiada exitosamente
        public event EventHandler ModalCerrado;
        public event EventHandler<string> ProcesoFallido;

        // --- Dependencias --- //
        private readonly IApiService _apiService;

        // --- Propiedades computadas para la UI --- //
        public bool EstaIngresandoToken => EstadoActual == EstadoRecuperacion.IngresandoToken;
        public bool EstaCambiandoContrasena => EstadoActual == EstadoRecuperacion.CambiandoContrasena;

        public string TituloModal => EstaIngresandoToken ? "Verificar token" : "Nueva contraseña";
        public string SubtituloModal => EstaIngresandoToken
            ? $"Token enviado a {EmailUsuario}"
            : "Ingresa tu nueva contraseña";

        // Constructor
        public RecuperarContrasenaViewModel(IApiService apiService)
        {
            _apiService = apiService;

            // Inicializar comandos
            ContinuarCommand = new AsyncRelayCommand(EjecutarContinuar, PuedeContinuar);
            RestablecerContrasenaCommand = new AsyncRelayCommand(EjecutarRestablecerContrasena, PuedeRestablecerContrasena);
            ReenviarTokenCommand = new AsyncRelayCommand(EjecutarReenviarToken);
            CerrarModalCommand = new AsyncRelayCommand(EjecutarCerrarModal);
            VolverAtrasCommand = new AsyncRelayCommand(EjecutarVolverAtras);
        }

        // --- Métodos de validación --- //
        private bool PuedeContinuar()
        {
            return !IsLoading &&
                   EstadoActual == EstadoRecuperacion.IngresandoToken &&
                   !string.IsNullOrWhiteSpace(Token) &&
                   Token.Length >= 6; // Los tokens suelen tener al menos 6 caracteres
        }

        private bool PuedeRestablecerContrasena()
        {
            return !IsLoading &&
                   EstadoActual == EstadoRecuperacion.CambiandoContrasena &&
                   !string.IsNullOrWhiteSpace(NuevaContrasena) &&
                   !string.IsNullOrWhiteSpace(ConfirmarContrasena) &&
                   NuevaContrasena.Length >= 6 &&
                   NuevaContrasena == ConfirmarContrasena;
        }

        // --- Métodos de los comandos --- //
        private async Task EjecutarContinuar()
        {
            if (IsLoading) return;

            // Validar token
            if (string.IsNullOrWhiteSpace(Token) || Token.Length < 6)
            {
                ProcesoFallido?.Invoke(this, "Por favor ingresa un token válido");
                return;
            }

            // Pasar al siguiente paso
            EstadoActual = EstadoRecuperacion.CambiandoContrasena;
            MensajeEstado = string.Empty;
        }

        private async Task EjecutarRestablecerContrasena()
        {
            if (IsLoading) return;

            // Validaciones adicionales
            if (NuevaContrasena != ConfirmarContrasena)
            {
                ProcesoFallido?.Invoke(this, "Las contraseñas no coinciden");
                return;
            }

            if (NuevaContrasena.Length < 6)
            {
                ProcesoFallido?.Invoke(this, "La contraseña debe tener al menos 6 caracteres");
                return;
            }

            IsLoading = true;
            MensajeEstado = "Actualizando contraseña...";

            try
            {
                var request = new ReqRestablecerContrasena
                {
                    Token = this.Token.Trim(),
                    NuevaContrasena = this.NuevaContrasena,
                    ConfirmarContrasena = this.ConfirmarContrasena
                };

                // Llamada al backend
                var response = await _apiService.RestablecerContrasenaAsync(request);

                if (response != null && response.resultado)
                {
                    MensajeEstado = "¡Contraseña actualizada!";
                    await Task.Delay(500);

                    // Disparar evento de proceso completado
                    ProcesoCompletado?.Invoke(this, "Tu contraseña ha sido actualizada correctamente");
                }
                else
                {
                    // Restablecer contraseña fallido
                    string errorMsg = response?.errores?.FirstOrDefault()?.mensaje ??
                                     "Error al actualizar la contraseña. Verifique que el token sea válido.";

                    MensajeEstado = "Error al actualizar";
                    ProcesoFallido?.Invoke(this, errorMsg);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en restablecer contraseña: {ex.Message}");
                MensajeEstado = "Error de conexión";
                ProcesoFallido?.Invoke(this, "Error de conexión. Verifique su conexión a internet.");
            }
            finally
            {
                IsLoading = false;

                // Limpiar mensaje de error después de unos segundos
                if (MensajeEstado.Contains("Error"))
                {
                    await Task.Delay(3000);
                    MensajeEstado = string.Empty;
                }
            }
        }

        private async Task EjecutarReenviarToken()
        {
            if (IsLoading) return;

            IsLoading = true;
            MensajeEstado = "Reenviando token...";

            try
            {
                // Llamar al endpoint de solicitar reset password nuevamente
                var request = new ReqSolicitarResetPassword
                {
                    Email = this.EmailUsuario
                };

                var response = await _apiService.SolicitarResetPasswordAsync(request);

                if (response != null && response.resultado)
                {
                    MensajeEstado = "Token reenviado";
                    await Task.Delay(2000);
                    MensajeEstado = string.Empty;
                }
                else
                {
                    MensajeEstado = "Error al reenviar";
                    await Task.Delay(3000);
                    MensajeEstado = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al reenviar";
                await Task.Delay(3000);
                MensajeEstado = string.Empty;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EjecutarVolverAtras()
        {
            if (EstadoActual == EstadoRecuperacion.CambiandoContrasena)
            {
                // Volver al estado de ingreso de token
                EstadoActual = EstadoRecuperacion.IngresandoToken;
                MensajeEstado = string.Empty;
                LimpiarCamposContrasena();
            }
            else
            {
                // Cerrar modal
                await EjecutarCerrarModal();
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
            EstadoActual = EstadoRecuperacion.IngresandoToken;
            Token = string.Empty;
            NuevaContrasena = string.Empty;
            ConfirmarContrasena = string.Empty;
            MensajeEstado = string.Empty;
            IsLoading = false;
        }

        private void LimpiarCamposContrasena()
        {
            NuevaContrasena = string.Empty;
            ConfirmarContrasena = string.Empty;
        }

        // --- Validación en tiempo real para contraseñas --- //
        public string ValidarContrasenas()
        {
            if (EstadoActual != EstadoRecuperacion.CambiandoContrasena)
                return string.Empty;

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

        // Métodos parciales para notificar cambios en CanExecute y propiedades computadas
        partial void OnTokenChanged(string value)
        {
            ContinuarCommand?.NotifyCanExecuteChanged();
        }

        partial void OnNuevaContrasenaChanged(string value)
        {
            RestablecerContrasenaCommand?.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(MensajeValidacion));
        }

        partial void OnConfirmarContrasenaChanged(string value)
        {
            RestablecerContrasenaCommand?.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(MensajeValidacion));
        }

        partial void OnIsLoadingChanged(bool value)
        {
            ContinuarCommand?.NotifyCanExecuteChanged();
            RestablecerContrasenaCommand?.NotifyCanExecuteChanged();
        }

        partial void OnEstadoActualChanged(EstadoRecuperacion value)
        {
            // Notificar cambios en las propiedades computadas
            OnPropertyChanged(nameof(EstaIngresandoToken));
            OnPropertyChanged(nameof(EstaCambiandoContrasena));
            OnPropertyChanged(nameof(TituloModal));
            OnPropertyChanged(nameof(SubtituloModal));
            OnPropertyChanged(nameof(MensajeValidacion));

            // Notificar cambios en comandos
            ContinuarCommand?.NotifyCanExecuteChanged();
            RestablecerContrasenaCommand?.NotifyCanExecuteChanged();
        }

        // Propiedad computada para mostrar validación en tiempo real
        public string MensajeValidacion => ValidarContrasenas();
    }
}