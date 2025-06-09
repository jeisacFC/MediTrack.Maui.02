using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Popups;
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
        public IApiService GetApiService()
        {
            return _apiService;
        }
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

                if (response != null && response.resultado && response.Usuario != null)
                {
                    Debug.WriteLine("=== RESPUESTA VÁLIDA DEL SERVIDOR ===");
                    Debug.WriteLine("=== ACTUALIZANDO DATOS DEL USUARIO ===");

                    // Actualizar el usuario - FORZAR ACTUALIZACIÓN DE UI
                    Usuario = response.Usuario;
                    Debug.WriteLine($"Usuario asignado: {Usuario.nombre} {Usuario.apellido1}");

                    // Actualizar propiedades formateadas
                    ActualizarPropiedadesFormateadas();

                    // FORZAR NOTIFICACIONES DE CAMBIO
                    OnPropertyChanged(nameof(Usuario));
                    OnPropertyChanged(nameof(NombreCompleto));
                    OnPropertyChanged(nameof(FechaNacimientoFormateada));
                    OnPropertyChanged(nameof(FechaRegistroFormateada));
                    OnPropertyChanged(nameof(UltimoAccesoFormateado));
                    OnPropertyChanged(nameof(GeneroTexto));
                    OnPropertyChanged(nameof(EstadoCuenta));

                    Debug.WriteLine("=== DATOS USUARIO CARGADOS EXITOSAMENTE ===");
                    Debug.WriteLine($"NombreCompleto final: '{NombreCompleto}'");
                    Debug.WriteLine($"Email final: '{Usuario.email}'");
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
            if (Usuario == null)
            {
                Debug.WriteLine("WARNING: Usuario es null en ActualizarPropiedadesFormateadas");
                return;
            }

            Debug.WriteLine("=== INICIANDO ACTUALIZACIÓN DE PROPIEDADES FORMATEADAS ===");

            // Nombre completo
            var nombre = Usuario.nombre ?? "";
            var apellido1 = Usuario.apellido1 ?? "";
            var apellido2 = Usuario.apellido2 ?? "";
            NombreCompleto = $"{nombre} {apellido1} {apellido2}".Trim();
            Debug.WriteLine($"Nombre completo calculado: '{NombreCompleto}'");

            // Fecha de nacimiento
            if (Usuario.fecha_nacimiento != DateTime.MinValue && Usuario.fecha_nacimiento.Year > 1900)
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
            Debug.WriteLine($"Fecha nacimiento formateada: '{FechaNacimientoFormateada}'");

            // Fecha de registro
            if (Usuario.fecha_registro != DateTime.MinValue && Usuario.fecha_registro.Year > 1900)
            {
                FechaRegistroFormateada = Usuario.fecha_registro.ToString("dd/MM/yyyy");
            }
            else
            {
                FechaRegistroFormateada = "No disponible";
            }
            Debug.WriteLine($"Fecha registro formateada: '{FechaRegistroFormateada}'");

            // Último acceso
            if (Usuario.ultimo_acceso != DateTime.MinValue && Usuario.ultimo_acceso.Year > 1900)
            {
                UltimoAccesoFormateado = Usuario.ultimo_acceso.ToString("dd/MM/yyyy HH:mm");
            }
            else
            {
                UltimoAccesoFormateado = "No disponible";
            }
            Debug.WriteLine($"Último acceso formateado: '{UltimoAccesoFormateado}'");

            // Género
            GeneroTexto = Usuario.id_genero switch
            {
                "1" => "Masculino",
                "2" => "Femenino",
                "3" => "Otro",
                _ => "No especificado"
            };
            Debug.WriteLine($"Género texto: '{GeneroTexto}'");

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
            Debug.WriteLine($"Estado cuenta: '{EstadoCuenta}'");

            Debug.WriteLine("=== PROPIEDADES FORMATEADAS ACTUALIZADAS ===");
        }

        private async Task CargarCondicionesMedicasAsync()
        {
            try
            {
                Debug.WriteLine("=== CARGANDO CONDICIONES MÉDICAS DEL USUARIO ===");

                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    Debug.WriteLine("ERROR: No se pudo obtener user_id del storage para condiciones médicas");
                    ErrorMessage = "No se pudo obtener la información del usuario";
                    return;
                }

                Debug.WriteLine($"Obteniendo condiciones médicas para userId: {userId}");

                var request = new ReqObtenerCondicionesUsuario
                {
                    IdUsuario = userId
                };

                var response = await _apiService.ObtenerCondicionesMedicasAsync(request);

                Debug.WriteLine($"Respuesta condiciones médicas - resultado: {response?.resultado}");

                if (response != null && response.resultado)
                {
                    Debug.WriteLine("=== CONDICIONES MÉDICAS OBTENIDAS EXITOSAMENTE ===");

                    condicionesMedicas.Clear();

                    if (response.Condiciones != null && response.Condiciones.Any())
                    {
                        foreach (var condicion in response.Condiciones)
                        {
                            condicionesMedicas.Add(condicion);
                            Debug.WriteLine($"Condición añadida: {condicion.nombre_condicion}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("El usuario no tiene condiciones médicas registradas");
                    }

                    OnPropertyChanged(nameof(CondicionesMedicas));
                    Debug.WriteLine($"Total condiciones médicas cargadas: {condicionesMedicas.Count}");
                }
                else
                {
                    Debug.WriteLine("ERROR: Respuesta inválida del servidor para condiciones médicas");
                    var mensajeError = response?.Mensaje ?? "Error desconocido al obtener condiciones médicas";

                    if (response?.errores != null && response.errores.Any())
                    {
                        var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                        mensajeError += $". Detalles: {erroresDetalle}";
                    }

                    ErrorMessage = mensajeError;
                    Debug.WriteLine($"Error condiciones médicas: {mensajeError}");

                    // No mostramos alerta aquí para no interrumpir la carga de datos
                    // Solo logueamos el error
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPCIÓN en CargarCondicionesMedicasAsync: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ErrorMessage = "Error al cargar condiciones médicas";
                await HandleErrorAsync(ex);
            }
        }

        private async Task CargarAlergiasAsync()
        {
            try
            {
                Debug.WriteLine("=== CARGANDO ALERGIAS DEL USUARIO ===");

                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    Debug.WriteLine("ERROR: No se pudo obtener user_id del storage para alergias");
                    ErrorMessage = "No se pudo obtener la información del usuario";
                    return;
                }

                Debug.WriteLine($"Obteniendo alergias para userId: {userId}");

                var request = new ReqObtenerAlergiasUsuario
                {
                    IdUsuario = userId
                };

                var response = await _apiService.ObtenerAlergiasUsuarioAsync(request);

                Debug.WriteLine($"Respuesta alergias - resultado: {response?.resultado}");

                if (response != null && response.resultado)
                {
                    Debug.WriteLine("=== ALERGIAS OBTENIDAS EXITOSAMENTE ===");

                    alergias.Clear();

                    if (response.Alergias != null && response.Alergias.Any())
                    {
                        foreach (var alergia in response.Alergias)
                        {
                            alergias.Add(alergia);
                            Debug.WriteLine($"Alergia añadida: {alergia.nombre_alergia}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("El usuario no tiene alergias registradas");
                    }

                    OnPropertyChanged(nameof(Alergias));
                    Debug.WriteLine($"Total alergias cargadas: {alergias.Count}");
                }
                else
                {
                    Debug.WriteLine("ERROR: Respuesta inválida del servidor para alergias");
                    var mensajeError = response?.Mensaje ?? "Error desconocido al obtener alergias";

                    if (response?.errores != null && response.errores.Any())
                    {
                        var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                        mensajeError += $". Detalles: {erroresDetalle}";
                    }

                    ErrorMessage = mensajeError;
                    Debug.WriteLine($"Error alergias: {mensajeError}");

                    // No mostramos alerta aquí para no interrumpir la carga de datos
                    // Solo logueamos el error
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPCIÓN en CargarAlergiasAsync: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ErrorMessage = "Error al cargar alergias";
                await HandleErrorAsync(ex);
            }
        }

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
            try
            {
                Debug.WriteLine("=== ABRIENDO POPUP DE EDITAR PERFIL ===");

                if (Usuario == null)
                {
                    await ShowAlertAsync("Error", "No hay información del usuario para editar");
                    return;
                }

                // Crear el ViewModel del popup
                var popupViewModel = new ActualizarPerfilPopupViewModel(_apiService, Usuario);

                // Crear y mostrar el popup
                var popup = new ActualizarPerfilPopup(popupViewModel);
                var resultado = await Shell.Current.ShowPopupAsync(popup);

                // Si se actualizó correctamente, refrescar los datos
                if (resultado is bool actualizado && actualizado)
                {
                    Debug.WriteLine("=== PERFIL ACTUALIZADO, REFRESCANDO DATOS ===");

                    // Actualizar las propiedades formateadas con los nuevos datos
                    ActualizarPropiedadesFormateadas();

                    // Forzar notificaciones de cambio para actualizar la UI
                    OnPropertyChanged(nameof(Usuario));
                    OnPropertyChanged(nameof(NombreCompleto));
                    OnPropertyChanged(nameof(FechaNacimientoFormateada));
                    OnPropertyChanged(nameof(GeneroTexto));

                    Debug.WriteLine("=== DATOS ACTUALIZADOS EN LA UI ===");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir popup de editar perfil: {ex.Message}");
                await ShowAlertAsync("Error", "Error al abrir el formulario de edición");
            }
        }
        [RelayCommand]
        private async Task GestionarCondicionesMedicas()
        {
            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    await ShowAlertAsync("Error", "No se pudo obtener la información del usuario");
                    return;
                }

                // Crear el ViewModel específico para condiciones
                var condicionesViewModel = new CondicionesMedicasViewModel(_apiService, userId);

                // Crear y mostrar el modal/popup
                var popup = new GestionCondicionesMedicasPopup(condicionesViewModel);
                var resultado = await Shell.Current.ShowPopupAsync(popup);

                // Si hubo cambios, refrescar las condiciones en el perfil
                if (resultado is bool actualizado && actualizado)
                {
                    await CargarCondicionesMedicasAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir gestión de condiciones: {ex.Message}");
                await ShowAlertAsync("Error", "Error al abrir la gestión de condiciones médicas");
            }
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
                Debug.WriteLine("=== INICIANDO REFRESCO DEL PERFIL ===");
                await CargarDatosUsuarioAsync();
                await CargarCondicionesMedicasAsync();
                await CargarAlergiasAsync();
                Debug.WriteLine("=== REFRESCO DEL PERFIL COMPLETADO ===");
            });
        }
    }
}