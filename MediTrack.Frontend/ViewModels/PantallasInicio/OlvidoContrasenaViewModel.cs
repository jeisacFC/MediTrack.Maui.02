using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Interfaces;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace MediTrack.Frontend.ViewModels.PantallasInicio
{
    public partial class OlvidoContrasenaViewModel : ObservableObject
    {
        // --- Propiedades para la UI --- //
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private string _mensajeEstado = string.Empty;

        // --- Comandos --- //
        public IAsyncRelayCommand EnviarCodigoCommand { get; }
        public IAsyncRelayCommand VolverInicioSesionCommand { get; }

        // --- Eventos para comunicar a la Vista --- //
        public event EventHandler<string> CodigoEnviado; // Pasa el email
        public event EventHandler<string> EnvioFallido;

        // --- Dependencias --- //
        private readonly IApiService _apiService;

        // Constructor
        public OlvidoContrasenaViewModel(IApiService apiService)
        {
            _apiService = apiService;

            // Inicializar comandos
            EnviarCodigoCommand = new AsyncRelayCommand(EjecutarEnviarCodigo, PuedeEnviarCodigo);
            VolverInicioSesionCommand = new AsyncRelayCommand(EjecutarVolverInicioSesion);
        }

        // --- Métodos de los comandos --- //
        private bool PuedeEnviarCodigo()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   Email.Contains("@") &&
                   Email.Contains(".");
        }

        private async Task EjecutarEnviarCodigo()
        {
            if (IsLoading) return;

            IsLoading = true;
            MensajeEstado = "Enviando token...";

            try
            {
                var request = new ReqSolicitarResetPassword
                {
                    Email = this.Email.Trim()
                };

                System.Diagnostics.Debug.WriteLine($"=== ENVIANDO REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"Email: {request.Email}");

                // Llamada al backend
                var response = await _apiService.SolicitarResetPasswordAsync(request);

                System.Diagnostics.Debug.WriteLine($"=== RESPUESTA RECIBIDA ===");
                System.Diagnostics.Debug.WriteLine($"Response es null: {response == null}");
                if (response != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Resultado: {response.resultado}");
                    System.Diagnostics.Debug.WriteLine($"EmailEnviado: {response.EmailEnviado}");
                    System.Diagnostics.Debug.WriteLine($"Mensaje: {response.Mensaje}");
                    System.Diagnostics.Debug.WriteLine($"Errores count: {response.errores?.Count ?? 0}");

                    if (response.errores != null)
                    {
                        foreach (var error in response.errores)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error: {error.mensaje}");
                        }
                    }
                }

                // TEMPORAL: Aceptar si el token llegó al correo, independientemente de la respuesta
                if (response != null && (response.resultado || response.EmailEnviado))
                {
                    MensajeEstado = "¡Token enviado!";
                    CodigoEnviado?.Invoke(this, Email.Trim());
                }
                else
                {
                    // Envío fallido
                    string errorMsg = response?.errores?.FirstOrDefault()?.mensaje ??
                                     response?.Mensaje ??
                                     "Error al enviar el token. Verifique que el email sea correcto.";

                    MensajeEstado = "Error al enviar";
                    EnvioFallido?.Invoke(this, errorMsg);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                MensajeEstado = "Error de conexión";
                EnvioFallido?.Invoke(this, "Error de conexión. Verifique su conexión a internet.");
            }
            finally
            {
                IsLoading = false;

                // Solo limpiar mensaje si hubo error
                if (MensajeEstado.Contains("Error"))
                {
                    await Task.Delay(3000);
                    MensajeEstado = string.Empty;
                }
            }
        }

        private async Task EjecutarVolverInicioSesion()
        {
            await Shell.Current.GoToAsync("//inicioSesion");
        }

        // --- Métodos públicos --- //
        public void LimpiarFormulario()
        {
            Email = string.Empty;
            MensajeEstado = string.Empty;
            IsLoading = false;
        }

        // Métodos parciales para notificar cambios en CanExecute
        partial void OnEmailChanged(string value)
        {
            EnviarCodigoCommand?.NotifyCanExecuteChanged();
        }

        partial void OnIsLoadingChanged(bool value)
        {
            EnviarCodigoCommand?.NotifyCanExecuteChanged();
        }
    }
}