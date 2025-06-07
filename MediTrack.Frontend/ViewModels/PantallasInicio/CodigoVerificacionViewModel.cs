using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;

namespace MediTrack.Frontend.ViewModels.PantallasInicio
{
    public partial class CodigoVerificacionViewModel : ObservableObject
    {
        // --- Propiedades para la UI --- //
        [ObservableProperty] private string _codigo = string.Empty;
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private string _mensajeEstado = string.Empty;
        [ObservableProperty] private string _emailUsuario = string.Empty;

        // --- Comandos --- //
        public IAsyncRelayCommand VerificarCodigoCommand { get; }
        public IAsyncRelayCommand ReenviarCodigoCommand { get; }
        public IAsyncRelayCommand CerrarModalCommand { get; }

        // --- Eventos para comunicar a la Vista --- //
        public event EventHandler<string> CodigoVerificado; // Pasa el email
        public event EventHandler ModalCerrado;
        public event EventHandler<string> VerificacionFallida;

        // Constructor
        public CodigoVerificacionViewModel()
        {
            // Inicializar comandos
            VerificarCodigoCommand = new AsyncRelayCommand(EjecutarVerificarCodigo, PuedeVerificarCodigo);
            ReenviarCodigoCommand = new AsyncRelayCommand(EjecutarReenviarCodigo);
            CerrarModalCommand = new AsyncRelayCommand(EjecutarCerrarModal);
        }

        // --- Métodos de los comandos --- //
        private bool PuedeVerificarCodigo()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Codigo) && Codigo.Length == 6;
        }

        private async Task EjecutarVerificarCodigo()
        {
            System.Diagnostics.Debug.WriteLine("=== INICIO EjecutarVerificarCodigo ===");
            System.Diagnostics.Debug.WriteLine($"Código ingresado: '{Codigo}', Length: {Codigo?.Length}");
            System.Diagnostics.Debug.WriteLine($"Email usuario: '{EmailUsuario}'");

            if (IsLoading)
            {
                System.Diagnostics.Debug.WriteLine("Ya está cargando, saliendo del método");
                return;
            }

            IsLoading = true;
            MensajeEstado = "Verificando código...";
            System.Diagnostics.Debug.WriteLine("Estado cambiado a: Verificando código...");

            try
            {
                // Simular verificación (por ahora acepta cualquier código de 6 dígitos)
                System.Diagnostics.Debug.WriteLine("Iniciando simulación de verificación...");
                await Task.Delay(1500); // Simular llamada al backend

                if (Codigo.Length == 6)
                {
                    System.Diagnostics.Debug.WriteLine("Código válido (6 dígitos), procesando...");
                    MensajeEstado = "¡Código verificado!";
                    await Task.Delay(500);

                    System.Diagnostics.Debug.WriteLine($"Disparando evento CodigoVerificado con email: '{EmailUsuario}'");
                    System.Diagnostics.Debug.WriteLine($"Evento CodigoVerificado tiene suscriptores: {CodigoVerificado != null}");

                    // Disparar evento de código verificado
                    CodigoVerificado?.Invoke(this, EmailUsuario);

                    System.Diagnostics.Debug.WriteLine("Evento CodigoVerificado disparado exitosamente");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Código inválido, longitud: {Codigo.Length}");
                    MensajeEstado = "Código inválido";
                    VerificacionFallida?.Invoke(this, "El código debe tener 6 dígitos");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR en verificación: {ex.Message}");
                MensajeEstado = "Error de verificación";
                VerificacionFallida?.Invoke(this, "Error al verificar el código. Intente nuevamente.");
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("IsLoading = false");

                // Limpiar mensaje después de unos segundos si hay error
                if (MensajeEstado.Contains("Error") || MensajeEstado.Contains("inválido"))
                {
                    System.Diagnostics.Debug.WriteLine("Programando limpieza de mensaje de error...");
                    await Task.Delay(3000);
                    MensajeEstado = string.Empty;
                    System.Diagnostics.Debug.WriteLine("Mensaje de error limpiado");
                }

                System.Diagnostics.Debug.WriteLine("=== FIN EjecutarVerificarCodigo ===");
            }
        }

        private async Task EjecutarReenviarCodigo()
        {
            if (IsLoading) return;

            IsLoading = true;
            MensajeEstado = "Reenviando código...";

            try
            {
                // Simular reenvío
                await Task.Delay(1000);
                MensajeEstado = "Código reenviado";

                await Task.Delay(2000);
                MensajeEstado = string.Empty;
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
            Codigo = string.Empty;
            MensajeEstado = string.Empty;
            IsLoading = false;
        }

        // Métodos parciales para notificar cambios en CanExecute
        partial void OnCodigoChanged(string value)
        {
            VerificarCodigoCommand?.NotifyCanExecuteChanged();
        }

        partial void OnIsLoadingChanged(bool value)
        {
            VerificarCodigoCommand?.NotifyCanExecuteChanged();
        }
    }
}