using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private bool _sintomasYaCargados = false;

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
            // Si ya están cargados, usar cache
            if (_sintomasYaCargados && _sintomasCache.Any())
            {
                await CargarDesdeCache();
                return;
            }

            IsLoading = true;
            Debug.WriteLine("[VM Síntomas] Iniciando carga de síntomas...");

            // Ejecutar carga pesada en hilo de fondo
            var sintomasResult = await Task.Run(async () =>
            {
                try
                {
                    // MÉTODO PRINCIPAL: Obtener TODOS los síntomas
                    var todosLosSintomas = await _apiService.ObtenerTodosLosSintomasAsync().ConfigureAwait(false);
                    Debug.WriteLine($"[VM Síntomas] Respuesta API: resultado={todosLosSintomas?.resultado}, count={todosLosSintomas?.Sintomas?.Count}");

                    if (todosLosSintomas?.resultado == true && todosLosSintomas.Sintomas?.Any() == true)
                    {
                        // Procesar y ordenar en hilo de fondo
                        var sintomasOrdenados = todosLosSintomas.Sintomas
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

                        return new { Success = true, Sintomas = sintomasOrdenados, Metodo = "Principal" };
                    }

                    // MÉTODO ALTERNATIVO: Usar filtros múltiples
                    Debug.WriteLine("[VM Síntomas] Método principal falló, intentando alternativo...");
                    return await CargarConMetodoAlternativo().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[VM Síntomas] Error en carga: {ex.Message}");
                    return new { Success = false, Sintomas = new List<SintomaSeleccionable>(), Metodo = "Error" };
                }
            }).ConfigureAwait(false);

            // Actualizar UI en hilo principal
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SintomasDisponibles.Clear();
                SintomasFiltrados.Clear();

                if (sintomasResult.Success && sintomasResult.Sintomas.Any())
                {
                    // Agregar síntomas por lotes para evitar bloqueos
                    var lotes = sintomasResult.Sintomas.Chunk(20);
                    foreach (var lote in lotes)
                    {
                        foreach (var sintoma in lote)
                        {
                            SintomasDisponibles.Add(sintoma);
                            SintomasFiltrados.Add(sintoma);
                        }
                    }

                    _sintomasCache = sintomasResult.Sintomas.ToList();
                    _sintomasYaCargados = true;
                    Debug.WriteLine($"[VM Síntomas] ÉXITO {sintomasResult.Metodo}: {SintomasDisponibles.Count} síntomas cargados");
                }
                else
                {
                    CargarSintomasEmergencia();
                }

                ActualizarEstados();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] ERROR CRÍTICO: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CargarSintomasEmergencia();
                ActualizarEstados();
            });
        }
        finally
        {
            IsLoading = false;
            _cargaSemaphore.Release();
        }
    }

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
                    await Task.Delay(50).ConfigureAwait(false); // Pausa breve
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[VM Síntomas] Error con filtro '{filtro}': {ex.Message}");
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

                return new { Success = true, Sintomas = sintomasOrdenados, Metodo = "Alternativo" };
            }

            return new { Success = false, Sintomas = new List<SintomaSeleccionable>(), Metodo = "Alternativo Fallido" };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] Error en método alternativo: {ex.Message}");
            return new { Success = false, Sintomas = new List<SintomaSeleccionable>(), Metodo = "Error Alternativo" };
        }
    }

    private async Task CargarDesdeCache()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            SintomasDisponibles.Clear();
            SintomasFiltrados.Clear();

            foreach (var sintoma in _sintomasCache.Where(s => !s.EstaSeleccionado))
            {
                var nuevoSintoma = new SintomaSeleccionable
                {
                    Id = sintoma.Id,
                    Nombre = sintoma.Nombre,
                    EsManual = sintoma.EsManual,
                    EstaSeleccionado = false,
                    MostrarTipo = sintoma.MostrarTipo
                };
                SintomasDisponibles.Add(nuevoSintoma);
                SintomasFiltrados.Add(nuevoSintoma);
            }

            ActualizarEstados();
            Debug.WriteLine($"[VM Síntomas] Cargado desde cache: {SintomasDisponibles.Count} síntomas");
        });
    }

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

        foreach (var sintoma in sintomasEmergencia)
        {
            SintomasDisponibles.Add(sintoma);
            SintomasFiltrados.Add(sintoma);
        }

        Debug.WriteLine($"[VM Síntomas] RECOVERY: {SintomasDisponibles.Count} síntomas de emergencia");
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
            Debug.WriteLine($"[VM Síntomas] Filtrando con: '{filtro}'");
            FiltroBusqueda = filtro ?? "";

            // Ejecutar filtrado en memoria (más rápido)
            var sintomasParaMostrar = string.IsNullOrWhiteSpace(FiltroBusqueda)
                ? SintomasDisponibles.Where(s => !s.EstaSeleccionado).ToList()
                : SintomasDisponibles.Where(s => !s.EstaSeleccionado &&
                    s.Nombre.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase)).ToList();

            // Actualizar colección de una vez
            SintomasFiltrados.Clear();
            foreach (var sintoma in sintomasParaMostrar)
            {
                SintomasFiltrados.Add(sintoma);
            }

            Debug.WriteLine($"[VM Síntomas] Filtrado: {SintomasFiltrados.Count} síntomas");

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
            if (sintoma == null) return;

            Debug.WriteLine($"[VM Síntomas] Toggle: {sintoma.Nombre} (actual: {sintoma.EstaSeleccionado})");
            sintoma.EstaSeleccionado = !sintoma.EstaSeleccionado;

            if (sintoma.EstaSeleccionado)
            {
                if (!SintomasSeleccionados.Any(s => s.Id == sintoma.Id))
                {
                    SintomasSeleccionados.Add(sintoma);
                    Debug.WriteLine($"[VM Síntomas] Agregado: {sintoma.Nombre}");
                }
                SintomasFiltrados.Remove(sintoma);
            }
            else
            {
                var sintomaEnSeleccionados = SintomasSeleccionados.FirstOrDefault(s => s.Id == sintoma.Id);
                if (sintomaEnSeleccionados != null)
                {
                    SintomasSeleccionados.Remove(sintomaEnSeleccionados);
                    Debug.WriteLine($"[VM Síntomas] Removido: {sintoma.Nombre}");
                }

                if (string.IsNullOrWhiteSpace(FiltroBusqueda) ||
                    sintoma.Nombre.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase))
                {
                    SintomasFiltrados.Add(sintoma);
                }
            }

            ActualizarEstados();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] Error en toggle: {ex.Message}");
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

            if (string.IsNullOrWhiteSpace(FiltroBusqueda) ||
                sintoma.Nombre.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase))
            {
                SintomasFiltrados.Add(sintoma);
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
            if (string.IsNullOrWhiteSpace(NuevoSintomaTexto)) return;

            Debug.WriteLine($"[VM Síntomas] Agregando síntoma manual: {NuevoSintomaTexto}");
            IsLoading = true;

            var userIdStr = await SecureStorage.GetAsync("user_id").ConfigureAwait(false);
            if (!int.TryParse(userIdStr, out var userId))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlert("Error", "Usuario no autenticado", "OK"));
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
                    var nuevoSintoma = new SintomaSeleccionable
                    {
                        Id = DateTime.Now.Millisecond,
                        Nombre = NuevoSintomaTexto.Trim(),
                        EsManual = true,
                        EstaSeleccionado = true,
                        MostrarTipo = true
                    };

                    SintomasDisponibles.Add(nuevoSintoma);
                    SintomasSeleccionados.Add(nuevoSintoma);
                    _sintomasCache.Add(nuevoSintoma);

                    NuevoSintomaTexto = "";
                    PuedeAgregarSintomaManual = false;
                    FiltrarSintomas("");

                    await Shell.Current.DisplayAlert("Éxito", "Síntoma agregado correctamente", "OK");
                    Debug.WriteLine($"[VM Síntomas] Síntoma manual agregado: {nuevoSintoma.Nombre}");
                }
                else
                {
                    var mensaje = response?.errores?.FirstOrDefault()?.mensaje ?? "Error desconocido";
                    await Shell.Current.DisplayAlert("Error", mensaje, "OK");
                    Debug.WriteLine($"[VM Síntomas] Error agregando síntoma: {mensaje}");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] Error agregando síntoma manual: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayAlert("Error", "No se pudo agregar el síntoma", "OK"));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GuardarSintomas()
    {
        try
        {
            if (!SintomasSeleccionados.Any())
            {
                await Shell.Current.DisplayAlert("Aviso", "Selecciona al menos un síntoma", "OK");
                return;
            }

            Debug.WriteLine($"[VM Síntomas] Guardando {SintomasSeleccionados.Count} síntomas");
            IsLoading = true;

            var userIdStr = await SecureStorage.GetAsync("user_id").ConfigureAwait(false);
            if (!int.TryParse(userIdStr, out var userId))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.DisplayAlert("Error", "Usuario no autenticado", "OK"));
                return;
            }

            var request = new ReqAgregarSintomasSeleccionados
            {
                IdUsuario = userId,
                IdsSintomas = SintomasSeleccionados.Select(s => s.Id).ToList()
            };

            Debug.WriteLine($"[VM Síntomas] Enviando: UserId={userId}, Síntomas=[{string.Join(",", request.IdsSintomas)}]");

            var response = await _apiService.AgregarSintomasSeleccionadosAsync(request).ConfigureAwait(false);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (response?.resultado == true)
                {
                    await Shell.Current.DisplayAlert("Éxito", "Síntomas guardados correctamente", "OK");
                    Debug.WriteLine("[VM Síntomas] Síntomas guardados exitosamente");
                    await CerrarModal();
                }
                else
                {
                    var mensaje = response?.errores?.FirstOrDefault()?.mensaje ?? "Error desconocido";
                    await Shell.Current.DisplayAlert("Error", mensaje, "OK");
                    Debug.WriteLine($"[VM Síntomas] Error guardando: {mensaje}");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM Síntomas] Error guardando síntomas: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayAlert("Error", "No se pudieron guardar los síntomas", "OK"));
        }
        finally
        {
            IsLoading = false;
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
                await _popup.CloseAsync();
            }
            else
            {
                Debug.WriteLine("[VM Síntomas] Referencia del popup es null");
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
        Debug.WriteLine($"[VM Síntomas] Estados: Disponibles={HaySintomasDisponibles}, Seleccionados={HaySintomasSeleccionados}");
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