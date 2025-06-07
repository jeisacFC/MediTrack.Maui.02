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

        [ObservableProperty]
        private Usuario usuario;

        [ObservableProperty]
        private ObservableCollection<CondicionesMedicas> condicionesMedicas;

        [ObservableProperty]
        private ObservableCollection<Alergias> alergias;

        [ObservableProperty]
        private ObservableCollection<CondicionesMedicas> condicionesMedicasSeleccionadas;

        [ObservableProperty]
        private ObservableCollection<Alergias> alergiasSeleccionadas;

        public PerfilViewModel(IApiService apiService)
        {
            _apiService = apiService;
            Title = "Perfil";

            // Inicializar colecciones
            condicionesMedicas = new ObservableCollection<CondicionesMedicas>();
            alergias = new ObservableCollection<Alergias>();
            condicionesMedicasSeleccionadas = new ObservableCollection<CondicionesMedicas>();
            alergiasSeleccionadas = new ObservableCollection<Alergias>();

            // Inicializar usuario vacío
            usuario = new Usuario();
        }

        // Propiedad calculada para el nombre completo
        public string NombreCompleto => usuario != null
            ? $"{usuario.nombre} {usuario.apellido1} {usuario.apellido2}".Trim()
            : string.Empty;

        public override async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                await CargarDatosUsuarioAsync();
                await CargarCondicionesMedicasAsync();
                await CargarAlergiasAsync();
            });
        }

        private async Task CargarDatosUsuarioAsync()
        {
            try
            {
                // Obtener ID del usuario desde el almacenamiento seguro
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    ErrorMessage = "No se pudo obtener la información del usuario";
                    await ShowAlertAsync("Error", "Sesión expirada. Por favor, inicia sesión nuevamente.");
                    await Shell.Current.GoToAsync("//Login");
                    return;
                }

                // Crear el request para obtener usuario
                var request = new ReqObtenerUsuario
                {
                    IdUsuario = userId
                };

                // Llamar al servicio API
                var response = await _apiService.GetUserAsync(request);

                if (response != null && response.resultado && response.Usuario != null)
                {
                    usuario = response.Usuario;

                    // Notificar cambio en nombre completo
                    OnPropertyChanged(nameof(NombreCompleto));

                    Debug.WriteLine($"Usuario cargado exitosamente: {usuario.nombre} {usuario.apellido1}");
                }
                else
                {
                    // Si hay errores específicos, mostrarlos
                    var mensajeError = response?.Mensaje ?? "Error desconocido al obtener datos del usuario";

                    if (response?.errores != null && response.errores.Any())
                    {
                        var erroresDetalle = string.Join(", ", response.errores.Select(e => e.Message));
                        mensajeError += $". Detalles: {erroresDetalle}";
                    }

                    ErrorMessage = mensajeError;
                    await ShowAlertAsync("Error", mensajeError);

                    // Si es un error de autenticación, redirigir al login
                    if (mensajeError.Contains("autenticado") || mensajeError.Contains("401"))
                    {
                        await Shell.Current.GoToAsync("//Login");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al cargar datos del usuario";
                await HandleErrorAsync(ex);
                Debug.WriteLine($"Error en CargarDatosUsuarioAsync: {ex.Message}");
            }
        }

        private async Task CargarCondicionesMedicasAsync()
        {
            try
            {
                // TODO: Implementar cuando tengas el método en IApiService
                // var condiciones = await _apiService.ObtenerCondicionesMedicasUsuarioAsync(Usuario.id_usuario);

                // Datos de ejemplo para pruebas - remover cuando implementes el servicio
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

                // Datos de ejemplo para pruebas - remover cuando implementes el servicio
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

        // Comando para editar perfil
        [RelayCommand]
        private async Task EditarPerfil()
        {
            await ExecuteAsync(async () =>
            {
                // Navegar a la pantalla de edición de perfil
                await Shell.Current.GoToAsync("//EditarPerfil");
            });
        }

        // Comando para cerrar sesión - IMPLEMENTADO
        [RelayCommand]
        private async Task CerrarSesion()
        {
            var confirmar = await ShowConfirmAsync(
                "Cerrar Sesión",
                "¿Estás seguro que deseas cerrar sesión?",
                "Cerrar Sesión",
                "Cancelar");

            if (confirmar)
            {
                await ExecuteAsync(async () =>
                {
                    try
                    {
                        // Crear el request para logout
                        var logoutRequest = new ReqLogout
                        {
                            InvalidarTodos = false // Por defecto, solo invalida la sesión actual
                        };

                        // Llamar al servicio de logout
                        var response = await _apiService.LogoutAsync(logoutRequest);

                        if (response != null && response.resultado && response.LogoutExitoso)
                        {
                            // Logout exitoso
                            await ShowAlertAsync("Éxito", response.Mensaje ?? "Sesión cerrada correctamente");

                            Debug.WriteLine($"Logout exitoso. Tokens invalidados: {response.TokensInvalidados}");
                        }
                        else
                        {
                            // Error en logout, pero aún así limpiar datos locales
                            var mensajeError = response?.Mensaje ?? "Error al cerrar sesión en el servidor";
                            Debug.WriteLine($"Error en logout del servidor: {mensajeError}");

                            await ShowAlertAsync("Advertencia",
                                "Hubo un problema al cerrar sesión en el servidor, pero se limpiaron los datos locales.");
                        }

                        // Limpiar datos locales independientemente del resultado del servidor
                        await LimpiarSesionAsync();

                        // Navegar a la pantalla de login
                        await Shell.Current.GoToAsync("//Login");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error en CerrarSesion: {ex.Message}");

                        // En caso de error, limpiar datos locales de todas formas
                        await LimpiarSesionAsync();

                        await ShowAlertAsync("Advertencia",
                            "Hubo un problema al cerrar sesión, pero se limpiaron los datos locales.");

                        await Shell.Current.GoToAsync("//Login");
                    }
                });
            }
        }

        // Comando para alternar notificaciones
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
                // Limpiar datos del ViewModel
                usuario = new Usuario();
                condicionesMedicas.Clear();
                alergias.Clear();
                condicionesMedicasSeleccionadas.Clear();
                alergiasSeleccionadas.Clear();

                // Notificar cambio en nombre completo
                OnPropertyChanged(nameof(NombreCompleto));

                // Limpiar datos del almacenamiento seguro
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
                // No relanzar la excepción para no interrumpir el flujo de logout
            }
        }

        // Método para actualizar el perfil
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

        // Comando para refrescar datos del perfil
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