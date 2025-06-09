using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MediTrack.Frontend.Vistas.PantallasPrincipales;
using System.Globalization;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels.Base;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Models.Request;
using System.Diagnostics;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class AgendaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private DateTime fechaSeleccionada = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<ResEventoCalendario> eventosDelDia = new();

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

        [ObservableProperty]
        private bool hayEventos = false;

        private readonly IApiService _apiService;
        private readonly CultureInfo _culturaEspañola = new("es-ES");
        private int _idUsuarioActual = 0;

        public AgendaViewModel(IApiService apiService)
        {
            try
            {
                Title = "Agenda";
                _apiService = apiService;

                //  CONFIGURAR CULTURA ESPAÑOLA
                CultureInfo.CurrentCulture = _culturaEspañola;
                CultureInfo.CurrentUICulture = _culturaEspañola;

                ActualizarTextosFecha();

                PropertyChanged += OnPropertyChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en AgendaViewModel: {ex.Message}");
            }
        }

        public override async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                // Obtener ID del usuario actual
                await ObtenerIdUsuarioActual();

                ActualizarTextosFecha();
                await CargarEventosDelDiaAsync();
            });
        }

        private async Task ObtenerIdUsuarioActual()
        {
            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var userId))
                {
                    _idUsuarioActual = userId;
                    Debug.WriteLine($"ID Usuario obtenido: {_idUsuarioActual}");
                }
                else
                {
                    Debug.WriteLine("❌ No se pudo obtener ID del usuario desde SecureStorage");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error obteniendo ID usuario: {ex.Message}");
            }
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FechaSeleccionada))
            {
                ActualizarTextosFecha();

                // Cargar eventos de forma asíncrona
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await CargarEventosDelDiaAsync();
                });
            }
        }

        private void ActualizarTextosFecha()
        {
            try
            {
                //  FORMATEAR FECHAS EN ESPAÑOL
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
                Debug.WriteLine($"❌ Error actualizando textos de fecha: {ex.Message}");
            }
        }

        public async Task CargarEventosDelDiaAsync()
        {
            await ExecuteAsync(async () =>
            {
                try
                {
                    EventosDelDia.Clear();

                    if (_idUsuarioActual <= 0)
                    {
                        await ObtenerIdUsuarioActual();
                        if (_idUsuarioActual <= 0)
                        {
                            Debug.WriteLine("❌ No hay usuario autenticado");
                            ActualizarEstadisticas();
                            return;
                        }
                    }

                    Debug.WriteLine($"Cargando eventos para {FechaSeleccionada:yyyy-MM-dd}...");

                    // Crear request para el backend
                    var request = new ReqObtenerUsuario
                    {
                        IdUsuario = _idUsuarioActual
                    };

                    // Llamar al backend
                    var response = await _apiService.ObtenerEventosAsync(request);

                    if (response != null && response.resultado && response.Eventos != null)
                    {
                        // Agregar eventos a la colección
                        foreach (var evento in response.Eventos.OrderBy(e => e.FechaHora))
                        {
                            EventosDelDia.Add(evento);
                        }

                        Debug.WriteLine($"Cargados {EventosDelDia.Count} eventos para {FechaSeleccionada:yyyy-MM-dd}");
                    }
                    else
                    {
                        Debug.WriteLine($"No se obtuvieron eventos del backend: {response?.Mensaje ?? "Respuesta nula"}");
                    }

                    // Actualizar estadísticas
                    ActualizarEstadisticas();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error cargando eventos: {ex.Message}");
                    await HandleErrorAsync(ex);
                }
            });
        }

        private void ActualizarEstadisticas()
        {
            try
            {
                TotalEventos = EventosDelDia.Count;
                HayEventos = TotalEventos > 0;

                // TODO: Cuando el backend soporte estado "completado", usar esa propiedad
                // Por ahora, simular algunos completados para la UI
                EventosCompletados = EventosDelDia.Count(e => e.FechaHora < DateTime.Now);
                EventosPendientes = TotalEventos - EventosCompletados;

                // Crear texto descriptivo
                if (TotalEventos == 0)
                {
                    EstadisticasTexto = "Sin eventos";
                }
                else
                {
                    var porcentaje = TotalEventos > 0 ? (EventosCompletados * 100 / TotalEventos) : 0;
                    EstadisticasTexto = $"{EventosCompletados} de {TotalEventos} completados ({porcentaje}%)";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error actualizando estadísticas: {ex.Message}");
                EstadisticasTexto = "Error en estadísticas";
            }
        }

        [RelayCommand]
        private async Task AgregarEvento()
        {
            await ExecuteAsync(async () =>
            {
                Debug.WriteLine($"Abriendo modal para agregar evento en fecha: {FechaSeleccionada:yyyy-MM-dd}");

                // Crear y abrir el modal
                var modal = new ModalAgregarEvento(FechaSeleccionada);

                // Mostrar el modal
                await Application.Current.MainPage.Navigation.PushModalAsync(modal);

                // Esperar a que el modal se cierre y recargar eventos
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

                // Si se guardó un evento, recargar la lista
                if (modal.EventoGuardado)
                {
                    Debug.WriteLine("Evento guardado, recargando lista...");
                    await CargarEventosDelDiaAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error esperando cierre de modal: {ex.Message}");
                await HandleErrorAsync(ex);
            }
        }

        [RelayCommand]
        private async Task EliminarEvento(ResEventoCalendario evento)
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

                // TODO: Necesitamos el ID del evento para eliminarlo
                // El modelo ResEventoCalendario no tiene ID, necesita ser agregado
                await ShowAlertAsync("En desarrollo", "La eliminación de eventos estará disponible cuando se agregue el ID al modelo de respuesta");

                /*
                // Código para cuando tengamos ID:
                var request = new ReqEvento
                {
                    IdEvento = evento.Id, // Necesario agregar esta propiedad
                    IdUsuario = _idUsuarioActual
                };

                var response = await _apiService.EliminarEventoAsync(request);

                if (response != null && response.resultado)
                {
                    await ShowAlertAsync("Éxito", $"Evento '{evento.Titulo}' eliminado correctamente");
                    await CargarEventosDelDiaAsync(); // Recargar lista
                }
                else
                {
                    await ShowAlertAsync("Error", "No se pudo eliminar el evento. Inténtalo de nuevo.");
                }
                */
            });
        }

        [RelayCommand]
        private async Task EditarEvento(ResEventoCalendario evento)
        {
            await ExecuteAsync(async () =>
            {
                if (evento == null) return;

                Debug.WriteLine($"Editando evento: {evento.Titulo}");

                // TODO: Implementar modal de edición cuando tengamos IDs
                await ShowAlertAsync("En desarrollo", "La edición de eventos estará disponible pronto");
            });
        }

        [RelayCommand]
        private async Task MostrarEstadisticas()
        {
            await ExecuteAsync(async () =>
            {
                if (TotalEventos == 0)
                {
                    await ShowAlertAsync("Estadísticas", "No hay eventos para este día");
                    return;
                }

                var porcentaje = TotalEventos > 0 ? (EventosCompletados * 100 / TotalEventos) : 0;
                var mensaje = $"{FechaFormateada}\n\n" +
                             $"Total de eventos: {TotalEventos}\n" +
                             $"Completados: {EventosCompletados}\n" +
                             $"Pendientes: {EventosPendientes}\n" +
                             $"Progreso: {porcentaje}%";

                await ShowAlertAsync("Estadísticas del día", mensaje);
            });
        }

        [RelayCommand]
        private async Task LimpiarCompletados()
        {
            await ExecuteAsync(async () =>
            {
                // TODO: Implementar cuando tengamos IDs y estado de completado real
                await ShowAlertAsync("En desarrollo", "Esta función estará disponible cuando se implemente el estado de completado en el backend");
            });
        }

        [RelayCommand]
        private async Task MarcarCompletado(ResEventoCalendario evento)
        {
            await ExecuteAsync(async () =>
            {
                if (evento == null) return;

                // TODO: Implementar cuando tengamos IDs y endpoint para cambiar estado
                await ShowAlertAsync("En desarrollo", "Esta función estará disponible cuando se implemente el estado de completado en el backend");

                /*
                // Código para cuando tengamos la funcionalidad completa:
                var request = new ReqEvento
                {
                    IdEvento = evento.Id, // Necesario agregar
                    IdUsuario = _idUsuarioActual
                };

                var response = await _apiService.CompletarEventoAsync(request);

                if (response != null && response.resultado)
                {
                    // Recargar eventos para reflejar el cambio
                    await CargarEventosDelDiaAsync();
                }
                */
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
                await CargarEventosDelDiaAsync();
            });
        }

        protected override async Task HandleErrorAsync(Exception exception)
        {
            await base.HandleErrorAsync(exception);

            if (exception.Message.Contains("conexión") || exception.Message.Contains("HttpRequest"))
            {
                ErrorMessage = "Error de conexión con el servidor";
            }
            else if (exception.Message.Contains("autenticación") || exception.Message.Contains("usuario"))
            {
                ErrorMessage = "Error de autenticación";
            }
            else
            {
                ErrorMessage = "Error inesperado en la agenda";
            }
        }
    }
}