using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MediTrack.Frontend.ViewModels
{
    public partial class ActualizarPerfilPopupViewModel : ObservableObject
    {
        private readonly IApiService _apiService;
        private readonly Usuarios _usuarioOriginal;
        public event EventHandler<bool> OnActualizacionCompletada;

        [ObservableProperty]
        private string nombre = string.Empty;

        [ObservableProperty]
        private string apellido1 = string.Empty;

        [ObservableProperty]
        private string apellido2 = string.Empty;

        [ObservableProperty]
        private DateTime fechaNacimiento = DateTime.Now.AddYears(-18);

        [ObservableProperty]
        private DateTime fechaMaxima = DateTime.Now.AddYears(-5);

        [ObservableProperty]
        private ObservableCollection<OpcionGenero> opcionesGenero;

        [ObservableProperty]
        private OpcionGenero generoSeleccionado;

        [ObservableProperty]
        private bool estaGuardando = false;

        [ObservableProperty]
        private string errorNombre = string.Empty;

        [ObservableProperty]
        private string errorApellido1 = string.Empty;

        [ObservableProperty]
        private string errorFechaNacimiento = string.Empty;

        public bool TieneErrorNombre => !string.IsNullOrEmpty(ErrorNombre);
        public bool TieneErrorApellido1 => !string.IsNullOrEmpty(ErrorApellido1);
        public bool TieneErrorFechaNacimiento => !string.IsNullOrEmpty(ErrorFechaNacimiento);

        public bool PuedeGuardar => !EstaGuardando &&
                                   !TieneErrorNombre &&
                                   !TieneErrorApellido1 &&
                                   !TieneErrorFechaNacimiento;

        public string TextoBotonGuardar => EstaGuardando ? "Guardando..." : "Guardar Cambios";
        public Usuarios GetUsuarioOriginal()
        {
            return _usuarioOriginal;
        }
        public ActualizarPerfilPopupViewModel(IApiService apiService, Usuarios usuario)
        {
            _apiService = apiService;
            _usuarioOriginal = usuario;

            InicializarOpcionesGenero();
            CargarDatosUsuario();
        }

        private void InicializarOpcionesGenero()
        {
            opcionesGenero = new ObservableCollection<OpcionGenero>
            {
                new OpcionGenero { Id = "1", Texto = "Masculino" },
                new OpcionGenero { Id = "2", Texto = "Femenino" },
                new OpcionGenero { Id = "3", Texto = "Otro" }
            };
        }

        private void CargarDatosUsuario()
        {
            if (_usuarioOriginal != null)
            {
                Nombre = _usuarioOriginal.nombre ?? string.Empty;
                Apellido1 = _usuarioOriginal.apellido1 ?? string.Empty;
                Apellido2 = _usuarioOriginal.apellido2 ?? string.Empty;

                if (_usuarioOriginal.fecha_nacimiento != DateTime.MinValue &&
                    _usuarioOriginal.fecha_nacimiento.Year > 1900)
                {
                    FechaNacimiento = _usuarioOriginal.fecha_nacimiento;
                }

                // Seleccionar género
                var genero = OpcionesGenero.FirstOrDefault(g => g.Id == _usuarioOriginal.id_genero);
                if (genero != null)
                {
                    GeneroSeleccionado = genero;
                }
            }
        }

        partial void OnNombreChanged(string value)
        {
            ValidarNombre();
            OnPropertyChanged(nameof(PuedeGuardar));
        }

        partial void OnApellido1Changed(string value)
        {
            ValidarApellido1();
            OnPropertyChanged(nameof(PuedeGuardar));
        }

        partial void OnFechaNacimientoChanged(DateTime value)
        {
            ValidarFechaNacimiento();
            OnPropertyChanged(nameof(PuedeGuardar));
        }

        partial void OnEstaGuardandoChanged(bool value)
        {
            OnPropertyChanged(nameof(PuedeGuardar));
            OnPropertyChanged(nameof(TextoBotonGuardar));
        }

        private void ValidarNombre()
        {
            ErrorNombre = string.Empty;

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                ErrorNombre = "El nombre es obligatorio";
            }
            else if (Nombre.Length < 2)
            {
                ErrorNombre = "El nombre debe tener al menos 2 caracteres";
            }
            else if (Nombre.Length > 50)
            {
                ErrorNombre = "El nombre no puede tener más de 50 caracteres";
            }

            OnPropertyChanged(nameof(TieneErrorNombre));
        }

        private void ValidarApellido1()
        {
            ErrorApellido1 = string.Empty;

            if (string.IsNullOrWhiteSpace(Apellido1))
            {
                ErrorApellido1 = "El primer apellido es obligatorio";
            }
            else if (Apellido1.Length < 2)
            {
                ErrorApellido1 = "El primer apellido debe tener al menos 2 caracteres";
            }
            else if (Apellido1.Length > 50)
            {
                ErrorApellido1 = "El primer apellido no puede tener más de 50 caracteres";
            }

            OnPropertyChanged(nameof(TieneErrorApellido1));
        }

        private void ValidarFechaNacimiento()
        {
            ErrorFechaNacimiento = string.Empty;

            var edad = DateTime.Now.Year - FechaNacimiento.Year;
            if (DateTime.Now.DayOfYear < FechaNacimiento.DayOfYear)
                edad--;

            if (edad < 5)
            {
                ErrorFechaNacimiento = "Debes tener al menos 5 años";
            }
            else if (edad > 120)
            {
                ErrorFechaNacimiento = "La edad no puede ser mayor a 120 años";
            }

            OnPropertyChanged(nameof(TieneErrorFechaNacimiento));
        }

        [RelayCommand]
        private async Task Guardar()
        {
            if (!PuedeGuardar) return;

            try
            {
                EstaGuardando = true;

                // Validar todos los campos
                ValidarNombre();
                ValidarApellido1();
                ValidarFechaNacimiento();

                if (TieneErrorNombre || TieneErrorApellido1 || TieneErrorFechaNacimiento)
                {
                    return;
                }

                Debug.WriteLine("=== INICIANDO ACTUALIZACIÓN DEL PERFIL ===");

                var request = new ReqActualizarUsuario
                {
                    IdUsuario = _usuarioOriginal.id_usuario,
                    Nombre = Nombre.Trim(),
                    Apellido1 = Apellido1.Trim(),
                    Apellido2 = string.IsNullOrWhiteSpace(Apellido2) ? null : Apellido2.Trim(),
                    FechaNacimiento = FechaNacimiento,
                    IdGenero = GeneroSeleccionado?.Id
                };

                Debug.WriteLine($"Request - Usuario ID: {request.IdUsuario}");
                Debug.WriteLine($"Request - Nombre: {request.Nombre}");
                Debug.WriteLine($"Request - Apellido1: {request.Apellido1}");
                Debug.WriteLine($"Request - Apellido2: {request.Apellido2}");
                Debug.WriteLine($"Request - Género: {request.IdGenero}");

                var response = await _apiService.ActualizarUsuarioAsync(request);

                if (response != null && response.resultado)
                {
                    Debug.WriteLine("=== ACTUALIZACIÓN EXITOSA ===");

                    // Actualizar los datos del usuario original
                    _usuarioOriginal.nombre = request.Nombre;
                    _usuarioOriginal.apellido1 = request.Apellido1;
                    _usuarioOriginal.apellido2 = request.Apellido2;
                    _usuarioOriginal.fecha_nacimiento = request.FechaNacimiento.Value;
                    _usuarioOriginal.id_genero = request.IdGenero;

                    // Notificar que la actualización fue exitosa
                    OnActualizacionCompletada?.Invoke(this, true);
                }
                else
                {
                    Debug.WriteLine("=== ERROR EN ACTUALIZACIÓN ===");
                    var mensajeError = response?.Mensaje ?? "Error desconocido al actualizar el perfil";

                    if (response?.errores != null && response.errores.Any())
                    {
                        var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                        mensajeError += $". Detalles: {erroresDetalle}";
                    }

                    Debug.WriteLine($"Error: {mensajeError}");
                    await Application.Current.MainPage.DisplayAlert("Error", mensajeError, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPCIÓN al actualizar perfil: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Error inesperado al actualizar el perfil. Por favor, intenta nuevamente.", "OK");
            }
            finally
            {
                EstaGuardando = false;
            }
        }
    }

    public class OpcionGenero
    {
        public string Id { get; set; }
        public string Texto { get; set; }
    }
}