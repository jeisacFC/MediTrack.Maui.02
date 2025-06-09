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
    public IAsyncRelayCommand<ResBuscarMedicamento> GuardarMedicamentoCommand { get; }


    public BusquedaViewModel(IApiService apiService)
    {
        _apiService = apiService;
        BuscarCommand = new AsyncRelayCommand(EjecutarBusquedaAsync, () => !IsBusy);
        LimpiarCommand = new RelayCommand(EjecutarLimpiar, () => !IsBusy);


        // --------------------------------- DEFINICION DE METODOS QUE VIENEN DE "PANTALLA  BUSQUEDA" ------------------------------------- //
        GuardarMedicamentoCommand = new AsyncRelayCommand<ResBuscarMedicamento>(EjecutarGuardarMedicamentoAsync);

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
                string errorMsg = resultado?.errores?.FirstOrDefault()?.mensaje ?? "No se encontró ningún medicamento con esos criterios.";
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
    private async Task EjecutarGuardarMedicamentoAsync(ResBuscarMedicamento resultadoAGuardar)
    {
        if (resultadoAGuardar?.Medicamento == null)
        {
            Debug.WriteLine("Intento de guardar un resultado de búsqueda nulo.");
            return;
        }

        IsBusy = true;
        try
        {
            var userIdStr = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdStr, out int userId))
            {
                await Shell.Current.DisplayAlert("Error de Usuario", "No se pudo identificar al usuario.", "OK");
                return;
            }

            // Mapeamos los datos del resultado de la búsqueda al objeto que espera la API de guardado
            var request = new ReqGuardarMedicamento
            {
                IdUsuario = userId,
                NombreComercial = resultadoAGuardar.Medicamento.NombreComercial,
                PrincipioActivo = resultadoAGuardar.Medicamento.PrincipioActivo,
                Dosis = resultadoAGuardar.Medicamento.Dosis,
                Fabricante = resultadoAGuardar.Medicamento.Fabricante,
                Usos = resultadoAGuardar.Usos,
                Advertencias = resultadoAGuardar.Advertencias,
                EfectosSecundarios = resultadoAGuardar.EfectosSecundarios,
                IdMetodoEscaneo = 2 // 2 para Búsqueda Manual, por ejemplo
            };

            // Llamamos al ApiService para guardar
            var resultadoGuardado = await _apiService.GuardarMedicamentoAsync(request);

            // Mostramos el mensaje que viene del backend
            if (resultadoGuardado != null)
            {
                await Shell.Current.DisplayAlert("Resultado", resultadoGuardado.Mensaje, "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Ocurrió un problema al intentar guardar.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Excepción al guardar medicamento manual: {ex.Message}");
            await Shell.Current.DisplayAlert("Error de Conexión", "No se pudo comunicar con el servidor.", "OK");
        }
        finally
        {
            IsBusy = false;
        }

    }
}