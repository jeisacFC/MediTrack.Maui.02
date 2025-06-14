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
            Debug.WriteLine("[PerfilVM] Iniciando initialize async");

            await ExecuteAsync(async () =>
            {
                await CargarDatosUsuarioAsync();
                await CargarCondicionesMedicasAsync();
                await CargarAlergiasAsync();
                Debug.WriteLine("[PerfilVM] Initialize async completado");
            });
        }

        private async Task CargarDatosUsuarioAsync()
        {
            try
            {
                Debug.WriteLine("[PerfilVM] Iniciando carga de datos usuario");

                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    Debug.WriteLine("[PerfilVM] ERROR: user_id inválido");
                    ErrorMessage = "Sesión expirada";
                    await ShowAlertAsync("Error", "Sesión expirada. Por favor, inicia sesión nuevamente.");
                    await Shell.Current.GoToAsync("//Login");
                    return;
                }

                var request = new ReqObtenerUsuario { IdUsuario = userId };
                Debug.WriteLine($"[PerfilVM] Solicitando usuario ID: {userId}");

                var response = await _apiService.GetUserAsync(request);

                if (response?.resultado == true && response.Usuario != null)
                {
                    Debug.WriteLine("[PerfilVM] ✅ Datos usuario obtenidos exitosamente");

                    Usuario = response.Usuario;
                    ActualizarPropiedadesFormateadas();

                    // Solo notificar propiedades principales
                    OnPropertyChanged(nameof(Usuario));
                    OnPropertyChanged(nameof(NombreCompleto));
                    OnPropertyChanged(nameof(FechaNacimientoFormateada));
                    OnPropertyChanged(nameof(FechaRegistroFormateada));
                    OnPropertyChanged(nameof(UltimoAccesoFormateado));
                    OnPropertyChanged(nameof(GeneroTexto));
                    OnPropertyChanged(nameof(EstadoCuenta));
                }
                else
                {
                    Debug.WriteLine("[PerfilVM] ❌ Error en respuesta del servidor");
                    var mensajeError = response?.Mensaje ?? "Error al obtener datos del usuario";

                    if (response?.errores?.Any() == true)
                    {
                        var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                        mensajeError += $". Detalles: {erroresDetalle}";
                    }

                    ErrorMessage = mensajeError;
                    await ShowAlertAsync("Error", mensajeError);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PerfilVM] EXCEPCIÓN en CargarDatosUsuarioAsync: {ex.Message}");
                ErrorMessage = "Error al cargar datos del usuario";
                await HandleErrorAsync(ex);
            }
        }

        private void ActualizarPropiedadesFormateadas()
        {
            if (Usuario == null)
            {
                Debug.WriteLine("[PerfilVM] WARNING: Usuario es null en ActualizarPropiedadesFormateadas");
                return;
            }

            // Nombre completo optimizado
            NombreCompleto = $"{Usuario.nombre} {Usuario.apellido1} {Usuario.apellido2}".Trim();

            // Fecha de nacimiento con validación mejorada
            if (Usuario.fecha_nacimiento.Year > 1900)
            {
                var edad = DateTime.Now.Year - Usuario.fecha_nacimiento.Year;
                if (DateTime.Now.DayOfYear < Usuario.fecha_nacimiento.DayOfYear) edad--;
                FechaNacimientoFormateada = $"{Usuario.fecha_nacimiento:dd/MM/yyyy} ({edad} años)";
            }
            else
            {
                FechaNacimientoFormateada = "No especificada";
            }

            // Fecha de registro
            FechaRegistroFormateada = Usuario.fecha_registro.Year > 1900
                ? Usuario.fecha_registro.ToString("dd/MM/yyyy")
                : "No disponible";

            // Último acceso
            UltimoAccesoFormateado = Usuario.ultimo_acceso.Year > 1900
                ? Usuario.ultimo_acceso.ToString("dd/MM/yyyy HH:mm")
                : "No disponible";

            // Género simplificado
            GeneroTexto = Usuario.id_genero switch
            {
                "1" => "Masculino",
                "2" => "Femenino",
                "3" => "Otro",
                _ => "No especificado"
            };

            // Estado cuenta simplificado
            EstadoCuenta = Usuario.cuenta_bloqueada ? "Bloqueada" :
                          Usuario.intentos_fallidos > 0 ? $"Activa ({Usuario.intentos_fallidos} intentos fallidos)" :
                          "Activa";

            Debug.WriteLine($"[PerfilVM] Usuario actualizado: {NombreCompleto}");
        }

        private async Task CargarCondicionesMedicasAsync()
        {
            try
            {
                Debug.WriteLine("[PerfilVM] Cargando condiciones médicas");

                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    Debug.WriteLine("[PerfilVM] ERROR: user_id inválido para condiciones médicas");
                    ErrorMessage = "No se pudo obtener la información del usuario";
                    return;
                }

                var request = new ReqObtenerCondicionesUsuario { IdUsuario = userId };
                var response = await _apiService.ObtenerCondicionesMedicasAsync(request);

                if (response?.resultado == true)
                {
                    Debug.WriteLine("[PerfilVM] ✅ Condiciones médicas obtenidas");
                    condicionesMedicas.Clear();

                    if (response.Condiciones?.Any() == true)
                    {
                        foreach (var condicion in response.Condiciones)
                        {
                            condicionesMedicas.Add(condicion);
                        }
                        Debug.WriteLine($"[PerfilVM] {condicionesMedicas.Count} condiciones cargadas");
                    }
                    else
                    {
                        Debug.WriteLine("[PerfilVM] Sin condiciones médicas registradas");
                    }

                    OnPropertyChanged(nameof(CondicionesMedicas));
                }
                else
                {
                    Debug.WriteLine("[PerfilVM] ❌ Error obteniendo condiciones médicas");
                    var mensajeError = response?.Mensaje ?? "Error al obtener condiciones médicas";

                    if (response?.errores?.Any() == true)
                    {
                        var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                        mensajeError += $". Detalles: {erroresDetalle}";
                    }

                    ErrorMessage = mensajeError;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PerfilVM] EXCEPCIÓN en CargarCondicionesMedicasAsync: {ex.Message}");
                ErrorMessage = "Error al cargar condiciones médicas";
                await HandleErrorAsync(ex);
            }
        }

        private async Task CargarAlergiasAsync()
        {
            try
            {
                Debug.WriteLine("[PerfilVM] Cargando alergias");

                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    Debug.WriteLine("[PerfilVM] ERROR: user_id inválido para alergias");
                    ErrorMessage = "No se pudo obtener la información del usuario";
                    return;
                }

                var request = new ReqObtenerAlergiasUsuario { IdUsuario = userId };
                var response = await _apiService.ObtenerAlergiasUsuarioAsync(request);

                if (response?.resultado == true)
                {
                    Debug.WriteLine("[PerfilVM] ✅ Alergias obtenidas");
                    alergias.Clear();

                    if (response.Alergias?.Any() == true)
                    {
                        foreach (var alergia in response.Alergias)
                        {
                            alergias.Add(alergia);
                        }
                        Debug.WriteLine($"[PerfilVM] {alergias.Count} alergias cargadas");
                    }
                    else
                    {
                        Debug.WriteLine("[PerfilVM] Sin alergias registradas");
                    }

                    OnPropertyChanged(nameof(Alergias));
                }
                else
                {
                    Debug.WriteLine("[PerfilVM] ❌ Error obteniendo alergias");
                    var mensajeError = response?.Mensaje ?? "Error al obtener alergias";

                    if (response?.errores?.Any() == true)
                    {
                        var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                        mensajeError += $". Detalles: {erroresDetalle}";
                    }

                    ErrorMessage = mensajeError;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PerfilVM] EXCEPCIÓN en CargarAlergiasAsync: {ex.Message}");
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
                Debug.WriteLine("[PerfilVM] Abriendo popup editar perfil");

                if (Usuario == null)
                {
                    await ShowAlertAsync("Error", "No hay información del usuario para editar");
                    return;
                }

                var popupViewModel = new ActualizarPerfilPopupViewModel(_apiService, Usuario);
                var popup = new ActualizarPerfilPopup(popupViewModel);
                var resultado = await Shell.Current.ShowPopupAsync(popup);

                Debug.WriteLine($"[PerfilVM] Modal editar perfil cerrado: {resultado}");

                // Siempre refrescar después del modal
                await CargarDatosUsuarioAsync();
                OnPropertyChanged(nameof(Usuario));
                OnPropertyChanged(nameof(NombreCompleto));
                OnPropertyChanged(nameof(FechaNacimientoFormateada));
                OnPropertyChanged(nameof(GeneroTexto));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PerfilVM] Error en EditarPerfil: {ex.Message}");
                await ShowAlertAsync("Error", "Error al abrir el formulario de edición");
            }
        }

        [RelayCommand]
        private async Task GestionarCondicionesMedicas()
        {
            try
            {
                Debug.WriteLine("[PerfilVM] Abriendo gestión condiciones médicas");

                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    await ShowAlertAsync("Error", "No se pudo obtener la información del usuario");
                    return;
                }

                var condicionesViewModel = new CondicionesMedicasViewModel(_apiService, userId);
                var popup = new GestionCondicionesMedicasPopup(condicionesViewModel);
                var resultado = await Shell.Current.ShowPopupAsync(popup);

                Debug.WriteLine($"[PerfilVM] Modal condiciones cerrado: {resultado}");

                // Siempre refrescar después del modal
                await CargarCondicionesMedicasAsync();
                OnPropertyChanged(nameof(CondicionesMedicas));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PerfilVM] Error en GestionarCondicionesMedicas: {ex.Message}");
                await ShowAlertAsync("Error", "Error al abrir la gestión de condiciones médicas");
            }
        }

        [RelayCommand]
        private async Task GestionarAlergias()
        {
            try
            {
                Debug.WriteLine("[PerfilVM] Abriendo gestión alergias");

                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    await ShowAlertAsync("Error", "No se pudo obtener la información del usuario");
                    return;
                }

                var alergiasViewModel = new AlergiasViewModel(_apiService, userId);
                var popup = new GestionAlergiasPopup(alergiasViewModel);
                var resultado = await Shell.Current.ShowPopupAsync(popup);

                Debug.WriteLine($"[PerfilVM] Modal alergias cerrado: {resultado}");

                // Siempre refrescar después del modal
                await CargarAlergiasAsync();
                OnPropertyChanged(nameof(Alergias));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PerfilVM] Error en GestionarAlergias: {ex.Message}");
                await ShowAlertAsync("Error", "Error al abrir la gestión de alergias");
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
                    var logoutRequest = new ReqLogout { InvalidarTodos = false };
                    var response = await _apiService.LogoutAsync(logoutRequest);

                    if (response?.resultado == true && response.LogoutExitoso)
                    {
                        Debug.WriteLine($"[PerfilVM] Logout exitoso. Tokens invalidados: {response.TokensInvalidados}");
                    }
                    else
                    {
                        var mensajeError = response?.Mensaje ?? "Error al cerrar sesión en el servidor";
                        Debug.WriteLine($"[PerfilVM] Error en logout: {mensajeError}");
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
                    Debug.WriteLine($"[PerfilVM] Error en CerrarSesion: {ex.Message}");
                    await LimpiarSesionAsync();
                    await ShowAlertAsync("Advertencia", "Hubo un problema al cerrar sesión, pero se limpiaron los datos locales.");

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
                await ShowAlertAsync("Configuración",
                    usuario.notificaciones_push
                        ? "Notificaciones activadas"
                        : "Notificaciones desactivadas");
            });
        }

        [RelayCommand]
        private async Task RefrescarPerfil()
        {
            await ExecuteAsync(async () =>
            {
                Debug.WriteLine("[PerfilVM] Iniciando refresco completo");
                await CargarDatosUsuarioAsync();
                await CargarCondicionesMedicasAsync();
                await CargarAlergiasAsync();
                Debug.WriteLine("[PerfilVM] Refresco completado");
            });
        }

        [RelayCommand]
        private async Task ActualizarPerfil()
        {
            await ExecuteAsync(async () =>
            {
                // TODO: Implementar actualización del perfil
                await ShowAlertAsync("Éxito", "Perfil actualizado correctamente");
            });
        }

        [RelayCommand]
        private async Task EditarInformacionPersonal()
        {
            try
            {
                Debug.WriteLine("[PerfilVM] Abriendo popup información personal");

                if (Usuario == null)
                {
                    await ShowAlertAsync("Error", "No hay información del usuario para editar");
                    return;
                }

                var popupViewModel = new ActualizarPerfilPopupViewModel(_apiService, Usuario);
                var popup = new ActualizarPerfilPopup(popupViewModel);
                var resultado = await Shell.Current.ShowPopupAsync(popup);

                Debug.WriteLine($"[PerfilVM] Modal información personal cerrado: {resultado}");

                // Siempre refrescar después del modal
                await CargarDatosUsuarioAsync();
                OnPropertyChanged(nameof(Usuario));
                OnPropertyChanged(nameof(NombreCompleto));
                OnPropertyChanged(nameof(FechaNacimientoFormateada));
                OnPropertyChanged(nameof(GeneroTexto));
                OnPropertyChanged(nameof(EstadoCuenta));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PerfilVM] Error en EditarInformacionPersonal: {ex.Message}");
                await ShowAlertAsync("Error", "Error al abrir el formulario de edición");
            }
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

                Debug.WriteLine("[PerfilVM] Datos de sesión limpiados");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PerfilVM] Error limpiando sesión: {ex.Message}");
            }
        }
    }
}