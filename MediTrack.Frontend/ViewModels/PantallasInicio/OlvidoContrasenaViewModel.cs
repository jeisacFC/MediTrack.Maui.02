using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;

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

        // Constructor
        public OlvidoContrasenaViewModel()
        {
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
            MensajeEstado = "Enviando código...";

            try
            {
                // Simular envío de código (aquí irías al backend real)
                await Task.Delay(2000); // Simular llamada al backend

                MensajeEstado = "¡Código enviado!";

                // Disparar evento de código enviado exitosamente
                CodigoEnviado?.Invoke(this, Email.Trim());
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al enviar";
                EnvioFallido?.Invoke(this, "Error al enviar el código. Verifique su conexión a internet.");
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