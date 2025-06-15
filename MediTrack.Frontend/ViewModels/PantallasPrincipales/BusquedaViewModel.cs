using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales;

public partial class BusquedaViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty] private ReqBuscarMedicamento _terminoBusqueda = new();
    [ObservableProperty] private ResBuscarMedicamento _resultadoBusqueda;
    [ObservableProperty] private bool _mostrarResultados;

    [ObservableProperty]
    private ObservableCollection<UsuarioMedicamentos> _misMedicamentos = new();


    // Controla el indicador de actividad (cargando...)
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotBusy))] private bool _isBusy;



    public bool IsNotBusy => !IsBusy;

    // --- Eventos para comunicar a la Vista ---
    public event EventHandler<ResBuscarMedicamento> BusquedaExitosa;
    public event EventHandler<string> BusquedaFallida;

    public event EventHandler<ResDetalleMedicamentoUsuario> MostrarDetalleMedicamento; // <-- Evento para mostrar el nuevo popup


    // --- Comandos ---
    public IAsyncRelayCommand BuscarCommand { get; }
    public IRelayCommand LimpiarCommand { get; }
    public IAsyncRelayCommand<ResBuscarMedicamento> GuardarMedicamentoCommand { get; }


    public IAsyncRelayCommand CargarMisMedicamentosCommand { get; }
    public IAsyncRelayCommand<UsuarioMedicamentos> VerDetalleCommand { get; }
    public IAsyncRelayCommand<UsuarioMedicamentos> EliminarMedicamentoCommand { get; }



    public BusquedaViewModel(IApiService apiService)
    {
        _apiService = apiService;

        // --------------------------------- DEFINICION DE METODOS QUE VIENEN DE "PANTALLA  BUSQUEDA" ------------------------------------- //
        BuscarCommand = new AsyncRelayCommand(EjecutarBusquedaAsync, () => !IsBusy);
        LimpiarCommand = new RelayCommand(EjecutarLimpiar, () => !IsBusy);
        GuardarMedicamentoCommand = new AsyncRelayCommand<ResBuscarMedicamento>(EjecutarGuardarMedicamentoAsync);
        
        CargarMisMedicamentosCommand = new AsyncRelayCommand(EjecutarCargarMisMedicamentosAsync);
        VerDetalleCommand = new AsyncRelayCommand<UsuarioMedicamentos>(EjecutarVerDetalleAsync);
        EliminarMedicamentoCommand = new AsyncRelayCommand<UsuarioMedicamentos>(EjecutarEliminarMedicamentoAsync);
            
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

    private async Task EjecutarCargarMisMedicamentosAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var userIdStr = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdStr, out int idUsuario)) return;

            var request = new ReqObtenerUsuario { IdUsuario = idUsuario };
            var response = await _apiService.ListarMedicamentosUsuarioAsync(request);

            if (response != null && response.resultado)
            {
                // Limpiamos la lista actual y la llenamos con los nuevos datos
                MisMedicamentos.Clear();
                foreach (var med in response.Medicamentos)
                {
                    MisMedicamentos.Add(med);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar mis medicamentos: {ex.Message}");
            // Opcional: podrías disparar un evento de error aquí
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EjecutarVerDetalleAsync(UsuarioMedicamentos medicamentoSeleccionado)
    {
        if (IsBusy || medicamentoSeleccionado == null) return;
        IsBusy = true;
        try
        {
            var userIdStr = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdStr, out int idUsuario)) return;

            var request = new ReqMedicamento { IdUsuario = idUsuario, IdMedicamento = medicamentoSeleccionado.id_medicamento };
            var response = await _apiService.ObtenerDetalleMedicamentoUsuarioAsync(request);

            if (response != null && response.resultado)
            {
                // Disparamos el evento para que la página (el code-behind) muestre el popup
                MostrarDetalleMedicamento?.Invoke(this, response);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudieron obtener los detalles del medicamento.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al obtener detalle: {ex.Message}");
            await Shell.Current.DisplayAlert("Error de Conexión", "No se pudo comunicar con el servidor.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EjecutarEliminarMedicamentoAsync(UsuarioMedicamentos medicamentoParaEliminar)
    {
        if (medicamentoParaEliminar == null) return;

        // Siempre es buena práctica pedir confirmación para acciones destructivas.
        bool confirmar = await Shell.Current.DisplayAlert(
            "Confirmar Eliminación",
            $"¿Estás seguro de que quieres eliminar {medicamentoParaEliminar.nombre_comercial} de tu lista?",
            "Sí, eliminar",
            "Cancelar");

        if (!confirmar) return;

        IsBusy = true;
        try
        {
            var userIdStr = await SecureStorage.GetAsync("user_id");
            if (!int.TryParse(userIdStr, out int idUsuario)) return;

            var request = new ReqMedicamento { IdUsuario = idUsuario, IdMedicamento = medicamentoParaEliminar.id_medicamento };
            var response = await _apiService.EliminarMedicamentoUsuarioAsync(request);

            if (response != null && response.resultado)
            {
                // Si la eliminación en el backend fue exitosa, lo quitamos de la colección local
                // para que la UI se actualice al instante.
                MisMedicamentos.Remove(medicamentoParaEliminar);
                await Shell.Current.DisplayAlert("Éxito", "Medicamento eliminado de tu lista.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", response?.Mensaje ?? "No se pudo eliminar el medicamento.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al eliminar medicamento: {ex.Message}");
            await Shell.Current.DisplayAlert("Error de Conexión", "No se pudo comunicar con el servidor.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

}