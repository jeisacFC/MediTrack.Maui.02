using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Services;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels.Base;
using System.Collections.ObjectModel;

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
                // TODO: Implementar cuando tengas el método en IApiService
                // Usuario = await _apiService.ObtenerUsuarioActualAsync();

                // Datos de ejemplo para pruebas - remover cuando implementes el servicio
                usuario = new Usuario
                {
                    id_usuario = 1,
                    nombre = "Jeremy",
                    apellido1 = "Duran",
                    apellido2 = "Vargas",
                    email = "jeremy@meditrack.com",
                    fecha_nacimiento = new DateTime(2003, 11, 2),
                    id_genero = "Masculino",
                    fecha_registro = DateTime.Now.AddMonths(-6),
                    ultimo_acceso = DateTime.Now.AddHours(-2),
                    notificaciones_push = true,
                    cuenta_bloqueada = false,
                    intentos_fallidos = 0
                };

                // Notificar cambio en nombre completo
                OnPropertyChanged(nameof(NombreCompleto));
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al cargar datos del usuario";
                await HandleErrorAsync(ex);
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

        // Comando para cerrar sesión
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
                    // Limpiar datos de sesión
                    await LimpiarSesionAsync();

                    // Navegar a la pantalla de login
                    await Shell.Current.GoToAsync("//Login");
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
            // Limpiar datos locales
            usuario = new Usuario();
            condicionesMedicas.Clear();
            alergias.Clear();
            condicionesMedicasSeleccionadas.Clear();
            alergiasSeleccionadas.Clear();

            // TODO: Limpiar tokens de autenticación, preferencias, etc.
            // await SecureStorage.Default.RemoveAsync("auth_token");

            await Task.CompletedTask;
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
    }
}