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
    public partial class AlergiasViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly int _userId;

        [ObservableProperty]
        private ObservableCollection<Alergias> alergiasDisponibles;

        [ObservableProperty]
        private ObservableCollection<Alergias> alergiasUsuario;

        [ObservableProperty]
        private ObservableCollection<Alergias> alergiasSeleccionadas;

        [ObservableProperty]
        private Alergias alergiaSeleccionada;

        [ObservableProperty]
        private string nombreNuevaAlergia = string.Empty;

        [ObservableProperty]
        private string descripcionNuevaAlergia = string.Empty;

        [ObservableProperty]
        private bool mostrarFormularioNuevaAlergia = false;

        public AlergiasViewModel(IApiService apiService, int userId)
        {
            _apiService = apiService;
            _userId = userId;
            Title = "Gestión de Alergias";

            // Inicializar colecciones
            alergiasDisponibles = new ObservableCollection<Alergias>();
            alergiasUsuario = new ObservableCollection<Alergias>();
            alergiasSeleccionadas = new ObservableCollection<Alergias>();
        }

        public override async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                await CargarAlergiasDisponiblesAsync();
                await CargarAlergiasUsuarioAsync();
            });
        }

        // CORREGIDO: Nombre del método consistente
        private async Task CargarAlergiasDisponiblesAsync()
        {
            try
            {
                Debug.WriteLine("=== CARGANDO ALERGIAS DISPONIBLES ===");

                var request = new ReqListarAlergias();
                var response = await _apiService.ListarAlergiasAsync(request);

                if (response != null && response.resultado && response.Alergias != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        AlergiasDisponibles.Clear();
                        foreach (var alergia in response.Alergias)
                        {
                            AlergiasDisponibles.Add(alergia);
                        }

                        // Notificar cambios en las alergias filtradas
                        NotificarCambiosAlergiasDisponibles();
                    });

                    Debug.WriteLine($"Alergias disponibles cargadas: {AlergiasDisponibles.Count}");
                }
                else
                {
                    var mensaje = response?.Mensaje ?? "Error al cargar alergias disponibles";
                    Debug.WriteLine($"Error: {mensaje}");
                    ErrorMessage = mensaje;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción en CargarAlergiasDisponiblesAsync: {ex.Message}");
                ErrorMessage = "Error al cargar alergias disponibles";
                await HandleErrorAsync(ex);
            }
        }

        // CORREGIDO: Nombre del método consistente
        private async Task CargarAlergiasUsuarioAsync()
        {
            try
            {
                Debug.WriteLine("=== CARGANDO ALERGIAS DEL USUARIO ===");

                var request = new ReqObtenerAlergiasUsuario
                {
                    IdUsuario = _userId
                };

                var response = await _apiService.ObtenerAlergiasUsuarioAsync(request);

                if (response != null && response.resultado)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        AlergiasUsuario.Clear();
                        if (response.Alergias != null)
                        {
                            foreach (var alergia in response.Alergias)
                            {
                                AlergiasUsuario.Add(alergia);
                            }
                        }

                        // Notificar cambios en las alergias filtradas
                        NotificarCambiosAlergiasDisponibles();
                    });

                    Debug.WriteLine($"Alergias del usuario cargadas: {AlergiasUsuario.Count}");
                }
                else
                {
                    var mensaje = response?.Mensaje ?? "Error al cargar alergias del usuario";
                    Debug.WriteLine($"Error: {mensaje}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción en CargarAlergiasUsuarioAsync: {ex.Message}");
                ErrorMessage = "Error al cargar alergias del usuario";
                await HandleErrorAsync(ex);
            }
        }

        [RelayCommand]
        private async Task AsignarAlergiaIndividual(Alergias alergia)
        {
            if (alergia == null) return;

            // Verificar si ya está asignada
            if (AlergiasUsuario.Any(a => a.id_alergia == alergia.id_alergia))
            {
                await ShowAlertAsync("Información", $"La alergia '{alergia.nombre_alergia}' ya está asignada");
                return;
            }

            await ExecuteAsync(async () =>
            {
                try
                {
                    Debug.WriteLine($"=== ASIGNANDO ALERGIA INDIVIDUAL: {alergia.nombre_alergia} ===");

                    var request = new ReqAsignarAlergiaUsuario
                    {
                        IdUsuario = _userId,
                        IdAlergia = alergia.id_alergia
                    };

                    var response = await _apiService.AsignarAlergiaUsuarioAsync(request);

                    if (response != null && response.resultado)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            AlergiasUsuario.Add(alergia);
                            // Notificar cambios para actualizar la lista filtrada
                            NotificarCambiosAlergiasDisponibles();
                        });

                        await ShowAlertAsync("Éxito", $"Alergia '{alergia.nombre_alergia}' asignada correctamente");

                        Debug.WriteLine($"=== ALERGIA {alergia.nombre_alergia} ASIGNADA EXITOSAMENTE ===");
                    }
                    else
                    {
                        var mensaje = response?.Mensaje ?? $"Error al asignar la alergia '{alergia.nombre_alergia}'";
                        if (response?.errores != null && response.errores.Any())
                        {
                            var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                            mensaje += $". Detalles: {erroresDetalle}";
                        }

                        await ShowAlertAsync("Error", mensaje);
                        Debug.WriteLine($"Error al asignar alergia: {mensaje}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Excepción en AsignarAlergiaIndividual: {ex.Message}");
                    await ShowAlertAsync("Error", $"Error al asignar la alergia '{alergia.nombre_alergia}'");
                }
            });
        }

        [RelayCommand]
        private async Task CrearNuevaAlergia()
        {
            if (string.IsNullOrWhiteSpace(NombreNuevaAlergia))
            {
                await ShowAlertAsync("Error", "El nombre de la alergia es requerido");
                return;
            }

            await ExecuteAsync(async () =>
            {
                try
                {
                    Debug.WriteLine($"=== CREANDO Y ASIGNANDO NUEVA ALERGIA: {NombreNuevaAlergia} ===");

                    var request = new ReqInsertarAlergia
                    {
                        NombreAlergia = NombreNuevaAlergia.Trim(),
                        Descripcion = DescripcionNuevaAlergia?.Trim()
                    };

                    var response = await _apiService.InsertarAlergiaAsync(request);

                    if (response != null && response.resultado)
                    {
                        // Crear la alergia con el ID devuelto por el API
                        var nuevaAlergia = new Alergias
                        {
                            id_alergia = response.IdAlergia,
                            nombre_alergia = NombreNuevaAlergia.Trim(),
                            descripcion = DescripcionNuevaAlergia?.Trim()
                        };

                        Debug.WriteLine($"Alergia creada con ID: {response.IdAlergia}");

                        // Asignar automáticamente al usuario
                        bool asignacionExitosa = await AsignarAlergiaAlUsuario(nuevaAlergia);

                        if (asignacionExitosa)
                        {
                            await ShowAlertAsync("Éxito", "Alergia creada y asignada correctamente");
                        }
                        else
                        {
                            await ShowAlertAsync("Advertencia", "Alergia creada pero no pudo ser asignada automáticamente");
                        }

                        // Limpiar formulario
                        NombreNuevaAlergia = string.Empty;
                        DescripcionNuevaAlergia = string.Empty;
                        MostrarFormularioNuevaAlergia = false;

                        // Recargar alergias disponibles para incluir la nueva alergia
                        await CargarAlergiasDisponiblesAsync();

                        // También recargar las alergias del usuario para mostrar la nueva asignación
                        await CargarAlergiasUsuarioAsync();

                        Debug.WriteLine("=== ALERGIA CREADA Y ASIGNADA EXITOSAMENTE ===");
                    }
                    else
                    {
                        var mensaje = response?.Mensaje ?? "Error al crear la alergia";
                        if (response?.errores != null && response.errores.Any())
                        {
                            var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                            mensaje += $". Detalles: {erroresDetalle}";
                        }

                        await ShowAlertAsync("Error", mensaje);
                        Debug.WriteLine($"Error al crear alergia: {mensaje}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Excepción en CrearNuevaAlergia: {ex.Message}");
                    Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                    await ShowAlertAsync("Error", "Error al crear la alergia");
                }
            });
        }

        private async Task<bool> AsignarAlergiaAlUsuario(Alergias alergia)
        {
            try
            {
                // Verificar si ya está asignada
                if (AlergiasUsuario.Any(a => a.id_alergia == alergia.id_alergia))
                {
                    Debug.WriteLine($"Alergia {alergia.nombre_alergia} ya está asignada");
                    return true;
                }

                var request = new ReqAsignarAlergiaUsuario
                {
                    IdUsuario = _userId,
                    IdAlergia = alergia.id_alergia
                };

                var response = await _apiService.AsignarAlergiaUsuarioAsync(request);

                if (response != null && response.resultado)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        AlergiasUsuario.Add(alergia);
                    });

                    Debug.WriteLine($"Alergia {alergia.nombre_alergia} asignada correctamente");
                    return true;
                }
                else
                {
                    var mensaje = response?.Mensaje ?? $"Error al asignar {alergia.nombre_alergia}";
                    Debug.WriteLine($"Error al asignar alergia: {mensaje}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción al asignar alergia: {ex.Message}");
                return false;
            }
        }

        // CORREGIDO: Nombre del comando consistente
        [RelayCommand]
        private async Task AsignarAlergiasSeleccionadas()
        {
            if (!AlergiasSeleccionadas.Any())
            {
                await ShowAlertAsync("Información", "No hay alergias seleccionadas para asignar");
                return;
            }

            await ExecuteAsync(async () =>
            {
                var alergiasAsignadas = 0;
                var errores = new List<string>();

                foreach (var alergia in AlergiasSeleccionadas.ToList())
                {
                    try
                    {
                        // Verificar si ya está asignada
                        if (AlergiasUsuario.Any(a => a.id_alergia == alergia.id_alergia))
                        {
                            Debug.WriteLine($"Alergia {alergia.nombre_alergia} ya está asignada");
                            continue;
                        }

                        var request = new ReqAsignarAlergiaUsuario
                        {
                            IdUsuario = _userId,
                            IdAlergia = alergia.id_alergia
                        };

                        var response = await _apiService.AsignarAlergiaUsuarioAsync(request);

                        if (response != null && response.resultado)
                        {
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                AlergiasUsuario.Add(alergia);
                            });

                            alergiasAsignadas++;
                            Debug.WriteLine($"Alergia {alergia.nombre_alergia} asignada correctamente");
                        }
                        else
                        {
                            var mensaje = response?.Mensaje ?? $"Error al asignar {alergia.nombre_alergia}";
                            errores.Add(mensaje);
                            Debug.WriteLine($"Error al asignar alergia: {mensaje}");
                        }
                    }
                    catch (Exception ex)
                    {
                        var mensaje = $"Error al asignar {alergia.nombre_alergia}: {ex.Message}";
                        errores.Add(mensaje);
                        Debug.WriteLine($"Excepción: {mensaje}");
                    }
                }

                // Limpiar selección
                AlergiasSeleccionadas.Clear();

                // Mostrar resultado
                var mensajeResultado = $"Alergias asignadas: {alergiasAsignadas}";
                if (errores.Any())
                {
                    mensajeResultado += $"\nErrores: {string.Join(", ", errores)}";
                }

                await ShowAlertAsync(errores.Any() ? "Completado con errores" : "Éxito", mensajeResultado);

                Debug.WriteLine($"=== ASIGNACIÓN COMPLETADA: {alergiasAsignadas} exitosas, {errores.Count} errores ===");
            });
        }

        [RelayCommand]
        private async Task DesasignarAlergia(Alergias alergia)
        {
            if (alergia == null) return;

            var confirmar = await ShowConfirmAsync(
                "Confirmar",
                $"¿Estás seguro que deseas desasignar la alergia '{alergia.nombre_alergia}'?",
                "Desasignar",
                "Cancelar");

            if (!confirmar) return;

            await ExecuteAsync(async () =>
            {
                try
                {
                    Debug.WriteLine($"=== DESASIGNANDO ALERGIA: {alergia.nombre_alergia} ===");

                    var request = new ReqDesasignarAlergiaUsuario
                    {
                        IdUsuario = _userId,
                        IdAlergia = alergia.id_alergia
                    };

                    var response = await _apiService.DesasignarAlergiaUsuarioAsync(request);

                    if (response != null && response.resultado)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            AlergiasUsuario.Remove(alergia);
                        });

                        await ShowAlertAsync("Éxito", "Alergia desasignada correctamente");

                        Debug.WriteLine("=== ALERGIA DESASIGNADA EXITOSAMENTE ===");
                    }
                    else
                    {
                        var mensaje = response?.Mensaje ?? "Error al desasignar la alergia";
                        if (response?.errores != null && response.errores.Any())
                        {
                            var erroresDetalle = string.Join(", ", response.errores.Select(e => e.mensaje));
                            mensaje += $". Detalles: {erroresDetalle}";
                        }

                        await ShowAlertAsync("Error", mensaje);
                        Debug.WriteLine($"Error al desasignar alergia: {mensaje}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Excepción en DesasignarAlergia: {ex.Message}");
                    await ShowAlertAsync("Error", "Error al desasignar la alergia");
                }
            });
        }

        // CORREGIDO: Nombre del comando y formato consistente
        [RelayCommand]
        private void ToggleMostrarFormularioNuevaAlergia()
        {
            MostrarFormularioNuevaAlergia = !MostrarFormularioNuevaAlergia;

            if (!MostrarFormularioNuevaAlergia)
            {
                // Limpiar formulario al ocultar
                NombreNuevaAlergia = string.Empty;
                DescripcionNuevaAlergia = string.Empty;
            }

            Debug.WriteLine($"=== FORMULARIO NUEVA ALERGIA: {(MostrarFormularioNuevaAlergia ? "MOSTRAR" : "OCULTAR")} ===");
        }

        [RelayCommand]
        private void CancelarNuevaAlergia()
        {
            NombreNuevaAlergia = string.Empty;
            DescripcionNuevaAlergia = string.Empty;
            MostrarFormularioNuevaAlergia = false;

            Debug.WriteLine("=== FORMULARIO NUEVA ALERGIA CANCELADO ===");
        }

        [RelayCommand]
        private async Task RefrescarDatos()
        {
            await ExecuteAsync(async () =>
            {
                Debug.WriteLine("=== REFRESCANDO DATOS DE ALERGIAS ===");
                await CargarAlergiasDisponiblesAsync();
                await CargarAlergiasUsuarioAsync();

                // Limpiar selecciones
                AlergiasSeleccionadas.Clear();

                Debug.WriteLine("=== REFRESCO COMPLETADO ===");
            });
        }

        // Método para manejar selección múltiple desde la vista
        public void OnAlergiasSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            AlergiasSeleccionadas.Clear();
            foreach (Alergias alergia in e.CurrentSelection)
            {
                AlergiasSeleccionadas.Add(alergia);
            }

            Debug.WriteLine($"Alergias seleccionadas: {AlergiasSeleccionadas.Count}");

            // Notificar cambio en la propiedad computada
            OnPropertyChanged(nameof(TieneAlergiasSeleccionadas));
        }

        // CORREGIDO: Nombres de propiedades consistentes con PascalCase
        public ObservableCollection<Alergias> AlergiasDisponiblesParaAsignar
        {
            get
            {
                if (AlergiasDisponibles == null || AlergiasUsuario == null)
                    return new ObservableCollection<Alergias>();

                var alergiasNoAsignadas = AlergiasDisponibles
                    .Where(disponible => !AlergiasUsuario.Any(usuario => usuario.id_alergia == disponible.id_alergia))
                    .ToList();

                return new ObservableCollection<Alergias>(alergiasNoAsignadas);
            }
        }

        // CORREGIDO: Nombre del método consistente
        private void NotificarCambiosAlergiasDisponibles()
        {
            OnPropertyChanged(nameof(AlergiasDisponiblesParaAsignar));
            OnPropertyChanged(nameof(TieneAlergiasDisponiblesParaAsignar));
        }

        // CORREGIDO: Nombres de propiedades consistentes
        public bool TieneAlergiasDisponiblesParaAsignar => AlergiasDisponiblesParaAsignar?.Any() == true;
        public bool TieneAlergiasUsuario => AlergiasUsuario?.Any() == true;
        public bool TieneAlergiasDisponibles => AlergiasDisponibles?.Any() == true;
        public bool TieneAlergiasSeleccionadas => AlergiasSeleccionadas?.Any() == true;
    }
}