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
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        CondicionesDisponibles.Clear();
                        foreach (var condicion in response.Condiciones)
                        {
                            CondicionesDisponibles.Add(condicion);
                        }

                        // Notificar cambios en las condiciones filtradas
                        NotificarCambiosCondicionesDisponibles();
                    });

                    Debug.WriteLine($"Condiciones disponibles cargadas: {CondicionesDisponibles.Count}");
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

        // Actualizar el método CargarCondicionesUsuarioAsync
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
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        CondicionesUsuario.Clear();
                        if (response.Condiciones != null)
                        {
                            foreach (var condicion in response.Condiciones)
                            {
                                CondicionesUsuario.Add(condicion);
                            }
                        }

                        // Notificar cambios en las condiciones filtradas
                        NotificarCambiosCondicionesDisponibles();
                    });

                    Debug.WriteLine($"Condiciones del usuario cargadas: {CondicionesUsuario.Count}");
                }
                else
                {
                    var mensaje = response?.Mensaje ?? "Error al cargar condiciones del usuario";
                    Debug.WriteLine($"Error: {mensaje}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción en CargarCondicionesUsuarioAsync: {ex.Message}");
                ErrorMessage = "Error al cargar condiciones del usuario";
                await HandleErrorAsync(ex);
            }
        }

        // Actualizar el comando AsignarCondicionIndividual
        [RelayCommand]
        private async Task AsignarCondicionIndividual(CondicionesMedicas condicion)
        {
            if (condicion == null) return;

            // Verificar si ya está asignada
            if (CondicionesUsuario.Any(c => c.id_condicion == condicion.id_condicion))
            {
                await ShowAlertAsync("Información", $"La condición '{condicion.nombre_condicion}' ya está asignada");
                return;
            }

            await ExecuteAsync(async () =>
            {
                try
                {
                    Debug.WriteLine($"=== ASIGNANDO CONDICIÓN INDIVIDUAL: {condicion.nombre_condicion} ===");

                    var request = new ReqAsignarCondicionUsuario
                    {
                        IdUsuario = _userId,
                        IdCondicion = condicion.id_condicion
                    };

                    var response = await _apiService.AsignarCondicionUsuarioAsync(request);

                    if (response != null && response.resultado)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            CondicionesUsuario.Add(condicion);
                            // Notificar cambios para actualizar la lista filtrada
                            NotificarCambiosCondicionesDisponibles();
                        });

                        await ShowAlertAsync("Éxito", $"Condición '{condicion.nombre_condicion}' asignada correctamente");

                        Debug.WriteLine($"=== CONDICIÓN {condicion.nombre_condicion} ASIGNADA EXITOSAMENTE ===");
                    }
                    else
                    {
                        var mensaje = response?.Mensaje ?? $"Error al asignar la condición '{condicion.nombre_condicion}'";
                        if (response?.errores != null && response.errores.Any())
                        {
                            var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                            mensaje += $". Detalles: {erroresDetalle}";
                        }

                        await ShowAlertAsync("Error", mensaje);
                        Debug.WriteLine($"Error al asignar condición: {mensaje}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Excepción en AsignarCondicionIndividual: {ex.Message}");
                    await ShowAlertAsync("Error", $"Error al asignar la condición '{condicion.nombre_condicion}'");
                }
            });
        }

        [RelayCommand]
        private async Task CrearNuevaCondicion()
        {
            if (string.IsNullOrWhiteSpace(NombreNuevaCondicion))
            {
                await ShowAlertAsync("Error", "El nombre de la condición es requerido");
                return;
            }

            await ExecuteAsync(async () =>
            {
                try
                {
                    Debug.WriteLine($"=== CREANDO Y ASIGNANDO NUEVA CONDICIÓN: {NombreNuevaCondicion} ===");

                    var request = new ReqInsertarCondicion
                    {
                        NombreCondicion = NombreNuevaCondicion.Trim(),
                        Descripcion = DescripcionNuevaCondicion?.Trim()
                    };

                    var response = await _apiService.InsertarCondicionAsync(request);

                    if (response != null && response.resultado)
                    {
                        // Crear la condición con el ID devuelto por el API
                        var nuevaCondicion = new CondicionesMedicas
                        {
                            id_condicion = response.IdCondicion,
                            nombre_condicion = NombreNuevaCondicion.Trim(),
                            descripcion = DescripcionNuevaCondicion?.Trim()
                        };

                        Debug.WriteLine($"Condición creada con ID: {response.IdCondicion}");

                        // Asignar automáticamente al usuario
                        bool asignacionExitosa = await AsignarCondicionAlUsuario(nuevaCondicion);

                        if (asignacionExitosa)
                        {
                            await ShowAlertAsync("Éxito", "Condición médica creada y asignada correctamente");
                        }
                        else
                        {
                            await ShowAlertAsync("Advertencia", "Condición creada pero no pudo ser asignada automáticamente");
                        }

                        // Limpiar formulario
                        NombreNuevaCondicion = string.Empty;
                        DescripcionNuevaCondicion = string.Empty;
                        MostrarFormularioNuevaCondicion = false;

                        // Recargar condiciones disponibles para incluir la nueva condición
                        await CargarCondicionesDisponiblesAsync();

                        // También recargar las condiciones del usuario para mostrar la nueva asignación
                        await CargarCondicionesUsuarioAsync();

                        Debug.WriteLine("=== CONDICIÓN CREADA Y ASIGNADA EXITOSAMENTE ===");
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
                    Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                    await ShowAlertAsync("Error", "Error al crear la condición médica");
                }
            });
        }

        private async Task<bool> AsignarCondicionAlUsuario(CondicionesMedicas condicion)
        {
            try
            {
                // Verificar si ya está asignada
                if (CondicionesUsuario.Any(c => c.id_condicion == condicion.id_condicion))
                {
                    Debug.WriteLine($"Condición {condicion.nombre_condicion} ya está asignada");
                    return true;
                }

                var request = new ReqAsignarCondicionUsuario
                {
                    IdUsuario = _userId,
                    IdCondicion = condicion.id_condicion
                };

                var response = await _apiService.AsignarCondicionUsuarioAsync(request);

                if (response != null && response.resultado)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        CondicionesUsuario.Add(condicion);
                    });

                    Debug.WriteLine($"Condición {condicion.nombre_condicion} asignada correctamente");
                    return true;
                }
                else
                {
                    var mensaje = response?.Mensaje ?? $"Error al asignar {condicion.nombre_condicion}";
                    Debug.WriteLine($"Error al asignar condición: {mensaje}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción al asignar condición: {ex.Message}");
                return false;
            }
        }
        
        [RelayCommand]
        private async Task AsignarCondicionesSeleccionadas()
        {
            if (!CondicionesSeleccionadas.Any())
            {
                await ShowAlertAsync("Información", "No hay condiciones seleccionadas para asignar");
                return;
            }

            await ExecuteAsync(async () =>
            {
                var condicionesAsignadas = 0;
                var errores = new List<string>();

                foreach (var condicion in CondicionesSeleccionadas.ToList())
                {
                    try
                    {
                        // Verificar si ya está asignada
                        if (CondicionesUsuario.Any(c => c.id_condicion == condicion.id_condicion))
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
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                CondicionesUsuario.Add(condicion);
                            });

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
                CondicionesSeleccionadas.Clear();

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
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            CondicionesUsuario.Remove(condicion);
                        });

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
            MostrarFormularioNuevaCondicion = !MostrarFormularioNuevaCondicion;

            if (!MostrarFormularioNuevaCondicion)
            {
                // Limpiar formulario al ocultar
                NombreNuevaCondicion = string.Empty;
                DescripcionNuevaCondicion = string.Empty;
            }

            Debug.WriteLine($"=== FORMULARIO NUEVA CONDICIÓN: {(MostrarFormularioNuevaCondicion ? "MOSTRAR" : "OCULTAR")} ===");
        }

        [RelayCommand]
        private void CancelarNuevaCondicion()
        {
            NombreNuevaCondicion = string.Empty;
            DescripcionNuevaCondicion = string.Empty;
            MostrarFormularioNuevaCondicion = false;

            Debug.WriteLine("=== FORMULARIO NUEVA CONDICIÓN CANCELADO ===");
        }

        [RelayCommand]
        private async Task RefrescarDatos()
        {
            await ExecuteAsync(async () =>
            {
                Debug.WriteLine("=== REFRESCANDO DATOS DE CONDICIONES ===");
                await CargarCondicionesDisponiblesAsync();
                await CargarCondicionesUsuarioAsync();

                // Limpiar selecciones
                CondicionesSeleccionadas.Clear();

                Debug.WriteLine("=== REFRESCO COMPLETADO ===");
            });
        }

        // Método para manejar selección múltiple desde la vista
        public void OnCondicionesSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            CondicionesSeleccionadas.Clear();
            foreach (CondicionesMedicas condicion in e.CurrentSelection)
            {
                CondicionesSeleccionadas.Add(condicion);
            }

            Debug.WriteLine($"Condiciones seleccionadas: {CondicionesSeleccionadas.Count}");

            // Notificar cambio en la propiedad computada
            OnPropertyChanged(nameof(TieneCondicionesSeleccionadas));
        }
        // Agregar esta propiedad al ViewModel para mostrar solo condiciones no asignadas
        public ObservableCollection<CondicionesMedicas> CondicionesDisponiblesParaAsignar
        {
            get
            {
                if (CondicionesDisponibles == null || CondicionesUsuario == null)
                    return new ObservableCollection<CondicionesMedicas>();

                var condicionesNoAsignadas = CondicionesDisponibles
                    .Where(disponible => !CondicionesUsuario.Any(usuario => usuario.id_condicion == disponible.id_condicion))
                    .ToList();

                return new ObservableCollection<CondicionesMedicas>(condicionesNoAsignadas);
            }
        }

        // Método para notificar cambios en la propiedad filtrada
        private void NotificarCambiosCondicionesDisponibles()
        {
            OnPropertyChanged(nameof(CondicionesDisponiblesParaAsignar));
            OnPropertyChanged(nameof(TieneCondicionesDisponiblesParaAsignar));
        }

        // Propiedad para verificar si hay condiciones disponibles para asignar
        public bool TieneCondicionesDisponiblesParaAsignar => CondicionesDisponiblesParaAsignar?.Any() == true;
        // Propiedades computed para la UI
        public bool TieneCondicionesUsuario => CondicionesUsuario?.Any() == true;
        public bool TieneCondicionesDisponibles => CondicionesDisponibles?.Any() == true;
        public bool TieneCondicionesSeleccionadas => CondicionesSeleccionadas?.Any() == true;
    }
}