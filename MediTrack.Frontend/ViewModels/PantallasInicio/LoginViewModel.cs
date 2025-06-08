using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Services.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace MediTrack.Frontend.ViewModels.PantallasInicio
{
    public partial class LoginViewModel : ObservableObject
    {
        // --- Propiedades para la UI --- //
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _contraseña = string.Empty;
        [ObservableProperty] private bool _recordarSesion = false;
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private string _mensajeEstado = string.Empty;

        // --- Comandos --- //
        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand IrARegistroCommand { get; }
        public IAsyncRelayCommand IrAOlvidoContraseñaCommand { get; }

        // --- Eventos para comunicar a la Vista --- //
        public event EventHandler<ResLogin> LoginExitoso;
        public event EventHandler<string> LoginFallido;

        // --- Dependencias --- //
        private readonly IApiService _apiService;

        // Constructor
        public LoginViewModel(IApiService apiService)
        {
            _apiService = apiService;

            // Inicializar comandos
            LoginCommand = new AsyncRelayCommand(EjecutarLogin, PuedeEjecutarLogin);
            IrARegistroCommand = new AsyncRelayCommand(EjecutarIrARegistro);
            IrAOlvidoContraseñaCommand = new AsyncRelayCommand(EjecutarIrAOlvidoContraseña);
        }

        // --- Métodos de los comandos --- //
        private bool PuedeEjecutarLogin()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(this.Email) &&
                   !string.IsNullOrWhiteSpace(this.Contraseña);
        }

        private async Task EjecutarLogin()
        {
            if (IsLoading) return;

            IsLoading = true;
            MensajeEstado = "Iniciando sesión...";

            try
            {
                // LOG: Verificar datos antes de crear request
                Debug.WriteLine($"=== DATOS ANTES DE REQUEST ===");
                Debug.WriteLine($"Email from UI: '{this.Email}'");
                Debug.WriteLine($"Contraseña from UI: '{this.Contraseña}'");
                Debug.WriteLine($"Email length: {this.Email?.Length}");
                Debug.WriteLine($"Contraseña length: {this.Contraseña?.Length}");

                var request = new ReqLogin
                {
                    email = this.Email?.Trim() ?? string.Empty,
                    contrasena = this.Contraseña ?? string.Empty                };

                Debug.WriteLine($"=== REQUEST CREADO ===");
                Debug.WriteLine($"Request Email: '{request.email}'");
                Debug.WriteLine($"Request Contraseña: '{request.contrasena}'");

                // Llamada al backend
                var response = await _apiService.LoginAsync(request);

                if (response != null && response.resultado)
                {
                    MensajeEstado = "¡Bienvenido!";

                    // Disparar evento de login exitoso
                    LoginExitoso?.Invoke(this, response);
                }
                else
                {
                    // Login fallido
                    string errorMsg = response?.errores?.FirstOrDefault()?.mensaje ??
                                     "Credenciales incorrectas. Verifique su email y contraseña.";

                    MensajeEstado = "Error en el login";
                    LoginFallido?.Invoke(this, errorMsg);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en login: {ex.Message}");
                MensajeEstado = "Error de conexión";
                LoginFallido?.Invoke(this, "Error de conexión. Verifique su conexión a internet.");
            }
            finally
            {
                IsLoading = false;

                // Limpiar mensaje después de unos segundos
                await Task.Delay(3000);
                MensajeEstado = string.Empty;
            }
        }

        private async Task EjecutarIrARegistro()
        {
            await Shell.Current.GoToAsync("///registro");
        }

        private async Task EjecutarIrAOlvidoContraseña()
        {
            await Shell.Current.GoToAsync("///olvidoContrasena");
        }

        public void LimpiarFormulario()
        {
            Email = string.Empty;
            Contraseña = string.Empty;
            RecordarSesion = false;
            MensajeEstado = string.Empty;
        }

        // Métodos parciales para notificar cambios en CanExecute
        partial void OnEmailChanged(string value)
        {
            LoginCommand?.NotifyCanExecuteChanged();
        }

        partial void OnContraseñaChanged(string value)
        {
            LoginCommand?.NotifyCanExecuteChanged();
        }

        partial void OnIsLoadingChanged(bool value)
        {
            LoginCommand?.NotifyCanExecuteChanged();
        }
    }
}