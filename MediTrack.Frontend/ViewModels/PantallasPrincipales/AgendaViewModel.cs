using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MediTrack.Frontend.Models;
using MediTrack.Frontend.Vistas.PantallasPrincipales;
using System.Globalization;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.ViewModels.Base;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class AgendaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private DateTime fechaSeleccionada = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<EventoAgenda> eventosDelDia = new();

        [ObservableProperty]
        private string mesActual = "";

        [ObservableProperty]
        private string fechaFormateada = "";

        [ObservableProperty]
        private int eventosCompletados = 0;

        [ObservableProperty]
        private int eventosPendientes = 0;

        [ObservableProperty]
        private int totalEventos = 0;

        [ObservableProperty]
        private string estadisticasTexto = "";

        private readonly EventosService _eventosService;
        private readonly CultureInfo _culturaEspañola = new("es-ES");

        public AgendaViewModel()
        {
            try
            {
                Title = "Agenda";

                // Usar servicio compartido
                _eventosService = EventosService.Instance;

                // Suscribirse a cambios
                _eventosService.EventoActualizado += OnEventoActualizado;
                _eventosService.EventoAgregado += OnEventoAgregado;
                _eventosService.EventoEliminado += OnEventoEliminado;

                // ✅ CONFIGURAR CULTURA ESPAÑOLA
                CultureInfo.CurrentCulture = _culturaEspañola;
                CultureInfo.CurrentUICulture = _culturaEspañola;

                ActualizarTextosFecha();
                CargarEventosDelDia();

                PropertyChanged += OnPropertyChanged;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en AgendaViewModel: {ex.Message}");
            }
        }

        public override async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                ActualizarTextosFecha();
                CargarEventosDelDia();
                await Task.CompletedTask;
            });
        }

        private void OnEventoActualizado(object sender, EventoAgenda evento)
        {
            // Recargar eventos si el cambio afecta la fecha actual
            if (evento.FechaHora.Date == FechaSeleccionada.Date)
            {
                CargarEventosDelDia();
            }
        }

        private void OnEventoAgregado(object sender, EventoAgenda evento)
        {
            // Recargar eventos si el nuevo evento afecta la fecha actual
            if (evento.FechaHora.Date == FechaSeleccionada.Date)
            {
                CargarEventosDelDia();
            }
        }

        private void OnEventoEliminado(object sender, EventoAgenda evento)
        {
            // Recargar eventos si la eliminación afecta la fecha actual
            if (evento.FechaHora.Date == FechaSeleccionada.Date)
            {
                CargarEventosDelDia();
            }
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FechaSeleccionada))
            {
                ActualizarTextosFecha();
                CargarEventosDelDia();
            }
        }

        private void ActualizarTextosFecha()
        {
            try
            {
                // ✅ FORMATEAR FECHAS EN ESPAÑOL
                MesActual = FechaSeleccionada.ToString("MMMM yyyy", _culturaEspañola);
                FechaFormateada = FechaSeleccionada.ToString("dddd, dd 'de' MMMM", _culturaEspañola);

                // Capitalizar primera letra
                if (!string.IsNullOrEmpty(MesActual))
                {
                    MesActual = char.ToUpper(MesActual[0]) + MesActual[1..];
                }

                if (!string.IsNullOrEmpty(FechaFormateada))
                {
                    FechaFormateada = char.ToUpper(FechaFormateada[0]) + FechaFormateada[1..];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando textos de fecha: {ex.Message}");
            }
        }

        public void CargarEventosDelDia()
        {
            try
            {
                EventosDelDia.Clear();

                // Usar servicio compartido
                var eventos = _eventosService.ObtenerEventos(FechaSeleccionada);

                foreach (var evento in eventos)
                {
                    EventosDelDia.Add(evento);
                }

                // Calcular estadísticas
                ActualizarEstadisticas();

                System.Diagnostics.Debug.WriteLine($"Cargados {EventosDelDia.Count} eventos para {FechaSeleccionada:yyyy-MM-dd} desde servicio");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando eventos: {ex.Message}");
            }
        }

        private void ActualizarEstadisticas()
        {
            try
            {
                var (completados, pendientes, total) = _eventosService.ObtenerEstadisticas(FechaSeleccionada);

                EventosCompletados = completados;
                EventosPendientes = pendientes;
                TotalEventos = total;

                // Crear texto descriptivo
                if (total == 0)
                {
                    EstadisticasTexto = "Sin eventos";
                }
                else
                {
                    var porcentaje = total > 0 ? (completados * 100 / total) : 0;
                    EstadisticasTexto = $"{completados} de {total} completados ({porcentaje}%)";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando estadísticas: {ex.Message}");
                EstadisticasTexto = "Error en estadísticas";
            }
        }

        [RelayCommand]
        private async Task AgregarEvento()
        {
            await ExecuteAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"Abriendo modal para agregar evento en fecha: {FechaSeleccionada:yyyy-MM-dd}");

                // Crear y abrir el modal con el nuevo ViewModel
                var modal = new ModalAgregarEvento(FechaSeleccionada);

                // Mostrar el modal
                await Application.Current.MainPage.Navigation.PushModalAsync(modal);

                // Esperar a que el modal se cierre
                await EsperarCierreModal(modal);
            });
        }

        private async Task EsperarCierreModal(ModalAgregarEvento modal)
        {
            try
            {
                // Esperar en un bucle hasta que el modal se cierre
                while (Application.Current.MainPage.Navigation.ModalStack.Contains(modal))
                {
                    await Task.Delay(100);
                }

                // Si se guardó un evento, ya está agregado automáticamente por el ViewModel del modal
                // Solo necesitamos confirmar que se guardó
                if (modal.EventoGuardado && modal.EventoCreado != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Evento guardado exitosamente: {modal.EventoCreado.Titulo}");

                    // Los eventos se recargarán automáticamente por la notificación del EventosService
                    // El AgregarEventoViewModel ya agregó el evento al servicio
                    // No necesitamos duplicar la lógica aquí
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error esperando cierre de modal: {ex.Message}");
                await HandleErrorAsync(ex);
            }
        }

        [RelayCommand]
        private async Task EliminarEvento(EventoAgenda evento)
        {
            await ExecuteAsync(async () =>
            {
                if (evento == null) return;

                // Confirmar eliminación
                bool confirmar = await ShowConfirmAsync(
                    "Eliminar evento",
                    $"¿Estás seguro de que deseas eliminar '{evento.Titulo}'?",
                    "Eliminar", "Cancelar");

                if (!confirmar) return;

                // Eliminar del servicio
                bool eliminado = _eventosService.EliminarEvento(evento);

                if (eliminado)
                {
                    await ShowAlertAsync("Éxito", $"Evento '{evento.Titulo}' eliminado correctamente");

                    // Recargar eventos (se hará automáticamente por la notificación del servicio)
                    System.Diagnostics.Debug.WriteLine($"Evento eliminado exitosamente: {evento.Titulo}");
                }
                else
                {
                    await ShowAlertAsync("Error", "No se pudo eliminar el evento. Inténtalo de nuevo.");
                }
            });
        }

        [RelayCommand]
        private async Task EditarEvento(EventoAgenda evento)
        {
            await ExecuteAsync(async () =>
            {
                if (evento == null) return;

                System.Diagnostics.Debug.WriteLine($"Editando evento: {evento.Titulo}");

                // TODO: Crear ModalEditarEvento o reutilizar ModalAgregarEvento con modo edición
                await ShowAlertAsync("En desarrollo", "La función de editar estará disponible pronto");
            });
        }

        [RelayCommand]
        private async Task MostrarEstadisticas()
        {
            await ExecuteAsync(async () =>
            {
                var (completados, pendientes, total) = _eventosService.ObtenerEstadisticas(FechaSeleccionada);

                if (total == 0)
                {
                    await ShowAlertAsync("Estadísticas", "No hay eventos para este día");
                    return;
                }

                var porcentaje = (completados * 100 / total);
                var mensaje = $"{FechaFormateada}\n\n" +
                             $"Total de eventos: {total}\n" +
                             $"Completados: {completados}\n" +
                             $"Pendientes: {pendientes}\n" +
                             $"Progreso: {porcentaje}%";

                await ShowAlertAsync("Estadísticas del día", mensaje);
            });
        }

        [RelayCommand]
        private async Task LimpiarCompletados()
        {
            await ExecuteAsync(async () =>
            {
                var eventosCompletados = EventosDelDia.Where(e => e.Completado).ToList();

                if (!eventosCompletados.Any())
                {
                    await ShowAlertAsync("Información", "No hay eventos completados para eliminar");
                    return;
                }

                bool confirmar = await ShowConfirmAsync(
                    "Limpiar completados",
                    $"¿Deseas eliminar los {eventosCompletados.Count} eventos completados?",
                    "Eliminar", "Cancelar");

                if (!confirmar) return;

                int eliminados = 0;
                foreach (var evento in eventosCompletados)
                {
                    if (_eventosService.EliminarEvento(evento))
                    {
                        eliminados++;
                    }
                }

                await ShowAlertAsync("Limpieza completada", $"Se eliminaron {eliminados} eventos completados");
            });
        }

        [RelayCommand]
        private async Task MarcarCompletado(EventoAgenda evento)
        {
            await ExecuteAsync(async () =>
            {
                if (evento != null)
                {
                    evento.Completado = !evento.Completado;

                    // Notificar al servicio que hubo cambios
                    _eventosService.ActualizarEstadoEvento(evento);

                    System.Diagnostics.Debug.WriteLine($"Evento {evento.Titulo} marcado como {(evento.Completado ? "completado" : "pendiente")}");
                }
                await Task.CompletedTask;
            });
        }

        [RelayCommand]
        private async Task IrAHoy()
        {
            await ExecuteAsync(async () =>
            {
                FechaSeleccionada = DateTime.Today;
                await Task.CompletedTask;
            });
        }

        [RelayCommand]
        private async Task IrAMañana()
        {
            await ExecuteAsync(async () =>
            {
                FechaSeleccionada = DateTime.Today.AddDays(1);
                await Task.CompletedTask;
            });
        }

        [RelayCommand]
        private async Task Navegar(string destino)
        {
            await ExecuteAsync(async () =>
            {
                switch (destino.ToLower())
                {
                    case "hoy":
                        FechaSeleccionada = DateTime.Today;
                        break;
                    case "mañana":
                        FechaSeleccionada = DateTime.Today.AddDays(1);
                        break;
                    case "anterior":
                        FechaSeleccionada = FechaSeleccionada.AddDays(-1);
                        break;
                    case "siguiente":
                        FechaSeleccionada = FechaSeleccionada.AddDays(1);
                        break;
                }
                await Task.CompletedTask;
            });
        }

        [RelayCommand]
        private async Task RecargarEventos()
        {
            await ExecuteAsync(async () =>
            {
                CargarEventosDelDia();
                await Task.CompletedTask;
            });
        }

        protected override async Task HandleErrorAsync(Exception exception)
        {
            await base.HandleErrorAsync(exception);

            if (exception.Message.Contains("servicio"))
            {
                ErrorMessage = "Error conectando con el servicio de eventos";
            }
            else
            {
                ErrorMessage = "Error inesperado en la agenda";
            }
        }

        ~AgendaViewModel()
        {
            try
            {
                if (_eventosService != null)
                {
                    _eventosService.EventoActualizado -= OnEventoActualizado;
                    _eventosService.EventoAgregado -= OnEventoAgregado;
                    _eventosService.EventoEliminado -= OnEventoEliminado;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en destructor de AgendaViewModel: {ex.Message}");
            }
        }
    }
}