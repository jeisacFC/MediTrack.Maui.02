using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Services.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;

namespace MediTrack.Frontend.ViewModels.PantallasInicio
{
    public partial class RegisterViewModel : ObservableObject
    {
        // --- Propiedades para la UI --- //
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _contraseña = string.Empty;
        [ObservableProperty] private string _confirmarContraseña = string.Empty;
        [ObservableProperty] private string _nombre = string.Empty;
        [ObservableProperty] private string _apellido1 = string.Empty;
        [ObservableProperty] private string _apellido2 = string.Empty;
        [ObservableProperty] private DateTime _fechaNacimiento = DateTime.Now.AddYears(-18);
        [ObservableProperty] private string _idGenero = string.Empty;
        [ObservableProperty] private bool _notificacionesPush = true;
        [ObservableProperty] private bool _isLoading = false;
        [ObservableProperty] private string _mensajeEstado = string.Empty;
        [ObservableProperty] private bool _aceptaTerminos = false;

        // --- Colecciones para Pickers --- //
        public ObservableCollection<GeneroOption> GenerosDisponibles { get; } = new()
        {
            new GeneroOption { Id = "1", Nombre = "Masculino" },      // Cambiar "M" → "1"
            new GeneroOption { Id = "2", Nombre = "Femenino" },       // Cambiar "F" → "2"  
            new GeneroOption { Id = "3", Nombre = "Otro" },           // Cambiar "O" → "3"
            new GeneroOption { Id = "0", Nombre = "Prefiero no decir" } // Cambiar "N" → "0"
        };

        [ObservableProperty] private GeneroOption _generoSeleccionado;

        // --- Comandos --- //
        public IAsyncRelayCommand RegisterCommand { get; }
        public IAsyncRelayCommand IrALoginCommand { get; }

        // --- Eventos para comunicar a la Vista --- //
        public event EventHandler<ResRegister> RegistroExitoso;
        public event EventHandler<string> RegistroFallido;

        // --- Dependencias --- //
        private readonly IApiService _apiService;

        // Constructor
        public RegisterViewModel(IApiService apiService)
        {
            _apiService = apiService;

            // Inicializar comandos
            RegisterCommand = new AsyncRelayCommand(EjecutarRegistro, PuedeEjecutarRegistro);
            IrALoginCommand = new AsyncRelayCommand(EjecutarIrALogin);

            // Inicializar género por defecto
            GeneroSeleccionado = GenerosDisponibles.FirstOrDefault();
        }

        // --- Métodos de los comandos --- //
        private bool PuedeEjecutarRegistro()
        {
            return !_isLoading &&
                   !string.IsNullOrWhiteSpace(_email) &&
                   !string.IsNullOrWhiteSpace(_contraseña) &&
                   !string.IsNullOrWhiteSpace(_confirmarContraseña) &&
                   !string.IsNullOrWhiteSpace(_nombre) &&
                   !string.IsNullOrWhiteSpace(_apellido1) &&
                   _generoSeleccionado != null &&
                   _aceptaTerminos &&
                   _contraseña == _confirmarContraseña &&
                   ValidarEmail(_email) &&
                   ValidarContraseña(_contraseña) &&
                   ValidarEdad(_fechaNacimiento);
        }

