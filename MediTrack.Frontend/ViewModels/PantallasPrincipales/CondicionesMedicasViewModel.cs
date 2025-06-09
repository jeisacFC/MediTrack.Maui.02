using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MediTrack.Frontend.ViewModels
{
    public partial class CondicionesMedicasViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly int _userId;

        [ObservableProperty]
        private ObservableCollection<CondicionesMedicas> condicionesDisponibles;

        [ObservableProperty]
        private ObservableCollection<CondicionesMedicas> condicionesUsuario;

        [ObservableProperty]
        private ObservableCollection<CondicionesMedicas> condicionesSeleccionadas;

        [ObservableProperty]
        private CondicionesMedicas condicionSeleccionada;

        [ObservableProperty]
        private string nombreNuevaCondicion = string.Empty;

        [ObservableProperty]
        private string descripcionNuevaCondicion = string.Empty;

        [ObservableProperty]
        private bool mostrarFormularioNuevaCondicion = false;

        public CondicionesMedicasViewModel(IApiService apiService, int userId)
        {
            _apiService = apiService;
            _userId = userId;
            Title = "Gestión de Condiciones Médicas";

            // Inicializar colecciones
            condicionesDisponibles = new ObservableCollection<CondicionesMedicas>();
            condicionesUsuario = new ObservableCollection<CondicionesMedicas>();
            condicionesSeleccionadas = new ObservableCollection<CondicionesMedicas>();
        }

        public override async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                await CargarCondicionesDisponiblesAsync();
                await CargarCondicionesUsuarioAsync();
            });
        }

        private async Task CargarCondicionesDisponiblesAsync()
        {
            try
            {
                Debug.WriteLine("=== CARGANDO CONDICIONES DISPONIBLES ===");

                var request = new ReqListarCondiciones();
                var response = await _apiService.ListarCondicionesMedicasAsync(request);

                if (response != null && response.resultado && response.Condiciones != null)
                {
                    condicionesDisponibles.Clear();
                    foreach (var condicion in response.Condiciones)
                    {
                        condicionesDisponibles.Add(condicion);
                    }

                    Debug.WriteLine($"Condiciones disponibles cargadas: {condicionesDisponibles.Count}");
                }
                else
                {
                    var mensaje = response?.Mensaje ?? "Error al cargar condiciones disponibles";
                    Debug.WriteLine($"Error: {mensaje}");
                    ErrorMessage = mensaje;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción en CargarCondicionesDisponiblesAsync: {ex.Message}");
                ErrorMessage = "Error al cargar condiciones disponibles";
                await HandleErrorAsync(ex);
            }
        }

        private async Task CargarCondicionesUsuarioAsync()
        {
            try
            {
                Debug.WriteLine("=== CARGANDO CONDICIONES DEL USUARIO ===");

                var request = new ReqObtenerCondicionesUsuario
                {
                    IdUsuario = _userId
                };

                var response = await _apiService.ObtenerCondicionesMedicasAsync(request);

                if (response != null && response.resultado)
                {
                    condicionesUsuario.Clear();
                    if (response.Condiciones != null)
                    {
                        foreach (var condicion in response.Condiciones)
                        {
                            condicionesUsuario.Add(condicion);
                        }
                    }

                    Debug.WriteLine($"Condiciones del usuario cargadas: {condicionesUsuario.Count}");
                }
                else
                {
                    var mensaje = response?.Mensaje ?? "Error al cargar condiciones del usuario";
                    Debug.WriteLine($"Error: {mensaje}");
                    // No mostramos error aquí ya que puede ser normal no tener condiciones
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción en CargarCondicionesUsuarioAsync: {ex.Message}");
                ErrorMessage = "Error al cargar condiciones del usuario";
                await HandleErrorAsync(ex);
            }
        }

        [RelayCommand]
        private async Task CrearNuevaCondicion()
        {
            if (string.IsNullOrWhiteSpace(nombreNuevaCondicion))
            {
                await ShowAlertAsync("Error", "El nombre de la condición es requerido");
                return;
            }

            await ExecuteAsync(async () =>
            {
                try
                {
                    Debug.WriteLine($"=== CREANDO NUEVA CONDICIÓN: {nombreNuevaCondicion} ===");

                    var request = new ReqInsertarCondicion
                    {
                        NombreCondicion = nombreNuevaCondicion.Trim(),
                        Descripcion = descripcionNuevaCondicion?.Trim()
                    };

                    var response = await _apiService.InsertarCondicionAsync(request);

                    if (response != null && response.resultado)
                    {
                        await ShowAlertAsync("Éxito", "Condición médica creada correctamente");

                        // Limpiar formulario
                        nombreNuevaCondicion = string.Empty;
                        descripcionNuevaCondicion = string.Empty;
                        mostrarFormularioNuevaCondicion = false;

                        // Recargar condiciones disponibles
                        await CargarCondicionesDisponiblesAsync();

                        Debug.WriteLine("=== CONDICIÓN CREADA EXITOSAMENTE ===");
                    }
                    else
                    {
                        var mensaje = response?.Mensaje ?? "Error al crear la condición médica";
                        if (response?.errores != null && response.errores.Any())
                        {
                            var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                            mensaje += $". Detalles: {erroresDetalle}";
                        }

                        await ShowAlertAsync("Error", mensaje);
                        Debug.WriteLine($"Error al crear condición: {mensaje}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Excepción en CrearNuevaCondicion: {ex.Message}");
                    await ShowAlertAsync("Error", "Error al crear la condición médica");
                }
            });
        }

        [RelayCommand]
        private async Task AsignarCondicionesSeleccionadas()
        {
            if (!condicionesSeleccionadas.Any())
            {
                await ShowAlertAsync("Información", "No hay condiciones seleccionadas para asignar");
                return;
            }

            await ExecuteAsync(async () =>
            {
                var condicionesAsignadas = 0;
                var errores = new List<string>();

                foreach (var condicion in condicionesSeleccionadas.ToList())
                {
                    try
                    {
                        // Verificar si ya está asignada
                        if (condicionesUsuario.Any(c => c.id_condicion == condicion.id_condicion))
                        {
                            Debug.WriteLine($"Condición {condicion.nombre_condicion} ya está asignada");
                            continue;
                        }

                        var request = new ReqAsignarCondicionUsuario
                        {
                            IdUsuario = _userId,
                            IdCondicion = condicion.id_condicion
                        };

                        var response = await _apiService.AsignarCondicionUsuarioAsync(request);

                        if (response != null && response.resultado)
                        {
                            condicionesUsuario.Add(condicion);
                            condicionesAsignadas++;
                            Debug.WriteLine($"Condición {condicion.nombre_condicion} asignada correctamente");
                        }
                        else
                        {
                            var mensaje = response?.Mensaje ?? $"Error al asignar {condicion.nombre_condicion}";
                            errores.Add(mensaje);
                            Debug.WriteLine($"Error al asignar condición: {mensaje}");
                        }
                    }
                    catch (Exception ex)
                    {
                        var mensaje = $"Error al asignar {condicion.nombre_condicion}: {ex.Message}";
                        errores.Add(mensaje);
                        Debug.WriteLine($"Excepción: {mensaje}");
                    }
                }

                // Limpiar selección
                condicionesSeleccionadas.Clear();

                // Mostrar resultado
                var mensajeResultado = $"Condiciones asignadas: {condicionesAsignadas}";
                if (errores.Any())
                {
                    mensajeResultado += $"\nErrores: {string.Join(", ", errores)}";
                }

                await ShowAlertAsync(errores.Any() ? "Completado con errores" : "Éxito", mensajeResultado);

                Debug.WriteLine($"=== ASIGNACIÓN COMPLETADA: {condicionesAsignadas} exitosas, {errores.Count} errores ===");
            });
        }

        [RelayCommand]
        private async Task DesasignarCondicion(CondicionesMedicas condicion)
        {
            if (condicion == null) return;

            var confirmar = await ShowConfirmAsync(
                "Confirmar",
                $"¿Estás seguro que deseas desasignar la condición '{condicion.nombre_condicion}'?",
                "Desasignar",
                "Cancelar");

            if (!confirmar) return;

            await ExecuteAsync(async () =>
            {
                try
                {
                    Debug.WriteLine($"=== DESASIGNANDO CONDICIÓN: {condicion.nombre_condicion} ===");

                    var request = new ReqDesasignarCondicionUsuario
                    {
                        IdUsuario = _userId,
                        IdCondicion = condicion.id_condicion
                    };

                    var response = await _apiService.DesasignarCondicionUsuarioAsync(request);

                    if (response != null && response.resultado)
                    {
                        condicionesUsuario.Remove(condicion);
                        await ShowAlertAsync("Éxito", "Condición desasignada correctamente");

                        Debug.WriteLine("=== CONDICIÓN DESASIGNADA EXITOSAMENTE ===");
                    }
                    else
                    {
                        var mensaje = response?.Mensaje ?? "Error al desasignar la condición";
                        if (response?.errores != null && response.errores.Any())
                        {
                            var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                            mensaje += $". Detalles: {erroresDetalle}";
                        }

                        await ShowAlertAsync("Error", mensaje);
                        Debug.WriteLine($"Error al desasignar condición: {mensaje}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Excepción en DesasignarCondicion: {ex.Message}");
                    await ShowAlertAsync("Error", "Error al desasignar la condición");
                }
            });
        }

        [RelayCommand]
        private void ToggleMostrarFormularioNuevaCondicion()
        {
            mostrarFormularioNuevaCondicion = !mostrarFormularioNuevaCondicion;

            if (!mostrarFormularioNuevaCondicion)
            {
                // Limpiar formulario al ocultar
                nombreNuevaCondicion = string.Empty;
                descripcionNuevaCondicion = string.Empty;
            }
        }

        [RelayCommand]
        private void CancelarNuevaCondicion()
        {
            nombreNuevaCondicion = string.Empty;
            descripcionNuevaCondicion = string.Empty;
            mostrarFormularioNuevaCondicion = false;
        }

        [RelayCommand]
        private async Task RefrescarDatos()
        {
            await ExecuteAsync(async () =>
            {
                Debug.WriteLine("=== REFRESCANDO DATOS DE CONDICIONES ===");
                await CargarCondicionesDisponiblesAsync();
                await CargarCondicionesUsuarioAsync();
                Debug.WriteLine("=== REFRESCO COMPLETADO ===");
            });
        }

        // Método para manejar selección múltiple desde la vista
        public void OnCondicionesSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            condicionesSeleccionadas.Clear();
            foreach (CondicionesMedicas condicion in e.CurrentSelection)
            {
                condicionesSeleccionadas.Add(condicion);
            }

            Debug.WriteLine($"Condiciones seleccionadas: {condicionesSeleccionadas.Count}");
        }

        // Propiedades computed para la UI
        public bool TieneCondicionesUsuario => condicionesUsuario?.Any() == true;
        public bool TieneCondicionesDisponibles => condicionesDisponibles?.Any() == true;
        public bool TieneCondicionesSeleccionadas => condicionesSeleccionadas?.Any() == true;
    }
}