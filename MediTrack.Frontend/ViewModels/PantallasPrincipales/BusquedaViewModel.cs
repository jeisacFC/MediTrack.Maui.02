using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Services.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales;

public partial class BusquedaViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    // --- Propiedades para enlazar en la UI ---

    // Un objeto que agrupa todos los campos del formulario de búsqueda
    [ObservableProperty]
    private ReqBuscarMedicamento _terminoBusqueda = new();

    // Almacena el resultado completo para mostrarlo después
    [ObservableProperty]
    private ResBuscarMedicamento _resultadoBusqueda;

    // Controla si la sección de resultados es visible
    [ObservableProperty]
    private bool _mostrarResultados;

    // Controla el indicador de actividad (cargando...)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    // --- Eventos para comunicar a la Vista ---
    public event EventHandler<ResBuscarMedicamento> BusquedaExitosa;
    public event EventHandler<string> BusquedaFallida;

    // --- Comandos ---
    public IAsyncRelayCommand BuscarCommand { get; }
    public IRelayCommand LimpiarCommand { get; }

    public BusquedaViewModel(IApiService apiService)
    {
        _apiService = apiService;
        BuscarCommand = new AsyncRelayCommand(EjecutarBusquedaAsync, () => !IsBusy);
        LimpiarCommand = new RelayCommand(EjecutarLimpiar, () => !IsBusy);
    }

    private async Task EjecutarBusquedaAsync()
    {
        if (string.IsNullOrWhiteSpace(TerminoBusqueda.NombreMedicamento) &&
            string.IsNullOrWhiteSpace(TerminoBusqueda.Dosis) &&
            string.IsNullOrWhiteSpace(TerminoBusqueda.PrincipioActivo))
        {
            BusquedaFallida?.Invoke(this, "Por favor, ingresa al menos un criterio de búsqueda.");
            return;
        }

        IsBusy = true;
        MostrarResultados = false;

        try
        {
            Debug.WriteLine($"ViewModel: Buscando medicamento: {TerminoBusqueda.NombreMedicamento}");
            var resultado = await _apiService.BuscarMedicamentoManualAsync(TerminoBusqueda);

            if (resultado != null && resultado.resultado && resultado.Medicamento != null)
            {
                ResultadoBusqueda = resultado;
                MostrarResultados = true; // Podrías usar esto para mostrar una sección de resultados en la misma página
                Debug.WriteLine($"ViewModel: Medicamento encontrado: {ResultadoBusqueda.Medicamento.NombreComercial}");

                // Dispara el evento para que la Vista muestre el modal/popup
                BusquedaExitosa?.Invoke(this, ResultadoBusqueda);
            }
            else
            {
                string errorMsg = resultado?.errores?.FirstOrDefault()?.Message ?? "No se encontró ningún medicamento con esos criterios.";
                BusquedaFallida?.Invoke(this, errorMsg);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en EjecutarBusquedaAsync: {ex.Message}");
            BusquedaFallida?.Invoke(this, "No se pudo realizar la búsqueda. Por favor, intenta de nuevo.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void EjecutarLimpiar()
    {
        // Crea una nueva instancia para limpiar los campos enlazados en la UI
        TerminoBusqueda = new ReqBuscarMedicamento();
        MostrarResultados = false;
        ResultadoBusqueda = null;
    }
}