        private async Task EjecutarRegistro()
        {
            if (_isLoading) return;

            IsLoading = true;
            MensajeEstado = "Registrando usuario...";

            try
            {
                // LOG: Verificar datos antes de crear request
                Debug.WriteLine($"=== DATOS ANTES DE REQUEST ===");
                Debug.WriteLine($"Email from UI: '{_email}'");
                Debug.WriteLine($"Nombre from UI: '{_nombre}'");
                Debug.WriteLine($"Apellido1 from UI: '{_apellido1}'");
                Debug.WriteLine($"Apellido2 from UI: '{_apellido2}'");
                Debug.WriteLine($"FechaNacimiento from UI: '{_fechaNacimiento}'");
                Debug.WriteLine($"Género seleccionado: '{_generoSeleccionado?.Id}'");
                Debug.WriteLine($"NotificacionesPush: '{_notificacionesPush}'");

                var request = new ReqRegister
                {
                    Email = _email?.Trim() ?? string.Empty,
                    Contrasena = _contraseña ?? string.Empty,
                    Nombre = _nombre?.Trim() ?? string.Empty,
                    Apellido1 = _apellido1?.Trim() ?? string.Empty,
                    Apellido2 = _apellido2?.Trim() ?? string.Empty,
                    FechaNacimiento = _fechaNacimiento,
                    IdGenero = _generoSeleccionado?.Id ?? string.Empty,
                    NotificacionesPush = _notificacionesPush
                };

                Debug.WriteLine($"=== REQUEST CREADO ===");
                Debug.WriteLine($"Request Email: '{request.Email}'");
                Debug.WriteLine($"Request Nombre: '{request.Nombre}'");
                Debug.WriteLine($"Request Apellido1: '{request.Apellido1}'");
                Debug.WriteLine($"Request FechaNacimiento: '{request.FechaNacimiento}'");
                Debug.WriteLine($"Request IdGenero: '{request.IdGenero}'");

                // Llamada al backend
                var response = await _apiService.RegisterAsync(request);

                if (response != null && response.resultado)
                {
                    MensajeEstado = "¡Registro exitoso!";

                    // Disparar evento de registro exitoso
                    RegistroExitoso?.Invoke(this, response);
                }
                else
                {
                    // Registro fallido
                    string errorMsg = response?.errores?.FirstOrDefault()?.Message ??
                                     response?.Mensaje ??
                                     "Error en el registro. Verifique los datos ingresados.";

                    MensajeEstado = "Error en el registro";
                    RegistroFallido?.Invoke(this, errorMsg);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en registro: {ex.Message}");
                MensajeEstado = "Error de conexión";
                RegistroFallido?.Invoke(this, "Error de conexión. Verifique su conexión a internet.");
            }
            finally
            {
                IsLoading = false;

                // Limpiar mensaje después de unos segundos
                await Task.Delay(3000);
                MensajeEstado = string.Empty;
            }
        }

        private async Task EjecutarIrALogin()
        {
            await Shell.Current.GoToAsync("//inicioSesion");
        }

        // --- Métodos de validación --- //
        private bool ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidarContraseña(string contraseña)
        {
            if (string.IsNullOrWhiteSpace(contraseña))
                return false;

            // Validar longitud mínima
            if (contraseña.Length < 8)
                return false;

            // Validar que tenga al menos una mayúscula, una minúscula y un número
            bool tieneMinuscula = contraseña.Any(char.IsLower);
            bool tieneMayuscula = contraseña.Any(char.IsUpper);
            bool tieneNumero = contraseña.Any(char.IsDigit);

            return tieneMinuscula && tieneMayuscula && tieneNumero;
        }

        private bool ValidarEdad(DateTime fechaNacimiento)
        {
            var edad = DateTime.Now.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > DateTime.Now.AddYears(-edad))
                edad--;

            return edad >= 13; // Edad mínima
        }

        public string ObtenerMensajeValidacionContraseña()
        {
            if (string.IsNullOrWhiteSpace(_contraseña))
                return "La contraseña es requerida";

            if (_contraseña.Length < 8)
                return "La contraseña debe tener al menos 8 caracteres";

            if (!_contraseña.Any(char.IsLower))
                return "La contraseña debe tener al menos una letra minúscula";

            if (!_contraseña.Any(char.IsUpper))
                return "La contraseña debe tener al menos una letra mayúscula";

            if (!_contraseña.Any(char.IsDigit))
                return "La contraseña debe tener al menos un número";

            return string.Empty;
        }

        public void LimpiarFormulario()
        {
            Email = string.Empty;
            Contraseña = string.Empty;
            ConfirmarContraseña = string.Empty;
            Nombre = string.Empty;
            Apellido1 = string.Empty;
            Apellido2 = string.Empty;
            FechaNacimiento = DateTime.Now.AddYears(-18);
            GeneroSeleccionado = GenerosDisponibles.FirstOrDefault();
            NotificacionesPush = true;
            AceptaTerminos = false;
            MensajeEstado = string.Empty;
        }

        // Métodos parciales para notificar cambios en CanExecute
        partial void OnEmailChanged(string value)
        {
            RegisterCommand?.NotifyCanExecuteChanged();
        }

        partial void OnContraseñaChanged(string value)
        {
            RegisterCommand?.NotifyCanExecuteChanged();
        }

        partial void OnConfirmarContraseñaChanged(string value)
        {
            RegisterCommand?.NotifyCanExecuteChanged();
        }

        partial void OnNombreChanged(string value)
        {
            RegisterCommand?.NotifyCanExecuteChanged();
        }

        partial void OnApellido1Changed(string value)
        {
            RegisterCommand?.NotifyCanExecuteChanged();
        }

        partial void OnGeneroSeleccionadoChanged(GeneroOption value)
        {
            if (value != null)
                _idGenero = value.Id;
            RegisterCommand?.NotifyCanExecuteChanged();
        }

        partial void OnAceptaTerminosChanged(bool value)
        {
            RegisterCommand?.NotifyCanExecuteChanged();
        }

        partial void OnFechaNacimientoChanged(DateTime value)
        {
            RegisterCommand?.NotifyCanExecuteChanged();
        }

        partial void OnIsLoadingChanged(bool value)
        {
            RegisterCommand?.NotifyCanExecuteChanged();
        }
    }

    // --- Clase auxiliar para el picker de género --- //
    public class GeneroOption
    {
        public string Id { get; set; }
        public string Nombre { get; set; }

        public override string ToString()
        {
            return Nombre;
        }
    }
}