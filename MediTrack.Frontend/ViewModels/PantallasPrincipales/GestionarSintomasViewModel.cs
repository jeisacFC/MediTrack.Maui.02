// MediTrack.Frontend/ViewModels/PantallasPrincipales/GestionarSintomasViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.Popups;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq; // Asegúrate de tener este using

namespace MediTrack.Frontend.ViewModels;

public partial class GestionarSintomasViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private Popup _popup;
    private CancellationTokenSource _filtroTimer;
    private readonly SemaphoreSlim _cargaSemaphore = new(1, 1);

    [ObservableProperty]
    private ObservableCollection<SintomaSeleccionable> sintomasDisponibles = new();
    [ObservableProperty]
    private ObservableCollection<SintomaSeleccionable> sintomasFiltrados = new();
    [ObservableProperty]
    private ObservableCollection<SintomaSeleccionable> sintomasSeleccionados = new();
    [ObservableProperty]
    private string filtroBusqueda = "";
    [ObservableProperty]
    private string nuevoSintomaTexto = "";
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private bool haySintomasDisponibles = false;
    [ObservableProperty]
    private bool haySintomasSeleccionados = false;
    [ObservableProperty]
    private bool puedeAgregarSintomaManual = false;

    // Cache para evitar recargas innecesarias
    private List<SintomaSeleccionable> _sintomasCache = new();
    private bool _sintomasYaCargados = false; // Bandera para saber si ya se intentó cargar del backend

    public GestionarSintomasViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    public void SetPopupReference(Popup popup)
    {
        _popup = popup;
    }

    public async Task CargarSintomasDisponibles()
    {
        // Evitar múltiples cargas simultáneas
        if (!await _cargaSemaphore.WaitAsync(100))
        {
            Debug.WriteLine("[VM Síntomas] Ya hay una carga en progreso, ignorando...");
            return;
        }

        try
        {
            IsLoading = true;
            Debug.WriteLine("[VM Síntomas] Iniciando carga de síntomas...");

            // Si ya se cargaron los síntomas del backend (y el caché no está vacío), usarlos.
            // Si el caché está vacío, siempre intentar de nuevo del backend.
            if (_sintomasYaCargados && _sintomasCache.Any())
            {
                Debug.WriteLine("[VM Síntomas] Usando caché de síntomas para esta sesión.");
                await CargarDesdeCache(); // Asegura que las selecciones de usuario se reflejen
                return;
            }

            // Primero, intentar obtener el ID de usuario para las llamadas a la API
            var userIdStr = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdStr, out var userId))
            {
                Debug.WriteLine("[VM Síntomas] No se pudo obtener el ID de usuario. Recurriendo a emergencia.");
                await MainThread.InvokeOnMainThreadAsync(CargarSintomasEmergencia);
                return;
            }

            // 1. Obtener todos los síntomas del ENUM desde el backend (lista maestra)
            ResObtenerSintomasEnum todosLosSintomasResponse = null;
            try
            {
                todosLosSintomasResponse = await _apiService.ObtenerTodosLosSintomasAsync();
                Debug.WriteLine($"[VM Síntomas] Respuesta API (ObtenerTodosLosSintomasEnum): resultado={todosLosSintomasResponse?.resultado}, count={todosLosSintomasResponse?.Sintomas?.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM Síntomas] Error al obtener síntomas del enum: {ex.Message}. Continuará con síntomas del usuario y manuales.");
            }

            // 2. Obtener síntomas reportados por el usuario (incluye manuales)
            ResObtenerSintomasUsuario sintomasUsuarioResponse = null;
            try
            {
                sintomasUsuarioResponse = await _apiService.ObtenerSintomasUsuarioAsync(new ReqObtenerUsuario { IdUsuario = userId });
                Debug.WriteLine($"[VM Síntomas] Respuesta API (ObtenerSintomasUsuario): resultado={sintomasUsuarioResponse?.resultado}, count={sintomasUsuarioResponse?.Sintomas?.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM Síntomas] Error al obtener síntomas del usuario: {ex.Message}. Ignorando síntomas reportados.");
            }

            // Consolidar y procesar los síntomas en un Task.Run para no bloquear
            var consolidatedSintomas = await Task.Run(() =>
            {
                var tempSintomas = new Dictionary<string, SintomaSeleccionable>(); // Usar nombre como clave para unicidad
                var initialSelected = new List<SintomaSeleccionable>();

                // A. Añadir síntomas del ENUM
                if (todosLosSintomasResponse?.resultado == true && todosLosSintomasResponse.Sintomas?.Any() == true)
                {
                    foreach (var sintomaEnum in todosLosSintomasResponse.Sintomas)
                    {
                        var sintoma = new SintomaSeleccionable
                        {
                            Id = sintomaEnum.IdSintoma,
                            Nombre = sintomaEnum.Nombre,
                            EsManual = false,
                            EstaSeleccionado = false,
                            MostrarTipo = false
                        };
                        tempSintomas[sintoma.Nombre.ToLowerInvariant()] = sintoma; // Usar lowerInvariant para asegurar unicidad
                    }
                }

                // B. Añadir síntomas del usuario (marcándolos y agregando manuales)
                if (sintomasUsuarioResponse?.resultado == true && sintomasUsuarioResponse.Sintomas?.Any() == true)
                {
                    foreach (var sintomaReportado in sintomasUsuarioResponse.Sintomas)
                    {
                        var nombreLower = sintomaReportado.Sintoma.ToLowerInvariant();
                        if (tempSintomas.TryGetValue(nombreLower, out var sintomaExistente))
                        {
                            // Si el síntoma ya está en el enum, solo marcarlo como seleccionado
                            sintomaExistente.EstaSeleccionado = true;
                            initialSelected.Add(sintomaExistente);
                        }
                        else
                        {
                            // Si es un síntoma manual y no está en el enum, agregarlo
                            var sintomaManual = new SintomaSeleccionable
                            {
                                Id = 0, // ID 0 o un valor temporal para manuales si el backend no lo devuelve
                                Nombre = sintomaReportado.Sintoma,
                                EsManual = true, // Asumir que si no está en el enum es manual
                                EstaSeleccionado = true,
                                MostrarTipo = true // Mostrar "Manual" en UI
                            };
                            tempSintomas[nombreLower] = sintomaManual;
                            initialSelected.Add(sintomaManual);
                        }
                    }
                }

                // Convertir a lista y ordenar
                var finalSintomas = tempSintomas.Values.OrderBy(s => s.Nombre).ToList();

                return new
                {
                    Success = true,
                    Sintomas = finalSintomas,
                    InitialSelected = initialSelected,
                    Message = "Carga combinada exitosa."
                };
            }).ConfigureAwait(false);

            // Actualizar UI en hilo principal
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SintomasDisponibles.Clear();
                SintomasFiltrados.Clear();
                SintomasSeleccionados.Clear();

                if (consolidatedSintomas.Success && consolidatedSintomas.Sintomas.Any())
                {
                    // Llenar _sintomasCache con todos los síntomas (enum + manuales)
                    _sintomasCache.AddRange(consolidatedSintomas.Sintomas);
                    _sintomasYaCargados = true;

                    // Asignar síntomas seleccionados
                    foreach (var s in consolidatedSintomas.InitialSelected)
                    {
                        SintomasSeleccionados.Add(s);
                    }

                    // Llenar SintomasDisponibles (todos) y SintomasFiltrados (solo no seleccionados)
                    foreach (var sintoma in _sintomasCache.OrderBy(s => s.Nombre))
                    {
                        SintomasDisponibles.Add(sintoma); // Todos los síntomas conocidos
                        if (!sintoma.EstaSeleccionado)
                        {
                            SintomasFiltrados.Add(sintoma);
                        }
                    }

                    Debug.WriteLine($"[VM Síntomas] ÉXITO: {SintomasDisponibles.Count} síntomas totales cargados. {SintomasSeleccionados.Count} seleccionados.");
                }
                else
                {
                    // Si no se pudo cargar nada del backend, usar síntomas de emergencia
                    Debug.WriteLine("[VM Síntomas] Carga de backend fallida o sin datos. Recurriendo a síntomas de emergencia.");
                    CargarSintomasEmergencia();
                }

                ActualizarEstados();
                EjecutarFiltro(FiltroBusqueda); // Re-aplicar filtro si ya había uno
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] ERROR CRÍTICO durante la carga principal: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CargarSintomasEmergencia(); // Fallback de emergencia en caso de error no manejado
                ActualizarEstados();
            });
        }
        finally
        {
            IsLoading = false;
            _cargaSemaphore.Release();
        }
    }

    // Este método alternativo solo debería existir si tienes un endpoint de búsqueda que funciona de manera diferente
    // y quieres consolidar resultados de múltiples llamadas. En tu caso, ya tienes ObtenerTodosLosSintomasEnum,
    // por lo que este método alternativo podría ser redundante o indicar un problema de diseño en el backend.
    // Lo mantengo por si es una lógica específica, pero lo ideal es que ObtenerTodosLosSintomasAsync sea suficiente.
    private async Task<dynamic> CargarConMetodoAlternativo()
    {
        try
        {
            var filtros = new[] { "a", "e", "i", "o", "u" };
            var sintomasAlternativos = new List<ResBuscarSintoma>();
            var sintomasUnicos = new HashSet<int>();

            foreach (var filtro in filtros)
            {
                try
                {
                    var request = new ReqBuscarSintoma { Filtro = filtro };
                    var response = await _apiService.BuscarSintomasAsync(request).ConfigureAwait(false);

                    if (response?.Any() == true && !response.Any(s => s.Nombre.Contains("Debe ingresar un texto")))
                    {
                        foreach (var sintoma in response.Where(s => !sintomasUnicos.Contains(s.IdSintoma)))
                        {
                            sintomasUnicos.Add(sintoma.IdSintoma);
                            sintomasAlternativos.Add(sintoma);
                        }
                    }
                    await Task.Delay(50).ConfigureAwait(false); // Pausa breve para no saturar la API
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[VM Síntomas] Error con filtro '{filtro}' en método alternativo: {ex.Message}");
                }
            }

            if (sintomasAlternativos.Any())
            {
                var sintomasOrdenados = sintomasAlternativos
                    .OrderBy(s => s.Nombre)
                    .Select(sintoma => new SintomaSeleccionable
                    {
                        Id = sintoma.IdSintoma,
                        Nombre = sintoma.Nombre,
                        EsManual = false,
                        EstaSeleccionado = false,
                        MostrarTipo = false
                    })
                    .ToList();

                return new { Success = true, Sintomas = sintomasOrdenados, Message = "Carga alternativa por filtros exitosa." };
            }

            return new { Success = false, Sintomas = new List<SintomaSeleccionable>(), Message = "Carga alternativa fallida." };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] Error en método alternativo: {ex.Message}");
            return new { Success = false, Sintomas = new List<SintomaSeleccionable>(), Message = $"Error en carga alternativa: {ex.Message}" };
        }
    }

    private async Task CargarDesdeCache()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            SintomasDisponibles.Clear();
            SintomasFiltrados.Clear();
            SintomasSeleccionados.Clear(); // Limpiar también los seleccionados para reconstruirlos

            // Re-evaluar el estado de selección de los síntomas desde el caché
            foreach (var sintoma in _sintomasCache.OrderBy(s => s.Nombre))
            {
                // Asegurarse de que el estado de selección esté actualizado si hubo cambios previos
                // La idea es que al cargar desde caché, si un síntoma fue deseleccionado,
                // no vuelva a aparecer como seleccionado.
                // Aquí podrías recargar los síntomas del usuario de nuevo si quieres asegurar el estado más reciente,
                // pero si el caché se actualiza correctamente con cada toggle, no es necesario.
                SintomasDisponibles.Add(sintoma);
                if (sintoma.EstaSeleccionado)
                {
                    SintomasSeleccionados.Add(sintoma);
                }
                else
                {
                    SintomasFiltrados.Add(sintoma);
                }
            }

            ActualizarEstados();
            EjecutarFiltro(FiltroBusqueda); // Re-aplicar el filtro actual
            Debug.WriteLine($"[VM Síntomas] Cargado desde cache: {SintomasDisponibles.Count} síntomas totales");
        });
    }

    // Este método solo debe ser un último recurso si NINGUNA llamada al backend funciona
    private void CargarSintomasEmergencia()
    {
        var sintomasEmergencia = new List<SintomaSeleccionable>
        {
            new() { Id = 1, Nombre = "Dolor de cabeza", EsManual = false, EstaSeleccionado = false },
            new() { Id = 2, Nombre = "Fiebre", EsManual = false, EstaSeleccionado = false },
            new() { Id = 3, Nombre = "Náuseas", EsManual = false, EstaSeleccionado = false },
            new() { Id = 4, Nombre = "Mareos", EsManual = false, EstaSeleccionado = false },
            new() { Id = 5, Nombre = "Fatiga", EsManual = false, EstaSeleccionado = false },
            new() { Id = 6, Nombre = "Dolor de garganta", EsManual = false, EstaSeleccionado = false },
            new() { Id = 7, Nombre = "Tos", EsManual = false, EstaSeleccionado = false },
            new() { Id = 8, Nombre = "Dolor muscular", EsManual = false, EstaSeleccionado = false },
        };

        SintomasDisponibles.Clear();
        SintomasFiltrados.Clear();
        SintomasSeleccionados.Clear(); // Asegurarse de que no haya nada seleccionado de cargas anteriores

        foreach (var sintoma in sintomasEmergencia)
        {
            SintomasDisponibles.Add(sintoma);
            SintomasFiltrados.Add(sintoma);
        }
        _sintomasCache = sintomasEmergencia.ToList(); // También actualizar el caché de emergencia
        _sintomasYaCargados = true;

        Debug.WriteLine($"[VM Síntomas] RECOVERY: {SintomasDisponibles.Count} síntomas de emergencia cargados.");
    }

    public void FiltrarSintomas(string filtro)
    {
        // Cancelar filtrado anterior
        _filtroTimer?.Cancel();
        _filtroTimer = new CancellationTokenSource();

        var token = _filtroTimer.Token;

        // Debounce: esperar 300ms antes de ejecutar filtro
        Task.Delay(300, token).ContinueWith(async _ =>
        {
            if (!token.IsCancellationRequested)
            {
                await MainThread.InvokeOnMainThreadAsync(() => EjecutarFiltro(filtro));
            }
        }, TaskScheduler.Default);
    }

    private void EjecutarFiltro(string filtro)
    {
        try
        {
            Debug.WriteLine($"[VM Síntomas] Ejecutando filtro con: '{filtro}'");
            FiltroBusqueda = filtro ?? "";

            // Filtrar desde el caché completo, excluyendo los ya seleccionados
            var sintomasParaMostrar = _sintomasCache
                .Where(s => !s.EstaSeleccionado &&
                           (string.IsNullOrWhiteSpace(FiltroBusqueda) ||
                            s.Nombre.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(s => s.Nombre)
                .ToList();

            SintomasFiltrados.Clear();
            foreach (var sintoma in sintomasParaMostrar)
            {
                SintomasFiltrados.Add(sintoma);
            }

            Debug.WriteLine($"[VM Síntomas] Después de filtro: {SintomasFiltrados.Count} síntomas disponibles");

            // Permitir agregar síntoma manual si el filtro no está vacío Y no hay resultados en los filtrados actuales.
            PuedeAgregarSintomaManual = !string.IsNullOrWhiteSpace(FiltroBusqueda) && !SintomasFiltrados.Any();
            ActualizarEstados();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] Error filtrando síntomas: {ex.Message}");
        }
    }

    [RelayCommand]
    public void ToggleSintomaSeleccion(SintomaSeleccionable sintoma)
    {
        try
        {
            if (sintoma == null)
            {
                Debug.WriteLine("[VM Síntomas] ERROR: sintoma es null en ToggleSintomaSeleccion");
                return;
            }

            Debug.WriteLine($"[VM Síntomas] ToggleSintomaSeleccion INICIO: {sintoma.Nombre} (actual: {sintoma.EstaSeleccionado})");

            // Cambiar el estado del síntoma
            sintoma.EstaSeleccionado = !sintoma.EstaSeleccionado;

            Debug.WriteLine($"[VM Síntomas] Después del toggle: {sintoma.Nombre} -> {sintoma.EstaSeleccionado}");

            if (sintoma.EstaSeleccionado)
            {
                Debug.WriteLine($"[VM Síntomas] Intentando agregar a SintomasSeleccionados...");

                // Verificar si ya está presente
                var yaExiste = SintomasSeleccionados.Any(s => s.Id == sintoma.Id && s.Nombre == sintoma.Nombre);
                Debug.WriteLine($"[VM Síntomas] ¿Ya existe en seleccionados? {yaExiste}");

                if (!yaExiste)
                {
                    SintomasSeleccionados.Add(sintoma);
                    Debug.WriteLine($"[VM Síntomas] AGREGADO: {sintoma.Nombre}. Total seleccionados: {SintomasSeleccionados.Count}");
                }

                // Remover de filtrados
                var sintomaEnFiltrados = SintomasFiltrados.FirstOrDefault(s => s.Id == sintoma.Id && s.Nombre == sintoma.Nombre);
                if (sintomaEnFiltrados != null)
                {
                    SintomasFiltrados.Remove(sintomaEnFiltrados);
                    Debug.WriteLine($"[VM Síntomas] Removido de filtrados");
                }
            }
            else
            {
                Debug.WriteLine($"[VM Síntomas] Intentando remover de SintomasSeleccionados...");

                var sintomaEnSeleccionados = SintomasSeleccionados.FirstOrDefault(s => s.Id == sintoma.Id && s.Nombre == sintoma.Nombre);
                if (sintomaEnSeleccionados != null)
                {
                    SintomasSeleccionados.Remove(sintomaEnSeleccionados);
                    Debug.WriteLine($"[VM Síntomas] REMOVIDO: {sintoma.Nombre}. Total seleccionados: {SintomasSeleccionados.Count}");
                }
            }

            // Actualizar el caché
            var cachedSintoma = _sintomasCache.FirstOrDefault(s => s.Id == sintoma.Id && s.Nombre == sintoma.Nombre);
            if (cachedSintoma != null)
            {
                cachedSintoma.EstaSeleccionado = sintoma.EstaSeleccionado;
                Debug.WriteLine($"[VM Síntomas] Cache actualizado para {sintoma.Nombre}");
            }

            ActualizarEstados();
            Debug.WriteLine($"[VM Síntomas] ToggleSintomaSeleccion TERMINADO");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] EXCEPCIÓN en toggle: {ex.Message}");
            Debug.WriteLine($"[VM Síntomas] StackTrace: {ex.StackTrace}");
        }
    }

    [RelayCommand]
    private void RemoverSintomaSeleccionado(SintomaSeleccionable sintoma)
    {
        try
        {
            if (sintoma == null) return;

            Debug.WriteLine($"[VM Síntomas] Removiendo: {sintoma.Nombre}");
            sintoma.EstaSeleccionado = false;
            SintomasSeleccionados.Remove(sintoma);

            // También actualizar el estado en el caché global
            var cachedSintoma = _sintomasCache.FirstOrDefault(s => s.Id == sintoma.Id && s.Nombre == sintoma.Nombre);
            if (cachedSintoma != null)
            {
                cachedSintoma.EstaSeleccionado = false;
            }

            // Re-añadir a la lista de filtrados si coincide con el filtro actual
            if (string.IsNullOrWhiteSpace(FiltroBusqueda) ||
                sintoma.Nombre.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase))
            {
                if (!SintomasFiltrados.Any(s => s.Id == sintoma.Id && s.Nombre == sintoma.Nombre))
                {
                    SintomasFiltrados.Add(sintoma);
                    // Re-ordenar
                    var sorted = SintomasFiltrados.OrderBy(s => s.Nombre).ToList();
                    SintomasFiltrados.Clear();
                    foreach (var s in sorted) { SintomasFiltrados.Add(s); }
                }
            }

            ActualizarEstados();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] Error removiendo síntoma: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AgregarSintomaManual()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NuevoSintomaTexto) || NuevoSintomaTexto.Trim().Length < 2)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlert("Error", "El síntoma debe tener al menos 2 caracteres.", "OK"));
                return;
            }

            // Verificar si el síntoma ya existe (ignorando mayúsculas/minúsculas) en el caché global
            if (_sintomasCache.Any(s => s.Nombre.Equals(NuevoSintomaTexto.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlert("Aviso", "Este síntoma ya existe o es similar a uno existente. Por favor, selecciónalo de la lista.", "OK"));
                return;
            }

            Debug.WriteLine($"[VM Síntomas] Agregando síntoma manual: {NuevoSintomaTexto}");
            IsLoading = true;

            var userIdStr = await SecureStorage.GetAsync("user_id").ConfigureAwait(false);
            if (!int.TryParse(userIdStr, out var userId))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlert("Error", "Usuario no autenticado para agregar síntomas.", "OK"));
                return;
            }

            var request = new ReqInsertarSintomaManual
            {
                IdUsuario = userId,
                Descripcion = NuevoSintomaTexto.Trim()
            };

            var response = await _apiService.InsertarSintomaManualAsync(request).ConfigureAwait(false);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (response?.resultado == true)
                {
                    // Asumiendo que el backend inserta y podemos obtener el ID si lo devuelve
                    // Si el backend no devuelve un ID real para el síntoma insertado,
                    // podríamos usar un ID negativo o temporal para distinguirlo.
                    // Para el ejercicio, usamos un ID basado en hash o tiempo para simular unicidad.
                    var newId = -(int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Usar el ID del backend si lo devuelve, sino un negativo.

                    var nuevoSintoma = new SintomaSeleccionable
                    {
                        Id = newId, // Asignar el ID real si el backend lo proporciona
                        Nombre = NuevoSintomaTexto.Trim(),
                        EsManual = true,
                        EstaSeleccionado = true,
                        MostrarTipo = true // Mostrar que es manual
                    };

                    // Añadir al caché global y a la lista de seleccionados
                    _sintomasCache.Add(nuevoSintoma);
                    if (!SintomasSeleccionados.Any(s => s.Id == nuevoSintoma.Id && s.Nombre == nuevoSintoma.Nombre))
                    {
                        SintomasSeleccionados.Add(nuevoSintoma);
                    }

                    NuevoSintomaTexto = "";
                    PuedeAgregarSintomaManual = false;
                    FiltrarSintomas(""); // Re-filtra para limpiar la lista y ocultar el formulario manual si aplica

                    await Shell.Current.DisplayAlert("Éxito", "Síntoma agregado correctamente", "OK");
                    Debug.WriteLine($"[VM Síntomas] Síntoma manual agregado: {nuevoSintoma.Nombre} (ID: {nuevoSintoma.Id})");
                }
                else
                {
                    var mensaje = response?.errores?.FirstOrDefault()?.mensaje ?? "Error desconocido al agregar síntoma manual";
                    await Shell.Current.DisplayAlert("Error", mensaje, "OK");
                    Debug.WriteLine($"[VM Síntomas] Error agregando síntoma manual: {mensaje}");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] Excepción agregando síntoma manual: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayAlert("Error", "No se pudo agregar el síntoma debido a un error de conexión o servidor.", "OK"));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GuardarSintomas()
    {
        Debug.WriteLine("COMANDO GUARDAR SINTOMAS EJECUTADO - INICIO");

        // AGREGAR ESTOS LOGS ANTES DE TODO:
        Debug.WriteLine($"=== VERIFICACION INICIAL ===");
        Debug.WriteLine($"SintomasSeleccionados.Count: {SintomasSeleccionados.Count}");
        Debug.WriteLine($"SintomasDisponibles.Count: {SintomasDisponibles.Count}");
        Debug.WriteLine($"SintomasFiltrados.Count: {SintomasFiltrados.Count}");

        try
        {
            Debug.WriteLine("=== INICIO GUARDAR SINTOMAS ===");
            Debug.WriteLine($"Total sintomas seleccionados: {SintomasSeleccionados.Count}");

            // PASO 1: Analizar que tienes seleccionado
            var sintomasEnum = SintomasSeleccionados.Where(s => !s.EsManual).ToList();
            var sintomasManuales = SintomasSeleccionados.Where(s => s.EsManual).ToList();

            Debug.WriteLine($"Sintomas del ENUM: {sintomasEnum.Count}");
            foreach (var s in sintomasEnum)
                Debug.WriteLine($"  - Enum: {s.Nombre} (ID: {s.Id})");

            Debug.WriteLine($"Sintomas MANUALES: {sintomasManuales.Count}");
            foreach (var s in sintomasManuales)
                Debug.WriteLine($"  - Manual: {s.Nombre} (ID: {s.Id})");

            // PASO 2: Verificar si hay algo que enviar
            if (!sintomasEnum.Any() && !sintomasManuales.Any())
            {
                Debug.WriteLine("ERROR: No hay sintomas para guardar");
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlert("Error", "No hay sintomas seleccionados para guardar.", "OK"));
                return;
            }

            if (!sintomasEnum.Any())
            {
                Debug.WriteLine("ADVERTENCIA: Solo hay sintomas manuales, que probablemente YA estan guardados individualmente");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    bool continuar = await Shell.Current.DisplayAlert(
                        "Aviso",
                        "Solo tienes sintomas manuales seleccionados. Estos ya fueron guardados cuando los agregaste. ¿Quieres cerrar el modal?",
                        "Si, cerrar",
                        "Cancelar");

                    if (continuar)
                    {
                        await CerrarModal();
                    }
                });
                return;
            }

            Debug.WriteLine($"Procediendo a guardar {sintomasEnum.Count} sintomas del enum");
            IsLoading = true;

            // PASO 3: Verificar usuario
            var userIdStr = await SecureStorage.GetAsync("user_id").ConfigureAwait(false);
            Debug.WriteLine($"UserId string obtenido: '{userIdStr}'");

            if (!int.TryParse(userIdStr, out var userId))
            {
                Debug.WriteLine("ERROR: No se pudo parsear el UserId");
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlert("Error", "Usuario no autenticado para guardar sintomas.", "OK"));
                return;
            }

            Debug.WriteLine($"UserId parseado correctamente: {userId}");

            // PASO 4: Preparar request solo con sintomas del enum
            var idsSintomasAEnviar = sintomasEnum.Select(s => s.Id).ToList();

            var request = new ReqAgregarSintomasSeleccionados
            {
                IdUsuario = userId,
                IdsSintomas = idsSintomasAEnviar
            };

            Debug.WriteLine($"REQUEST preparado:");
            Debug.WriteLine($"  - IdUsuario: {request.IdUsuario}");
            Debug.WriteLine($"  - IdsSintomas: [{string.Join(", ", request.IdsSintomas)}]");
            Debug.WriteLine($"  - Total IDs a enviar: {request.IdsSintomas.Count}");

            // PASO 5: Llamar al API
            Debug.WriteLine("Llamando al API...");
            var response = await _apiService.AgregarSintomasSeleccionadosAsync(request).ConfigureAwait(false);

            Debug.WriteLine($"RESPUESTA del API:");
            Debug.WriteLine($"  - resultado: {response?.resultado}");
            Debug.WriteLine($"  - Mensaje: '{response?.Mensaje}'");
            Debug.WriteLine($"  - CantidadInsertada: {(response as ResAgregarSintomasSeleccionados)?.CantidadInsertada}");

            if (response?.errores?.Any() == true)
            {
                Debug.WriteLine($"  - Errores:");
                foreach (var error in response.errores)
                    Debug.WriteLine($"    * {error.mensaje}");
            }

            // PASO 6: Procesar respuesta
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (response?.resultado == true)
                {
                    Debug.WriteLine("EXITO: Sintomas guardados correctamente");
                    await Shell.Current.DisplayAlert("Exito", "Sintomas guardados correctamente", "OK");
                    await CerrarModal();
                }
                else
                {
                    var mensaje = response?.errores?.FirstOrDefault()?.mensaje ??
                                 response?.Mensaje ??
                                 "Error desconocido al guardar sintomas";
                    Debug.WriteLine($"ERROR: {mensaje}");
                    await Shell.Current.DisplayAlert("Error", mensaje, "OK");
                }
            });

            Debug.WriteLine("=== FIN GUARDAR SINTOMAS ===");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"EXCEPCION COMPLETA: {ex}");
            Debug.WriteLine($"StackTrace: {ex.StackTrace}");

            await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayAlert("Error",
                    $"No se pudieron guardar los sintomas debido a un error: {ex.Message}", "OK"));
        }
        finally
        {
            IsLoading = false;
            Debug.WriteLine("IsLoading = false");
        }
    }
    [RelayCommand]
    private async Task CerrarModal()
    {
        try
        {
            Debug.WriteLine("[VM Síntomas] Cerrando modal...");
            if (_popup != null)
            {
                // Devolvemos 'true' para indicar que se hicieron cambios que podrían necesitar un refresh en la página principal
                await _popup.CloseAsync(true);
            }
            else
            {
                Debug.WriteLine("[VM Síntomas] Referencia del popup es null, no se puede cerrar programáticamente.");
                // Si el popup es null, intentar PopModalAsync como fallback (si fue abierto con PushModalAsync)
                await Shell.Current.Navigation.PopModalAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] Error cerrando modal: {ex.Message}");
        }
    }

    private void ActualizarEstados()
    {
        HaySintomasDisponibles = SintomasFiltrados.Any();
        HaySintomasSeleccionados = SintomasSeleccionados.Any();

        // AGREGAR ESTOS LOGS:
        Debug.WriteLine($"[VM Síntomas] Estados actualizados:");
        Debug.WriteLine($"  - HaySintomasDisponibles: {HaySintomasDisponibles}");
        Debug.WriteLine($"  - HaySintomasSeleccionados: {HaySintomasSeleccionados}");
        Debug.WriteLine($"  - SintomasSeleccionados.Count: {SintomasSeleccionados.Count}");
        Debug.WriteLine($"  - Botón Guardar debería estar: {(HaySintomasSeleccionados ? "HABILITADO" : "DESHABILITADO")}");

        if (SintomasSeleccionados.Any())
        {
            Debug.WriteLine($"[VM Síntomas] Síntomas seleccionados:");
            foreach (var s in SintomasSeleccionados)
            {
                Debug.WriteLine($"    - {s.Nombre} (ID: {s.Id}, Manual: {s.EsManual})");
            }
        }
    }

    // Cleanup resources
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _filtroTimer?.Cancel();
            _filtroTimer?.Dispose();
            _cargaSemaphore?.Dispose();
        }
    }
}

// Modelo para síntomas seleccionables
public partial class SintomaSeleccionable : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string nombre = "";

    [ObservableProperty]
    private bool estaSeleccionado = false;

    [ObservableProperty]
    private bool esManual = false;

    [ObservableProperty]
    private bool mostrarTipo = false;
}