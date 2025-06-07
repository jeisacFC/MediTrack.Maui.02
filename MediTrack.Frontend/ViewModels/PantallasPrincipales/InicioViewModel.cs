using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Models.Model;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class InicioViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<EventoAgenda> medicamentosHoy = new();

        [ObservableProperty]
        private string fechaHoy = "";

        private readonly EventosService _eventosService;
        private readonly CultureInfo _culturaEspañola = new("es-ES");

        public InicioViewModel()
        {
            try
            {
                // Usar servicio compartido
                _eventosService = EventosService.Instance;

                // Suscribirse a cambios
                _eventosService.EventoActualizado += OnEventoActualizado;

                // Configurar cultura española
                CultureInfo.CurrentCulture = _culturaEspañola;
                CultureInfo.CurrentUICulture = _culturaEspañola;

                ActualizarFechaHoy();
                CargarMedicamentosHoy();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en InicioViewModel: {ex.Message}");
            }
        }

        private void OnEventoActualizado(object sender, EventoAgenda evento)
        {
            // Recargar medicamentos cuando hay cambios
            if (evento.Tipo == "Medicamento" && evento.FechaHora.Date == DateTime.Today)
            {
                CargarMedicamentosHoy();
            }
        }

        private void ActualizarFechaHoy()
        {
            try
            {
                FechaHoy = DateTime.Today.ToString("dddd, dd 'de' MMMM", _culturaEspañola);

                // Capitalizar primera letra
                if (!string.IsNullOrEmpty(FechaHoy))
                {
                    FechaHoy = char.ToUpper(FechaHoy[0]) + FechaHoy[1..];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando fecha: {ex.Message}");
                FechaHoy = DateTime.Today.ToString("dd/MM/yyyy");
            }
        }

        private void CargarMedicamentosHoy()
        {
            try
            {
                MedicamentosHoy.Clear();

                // Usar servicio compartido
                var medicamentos = _eventosService.ObtenerMedicamentosHoy();

                foreach (var medicamento in medicamentos)
                {
                    MedicamentosHoy.Add(medicamento);
                }

                System.Diagnostics.Debug.WriteLine($"Cargados {MedicamentosHoy.Count} medicamentos para hoy desde servicio");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando medicamentos: {ex.Message}");
            }
        }

        [RelayCommand]
        private void MarcarTomado(EventoAgenda medicamento)
        {
            try
            {
                if (medicamento != null)
                {
                    medicamento.Completado = !medicamento.Completado;

                    // Notificar al servicio que hubo cambios
                    _eventosService.ActualizarEstadoEvento(medicamento);

                    System.Diagnostics.Debug.WriteLine($"Medicamento {medicamento.Titulo} marcado como {(medicamento.Completado ? "tomado" : "pendiente")}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marcando medicamento: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task IrAAgenda()
        {
            try
            {
                // Navegar a la pantalla de agenda
                await Shell.Current.GoToAsync("//agenda");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a agenda: {ex.Message}");
            }
        }

        // Método público para ser llamado desde OnAppearing si es necesario
        public void RecargarMedicamentos()
        {
            CargarMedicamentosHoy();
        }

        // Cleanup
        ~InicioViewModel()
        {
            if (_eventosService != null)
            {
                _eventosService.EventoActualizado -= OnEventoActualizado;
            }
        }
    }
}