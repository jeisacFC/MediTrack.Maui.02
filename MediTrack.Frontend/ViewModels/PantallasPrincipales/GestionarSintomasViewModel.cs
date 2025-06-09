using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public GestionarSintomasViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task CargarSintomasDisponibles()
    {
        try
        {
            IsLoading = true;
            SintomasDisponibles.Clear();

            // Buscar todos los síntomas disponibles
            var request = new ReqBuscarSintoma { Filtro = "" };
            var response = await _apiService.BuscarSintomasAsync(request);

            if (response != null && response.Any())
            {
                foreach (var sintoma in response)
                {
                    SintomasDisponibles.Add(new SintomaSeleccionable
                    {
                        Id = sintoma.IdSintoma, // Usar IdSintoma en lugar de Id
                        Nombre = sintoma.Nombre,
                        EsManual = false, // ResBuscarSintoma no tiene EsManual
                        EstaSeleccionado = false,
                        MostrarTipo = false
                    });
                }
            }
            else
            {
                // Fallback a datos de ejemplo si no hay respuesta del servidor
                var sintomasEjemplo = new List<SintomaSeleccionable>
                {
                    new() { Id = 1, Nombre = "Dolor de cabeza", EsManual = false, EstaSeleccionado = false },
                    new() { Id = 2, Nombre = "Fiebre", EsManual = false, EstaSeleccionado = false },
                    new() { Id = 3, Nombre = "Náuseas", EsManual = false, EstaSeleccionado = false },
                    new() { Id = 4, Nombre = "Mareos", EsManual = false, EstaSeleccionado = false },
                    new() { Id = 5, Nombre = "Fatiga", EsManual = false, EstaSeleccionado = false },
                };

                foreach (var sintoma in sintomasEjemplo)
                {
                    SintomasDisponibles.Add(sintoma);
                }
            }

            FiltrarSintomas(FiltroBusqueda);
            ActualizarEstados();

            Debug.WriteLine($"Cargados {SintomasDisponibles.Count} síntomas disponibles");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cargando síntomas: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "No se pudieron cargar los síntomas", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void FiltrarSintomas(string filtro)
    {
        try
        {
            FiltroBusqueda = filtro ?? "";
            SintomasFiltrados.Clear();

            var sintomasParaMostrar = string.IsNullOrWhiteSpace(FiltroBusqueda)
                ? SintomasDisponibles.Where(s => !s.EstaSeleccionado)
                : SintomasDisponibles.Where(s => !s.EstaSeleccionado &&
                    s.Nombre.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase));

            foreach (var sintoma in sintomasParaMostrar)
            {
                SintomasFiltrados.Add(sintoma);
            }

            // Mostrar opción de agregar síntoma manual si no hay resultados y hay texto
            PuedeAgregarSintomaManual = !string.IsNullOrWhiteSpace(FiltroBusqueda) &&
                                       !SintomasFiltrados.Any();

            ActualizarEstados();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error filtrando síntomas: {ex.Message}");
        }
    }

    public void ToggleSintomaSeleccion(SintomaSeleccionable sintoma)
    {
        try
        {
            if (sintoma == null) return;

            sintoma.EstaSeleccionado = !sintoma.EstaSeleccionado;

            if (sintoma.EstaSeleccionado)
            {
                // Agregar a seleccionados
                if (!SintomasSeleccionados.Any(s => s.Id == sintoma.Id))
                {
                    SintomasSeleccionados.Add(sintoma);
                }

                // Remover de filtrados
                SintomasFiltrados.Remove(sintoma);
            }
            else
            {
                // Remover de seleccionados
                var sintomaEnSeleccionados = SintomasSeleccionados.FirstOrDefault(s => s.Id == sintoma.Id);
                if (sintomaEnSeleccionados != null)
                {
                    SintomasSeleccionados.Remove(sintomaEnSeleccionados);
                }

                // Agregar de vuelta a filtrados si cumple el filtro
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
            Debug.WriteLine($"Error en toggle síntoma: {ex.Message}");
        }
    }

    [RelayCommand]
    private void RemoverSintomaSeleccionado(SintomaSeleccionable sintoma)
    {
        try
        {
            if (sintoma == null) return;

            sintoma.EstaSeleccionado = false;
            SintomasSeleccionados.Remove(sintoma);

            // Agregar de vuelta a la lista si cumple el filtro
            if (string.IsNullOrWhiteSpace(FiltroBusqueda) ||
                sintoma.Nombre.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase))
            {
                SintomasFiltrados.Add(sintoma);
            }

            ActualizarEstados();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error removiendo síntoma: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AgregarSintomaManual()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NuevoSintomaTexto)) return;

            IsLoading = true;

            var userIdStr = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdStr, out var userId))
            {
                await Shell.Current.DisplayAlert("Error", "Usuario no autenticado", "OK");
                return;
            }

            var request = new ReqInsertarSintomaManual
            {
                IdUsuario = userId,
                Descripcion = NuevoSintomaTexto.Trim()
            };

            var response = await _apiService.InsertarSintomaManualAsync(request);
            if (response?.resultado == true)
            {
                // Agregar el nuevo síntoma a la lista
                var nuevoSintoma = new SintomaSeleccionable
                {
                    Id = DateTime.Now.Millisecond, // ID temporal hasta obtener el real del backend
                    Nombre = NuevoSintomaTexto.Trim(),
                    EsManual = true,
                    EstaSeleccionado = true,
                    MostrarTipo = true
                };

                SintomasDisponibles.Add(nuevoSintoma);
                SintomasSeleccionados.Add(nuevoSintoma);

                NuevoSintomaTexto = "";
                PuedeAgregarSintomaManual = false;
                FiltrarSintomas("");

                await Shell.Current.DisplayAlert("Éxito", "Síntoma agregado correctamente", "OK");
            }
            else
            {
                var mensaje = response?.errores?.FirstOrDefault()?.mensaje ?? "Error desconocido";
                await Shell.Current.DisplayAlert("Error", mensaje, "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error agregando síntoma manual: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "No se pudo agregar el síntoma", "OK");
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

            IsLoading = true;

            var userIdStr = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdStr, out var userId))
            {
                await Shell.Current.DisplayAlert("Error", "Usuario no autenticado", "OK");
                return;
            }

            var request = new ReqAgregarSintomasSeleccionados
            {
                IdUsuario = userId,
                IdsSintomas = SintomasSeleccionados.Select(s => s.Id).ToList()
            };

            var response = await _apiService.AgregarSintomasSeleccionadosAsync(request);
            if (response?.resultado == true)
            {
                await Shell.Current.DisplayAlert("Éxito", "Síntomas guardados correctamente", "OK");
                await CerrarModal();
            }
            else
            {
                var mensaje = response?.errores?.FirstOrDefault()?.mensaje ?? "Error desconocido";
                await Shell.Current.DisplayAlert("Error", mensaje, "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error guardando síntomas: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "No se pudieron guardar los síntomas", "OK");
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
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PopModalAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cerrando modal: {ex.Message}");
        }
    }

    private void ActualizarEstados()
    {
        HaySintomasDisponibles = SintomasFiltrados.Any();
        HaySintomasSeleccionados = SintomasSeleccionados.Any();
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