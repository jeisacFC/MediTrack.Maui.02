using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MediTrack.Frontend.ViewModels
{
    public partial class PerfilViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private bool _isLoggingOut = false;

        [ObservableProperty]
        private Usuarios usuario;

        [ObservableProperty]
        private ObservableCollection<CondicionesMedicas> condicionesMedicas;

        [ObservableProperty]
        private ObservableCollection<Alergias> alergias;

        [ObservableProperty]
        private ObservableCollection<CondicionesMedicas> condicionesMedicasSeleccionadas;

        [ObservableProperty]
        private ObservableCollection<Alergias> alergiasSeleccionadas;

        [ObservableProperty]
        private string nombreCompleto = string.Empty;

        // Propiedades adicionales para mostrar información formateada
        [ObservableProperty]
        private string fechaNacimientoFormateada = string.Empty;

        [ObservableProperty]
        private string fechaRegistroFormateada = string.Empty;

        [ObservableProperty]
        private string ultimoAccesoFormateado = string.Empty;

        [ObservableProperty]
        private string generoTexto = string.Empty;

        [ObservableProperty]
        private string estadoCuenta = string.Empty;

        public PerfilViewModel(IApiService apiService)
        {
            _apiService = apiService;
            Title = "Perfil";
            _isLoggingOut = false;

            // Inicializar colecciones
            condicionesMedicas = new ObservableCollection<CondicionesMedicas>();
            alergias = new ObservableCollection<Alergias>();
            condicionesMedicasSeleccionadas = new ObservableCollection<CondicionesMedicas>();
            alergiasSeleccionadas = new ObservableCollection<Alergias>();

            // Inicializar usuario vacío
            usuario = new Usuarios();
            InicializarPropiedadesFormateadas();
        }

        private void InicializarPropiedadesFormateadas()
        {
            NombreCompleto = string.Empty;
            FechaNacimientoFormateada = string.Empty;
            FechaRegistroFormateada = string.Empty;
            UltimoAccesoFormateado = string.Empty;
            GeneroTexto = string.Empty;
            EstadoCuenta = string.Empty;
        }

        public override async Task InitializeAsync()
        {
            Debug.WriteLine("=== INICIANDO INITIALIZE ASYNC DEL PERFIL ===");

            await ExecuteAsync(async () =>
            {
                Debug.WriteLine("Cargando datos del usuario...");
                await CargarDatosUsuarioAsync();

                Debug.WriteLine("Cargando condiciones médicas...");
                await CargarCondicionesMedicasAsync();

                Debug.WriteLine("Cargando alergias...");
                await CargarAlergiasAsync();

                Debug.WriteLine("=== INITIALIZE ASYNC COMPLETADO ===");
            });
        }

        private async Task CargarDatosUsuarioAsync()
        {
            try
            {
                Debug.WriteLine("=== INICIANDO CARGA DE DATOS USUARIO ===");

                var userIdStr = await SecureStorage.GetAsync("user_id");
                Debug.WriteLine($"User ID obtenido del storage: {userIdStr}");

                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    Debug.WriteLine("ERROR: No se pudo obtener user_id del storage");
                    ErrorMessage = "No se pudo obtener la información del usuario";
                    await ShowAlertAsync("Error", "Sesión expirada. Por favor, inicia sesión nuevamente.");
                    await Shell.Current.GoToAsync("//Login");
                    return;
                }

                Debug.WriteLine($"Creando request con userId: {userId}");

                var request = new ReqObtenerUsuario
                {
                    IdUsuario = userId
                };

                Debug.WriteLine("Llamando al servicio API...");
                var response = await _apiService.GetUserAsync(request);

                Debug.WriteLine($"Respuesta recibida - resultado: {response?.resultado}");
                Debug.WriteLine($"Usuario en respuesta: {response?.Usuario != null}");

                // CORRECCIÓN: Verificar que la respuesta sea válida
                if (response != null && response.resultado)
                {
                    Debug.WriteLine("=== RESPUESTA VÁLIDA DEL SERVIDOR ===");

                    if (response.Usuario != null)
                    {
                        Debug.WriteLine("=== ACTUALIZANDO DATOS DEL USUARIO ===");

                        // Actualizar el usuario
                        Usuario = response.Usuario;
                        Debug.WriteLine($"Usuario asignado: {Usuario.nombre} {Usuario.apellido1}");

                        // Actualizar propiedades formateadas
                        ActualizarPropiedadesFormateadas();

                        Debug.WriteLine($"NombreCompleto establecido: '{NombreCompleto}'");
                        Debug.WriteLine($"Email: {Usuario.email}");
                        Debug.WriteLine($"Fecha nacimiento: {Usuario.fecha_nacimiento}");
                        Debug.WriteLine($"Género: {Usuario.id_genero}");
                        Debug.WriteLine($"Notificaciones: {Usuario.notificaciones_push}");

                        // Forzar actualización de la UI
                        OnPropertyChanged(nameof(Usuario));
                        OnPropertyChanged(nameof(NombreCompleto));

                        Debug.WriteLine("=== DATOS USUARIO CARGADOS EXITOSAMENTE ===");
                    }
                    else
                    {
                        Debug.WriteLine("ADVERTENCIA: Usuario es null en la respuesta válida");
                        ErrorMessage = "No se recibieron datos del usuario";
                        await ShowAlertAsync("Error", "No se pudieron cargar los datos del usuario");
                    }
                }
                else
                {
                    Debug.WriteLine("ERROR: Respuesta inválida del servidor");
                    var mensajeError = response?.Mensaje ?? "Error desconocido al obtener datos del usuario";

                    if (response?.errores != null && response.errores.Any())
                    {
                        var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                        mensajeError += $". Detalles: {erroresDetalle}";
                    }

                    ErrorMessage = mensajeError;
                    await ShowAlertAsync("Error", mensajeError);
                    Debug.WriteLine($"Mensaje de error: {mensajeError}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPCIÓN en CargarDatosUsuarioAsync: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ErrorMessage = "Error al cargar datos del usuario";
                await HandleErrorAsync(ex);
            }
        }

        private void ActualizarPropiedadesFormateadas()
        {
            if (Usuario == null) return;

            // Nombre completo
            NombreCompleto = $"{Usuario.nombre} {Usuario.apellido1} {Usuario.apellido2}".Trim();

            // Fecha de nacimiento
            if (Usuario.fecha_nacimiento != DateTime.MinValue)
            {
                FechaNacimientoFormateada = Usuario.fecha_nacimiento.ToString("dd/MM/yyyy");

                // Calcular edad
                var edad = DateTime.Now.Year - Usuario.fecha_nacimiento.Year;
                if (DateTime.Now.DayOfYear < Usuario.fecha_nacimiento.DayOfYear)
                    edad--;

                FechaNacimientoFormateada += $" ({edad} años)";
            }
            else
            {
                FechaNacimientoFormateada = "No especificada";
            }

            // Fecha de registro
            if (Usuario.fecha_registro != DateTime.MinValue)
            {
                FechaRegistroFormateada = Usuario.fecha_registro.ToString("dd/MM/yyyy");
            }
            else
            {
                FechaRegistroFormateada = "No disponible";
            }

            // Último acceso
            if (Usuario.ultimo_acceso != DateTime.MinValue)
            {
                UltimoAccesoFormateado = Usuario.ultimo_acceso.ToString("dd/MM/yyyy HH:mm");
            }
            else
            {
                UltimoAccesoFormateado = "No disponible";
            }

            // Género
            GeneroTexto = Usuario.id_genero switch
            {
                "1" => "Masculino",
                "2" => "Femenino",
                "3" => "Otro",
                _ => "No especificado"
            };

            // Estado de cuenta
            if (Usuario.cuenta_bloqueada)
            {
                EstadoCuenta = "Bloqueada";
            }
            else
            {
                EstadoCuenta = Usuario.intentos_fallidos > 0
                    ? $"Activa ({Usuario.intentos_fallidos} intentos fallidos)"
                    : "Activa";
            }

            Debug.WriteLine($"Propiedades formateadas actualizadas:");
            Debug.WriteLine($"- NombreCompleto: {NombreCompleto}");
            Debug.WriteLine($"- FechaNacimiento: {FechaNacimientoFormateada}");
            Debug.WriteLine($"- Género: {GeneroTexto}");
        }

        private async Task CargarCondicionesMedicasAsync()
        {
            try
            {
                // TODO: Implementar cuando tengas el método en IApiService
                // var condiciones = await _apiService.ObtenerCondicionesMedicasUsuarioAsync(Usuario.id_usuario);

                // Datos de ejemplo para pruebas
                var condicionesEjemplo = new List<CondicionesMedicas>
                {
                    new CondicionesMedicas
                    {
                        id_condicion = 1,
                        nombre_condicion = "Hipertensión",
                        descripcion = "Presión arterial alta",
                        FechaDiagnostico = DateTime.Now.AddYears(-2)
                    },
                    new CondicionesMedicas
                    {
                        id_condicion = 2,
                        nombre_condicion = "Diabetes Tipo 2",
                        descripcion = "Diabetes mellitus tipo 2",
                        FechaDiagnostico = DateTime.Now.AddYears(-1)
                    }
                };

                condicionesMedicas.Clear();
                foreach (var condicion in condicionesEjemplo)
                {
                    condicionesMedicas.Add(condicion);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al cargar condiciones médicas";
                await HandleErrorAsync(ex);
            }
        }

        private async Task CargarAlergiasAsync()
        {
            try
            {
                // TODO: Implementar cuando tengas el método en IApiService
                // var alergias = await _apiService.ObtenerAlergiasUsuarioAsync(Usuario.id_usuario);

                // Datos de ejemplo para pruebas
                var alergiasEjemplo = new List<Alergias>
                {
                    new Alergias
                    {
                        id_alergia = 1,
                        nombre_alergia = "Penicilina",
                        descripcion = "Alergia a antibióticos",
                        FechaDiagnostico = DateTime.Now.AddYears(-3)
                    },
                    new Alergias
                    {
                        id_alergia = 2,
                        nombre_alergia = "Mariscos",
                        descripcion = "Alergia alimentaria",
                        FechaDiagnostico = DateTime.Now.AddYears(-5)
                    }
                };

                alergias.Clear();
                foreach (var alergia in alergiasEjemplo)
                {
                    alergias.Add(alergia);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al cargar alergias";
                await HandleErrorAsync(ex);
            }
        }

        // Métodos para manejar selección múltiple
        public void OnCondicionesMedicasSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            condicionesMedicasSeleccionadas.Clear();
            foreach (CondicionesMedicas condicion in e.CurrentSelection)
            {
                condicionesMedicasSeleccionadas.Add(condicion);
            }
        }

        public void OnAlergiasSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            alergiasSeleccionadas.Clear();
            foreach (Alergias alergia in e.CurrentSelection)
            {
                alergiasSeleccionadas.Add(alergia);
            }
        }

        [RelayCommand]
        private async Task EditarPerfil()
        {
            await ExecuteAsync(async () =>
            {
                await Shell.Current.GoToAsync("//EditarPerfil");
            });
        }

        [RelayCommand]
        private async Task CerrarSesion()
        {
            if (_isLoggingOut) return;

            var confirmar = await ShowConfirmAsync(
                "Cerrar Sesión",
                "¿Estás seguro que deseas cerrar sesión?",
                "Cerrar Sesión",
                "Cancelar");

            if (confirmar)
            {
                _isLoggingOut = true;

                try
                {
                    var logoutRequest = new ReqLogout
                    {
                        InvalidarTodos = false
                    };

                    var response = await _apiService.LogoutAsync(logoutRequest);

                    if (response != null && response.resultado && response.LogoutExitoso)
                    {
                        Debug.WriteLine($"Logout exitoso. Tokens invalidados: {response.TokensInvalidados}");
                    }
                    else
                    {
                        var mensajeError = response?.Mensaje ?? "Error al cerrar sesión en el servidor";
                        Debug.WriteLine($"Error en logout del servidor: {mensajeError}");
                    }

                    await LimpiarSesionAsync();
                    await ShowAlertAsync("Éxito", "Sesión cerrada correctamente");

                    try
                    {
                        await Shell.Current.GoToAsync("///Login");
                    }
                    catch
                    {
                        try
                        {
                            await Shell.Current.GoToAsync("Login");
                        }
                        catch
                        {
                            Application.Current.MainPage = new AppShell();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en CerrarSesion: {ex.Message}");
                    await LimpiarSesionAsync();
                    await ShowAlertAsync("Advertencia",
                        "Hubo un problema al cerrar sesión, pero se limpiaron los datos locales.");

                    try
                    {
                        await Shell.Current.GoToAsync("Login");
                    }
                    catch
                    {
                        Application.Current.MainPage = new AppShell();
                    }
                }
                finally
                {
                    _isLoggingOut = false;
                }
            }
        }

        [RelayCommand]
        private async Task AlternarNotificaciones()
        {
            await ExecuteAsync(async () =>
            {
                // TODO: Implementar actualización en el servidor
                // await _apiService.ActualizarNotificacionesAsync(Usuario.id_usuario, Usuario.notificaciones_push);

                await ShowAlertAsync("Configuración",
                    usuario.notificaciones_push
                        ? "Notificaciones activadas"
                        : "Notificaciones desactivadas");
            });
        }

        private async Task LimpiarSesionAsync()
        {
            try
            {
                Usuario = new Usuarios();
                InicializarPropiedadesFormateadas();
                condicionesMedicas.Clear();
                alergias.Clear();
                condicionesMedicasSeleccionadas.Clear();
                alergiasSeleccionadas.Clear();

                SecureStorage.Remove("user_id");
                SecureStorage.Remove("jwt_token");
                SecureStorage.Remove("refresh_token");
                SecureStorage.Remove("user_email");
                SecureStorage.Remove("user_name");

                Debug.WriteLine("Datos de sesión limpiados correctamente");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al limpiar sesión: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ActualizarPerfil()
        {
            await ExecuteAsync(async () =>
            {
                // TODO: Implementar actualización del perfil
                // await _apiService.ActualizarPerfilAsync(Usuario);
                await ShowAlertAsync("Éxito", "Perfil actualizado correctamente");
            });
        }

        [RelayCommand]
        private async Task RefrescarPerfil()
        {
            await ExecuteAsync(async () =>
            {
                await CargarDatosUsuarioAsync();
                await CargarCondicionesMedicasAsync();
                await CargarAlergiasAsync();
            });
        }
    }
}