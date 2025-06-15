using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.ViewModels.Base;
using MediTrack.Frontend.Services.Implementaciones;
using System.Collections.ObjectModel;
using System.Globalization;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using System.Diagnostics;
using MediTrack.Frontend.Services.Interfaces;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class AgregarEventoViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly int _userId;

        // Propiedades del formulario
        [ObservableProperty]
        private string titulo = string.Empty;

        [ObservableProperty]
        private string descripcion = string.Empty;

        [ObservableProperty]
        private DateTime fechaInicio = DateTime.Now;

        [ObservableProperty]
        private DateTime fechaFin = DateTime.Now.AddHours(1);

        [ObservableProperty]
        private bool esRecurrente = false;

        [ObservableProperty]
        private int idTipoEvento;

        [ObservableProperty]
        private int? idTipoRecurrencia;

        // EstadoEvento se inicializa por defecto a "pendiente", pero podríamos dejar la propiedad para permitir modificar si se desea.
        [ObservableProperty]
        private string estadoEvento = "pendiente";

        [ObservableProperty]
        private int? idMedicamento;



        [ObservableProperty]
        private ObservableCollection<TiposEvento> tiposEvento = new ObservableCollection<TiposEvento>();

        [ObservableProperty]
        private TiposEvento selectedTipoEvento;

        partial void OnSelectedTipoEventoChanged(TiposEvento value)
        {
            if (value != null)
                idTipoEvento = value.id_tipo_evento; // Ajusta la propiedad del modelo request
        }

        [ObservableProperty]
        private ObservableCollection<TiposRecurrencia> tiposRecurrencia = new ObservableCollection<TiposRecurrencia>();

        [ObservableProperty]
        private TiposRecurrencia selectedTipoRecurrencia;

        partial void OnSelectedTipoRecurrenciaChanged(TiposRecurrencia value)
        {
            if (value != null)
                idTipoRecurrencia = value.id_tipo_recurrencia;
            else
                idTipoRecurrencia = null;
        }

        [ObservableProperty]
        private ObservableCollection<UsuarioMedicamentos> medicamentosUsuario = new ObservableCollection<UsuarioMedicamentos>();

        [ObservableProperty]
        private UsuarioMedicamentos selectedMedicamentoUsuario;

        partial void OnSelectedMedicamentoUsuarioChanged(UsuarioMedicamentos value)
        {
            if (value != null)
                idMedicamento = value.id_medicamento;
            else
                idMedicamento = null;
        }


        // Si quieres listas para pickers (tipos de evento, tipos de recurrencia, medicamentos),
        // podrías cargar aquí ObservableCollection de opciones al inicializar.
        // Por simplicidad, no incluyo carga de listas, pero abajo muestro dónde encajaría:
        // [ObservableProperty]
        // private ObservableCollection<TipoEvento> tiposEvento;
        // [ObservableProperty]
        // private ObservableCollection<TipoRecurrencia> tiposRecurrencia;

        public AgregarEventoViewModel(IApiService apiService, int userId)
        {
            _apiService = apiService;
            _userId = userId;
            Title = "Crear Evento Médico";

            // Si necesitas cargar listas para pickers:
            tiposEvento = new ObservableCollection<TiposEvento>();
            tiposRecurrencia = new ObservableCollection<TiposRecurrencia>();
        }

        public override async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                // Aquí podrías cargar listas para pickers (tipos de evento, recurrencia, medicamentos del usuario, etc.)
                // await CargarTiposEventoAsync();
                // await CargarTiposRecurrenciaAsync();
                // await CargarMedicamentosUsuarioAsync();
            });
        }

        [RelayCommand]
        private async Task CrearEventoAsync()
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(titulo))
            {
                await ShowAlertAsync("Error", "El título es requerido.");
                return;
            }
            if (fechaInicio == default || fechaFin == default)
            {
                await ShowAlertAsync("Error", "Las fechas son requeridas.");
                return;
            }
            if (fechaFin < fechaInicio)
            {
                await ShowAlertAsync("Error", "La fecha fin debe ser posterior o igual a la fecha inicio.");
                return;
            }
            if (idTipoEvento <= 0)
            {
                await ShowAlertAsync("Error", "Selecciona un tipo de evento válido.");
                return;
            }
            if (esRecurrente && (!idTipoRecurrencia.HasValue || idTipoRecurrencia.Value <= 0))
            {
                await ShowAlertAsync("Error", "Selecciona un tipo de recurrencia válido para un evento recurrente.");
                return;
            }
            // Otras validaciones: por ejemplo, si el evento no es recurrente, podrías forzar IdTipoRecurrencia = null.
            // También validar estadoEvento si permites otros estados.
            // Validar idMedicamento si aplica.

            // Crear request
            var request = new ReqInsertarEventoMedico
            {
                IdUsuario = _userId,
                Titulo = titulo.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim(),
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                EsRecurrente = esRecurrente,
                IdTipoEvento = idTipoEvento,
                IdTipoRecurrencia = esRecurrente ? idTipoRecurrencia : null,
                EstadoEvento = estadoEvento, // "pendiente" o el que uses
                IdMedicamento = idMedicamento
            };

            await ExecuteAsync(async () =>
            {
                try
                {
                    Debug.WriteLine($"=== CREANDO EVENTO: {request.Titulo}, Usuario: {request.IdUsuario} ===");
                    var response = await _apiService.InsertarEventoMedicoAsync(request);
                    if (response != null && response.resultado)
                    {
                        // Éxito: limpiar formulario o navegar atrás
                        await ShowAlertAsync("Éxito", "Evento médico creado correctamente.");
                        Debug.WriteLine("Evento creado exitosamente.");

                        // Limpiar campos si deseas permitir más inserciones:
                        titulo = string.Empty;
                        descripcion = string.Empty;
                        fechaInicio = DateTime.Now;
                        fechaFin = DateTime.Now.AddHours(1);
                        esRecurrente = false;
                        idTipoEvento = 0;
                        idTipoRecurrencia = null;
                        estadoEvento = "pendiente";
                        idMedicamento = null;

                        // Opcional: notificar a otra ViewModel o recargar lista de eventos
                        // MessagingCenter.Send(this, "EventoCreado");  // o CommunityToolkit.Mvvm Messenger
                    }
                    else
                    {
                        // Manejar errores devueltos
                        var mensaje = response?.Mensaje ?? "Error al crear evento médico.";
                        if (response?.errores != null && response.errores.Any())
                        {
                            var detalles = string.Join("; ", response.errores.Select(e => e.mensaje));
                            mensaje += $"\nDetalles: {detalles}";
                        }
                        await ShowAlertAsync("Error", mensaje);
                        Debug.WriteLine($"Error al crear evento: {mensaje}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Excepción en CrearEventoAsync: {ex.Message}");
                    await ShowAlertAsync("Error", "Ocurrió un error al crear el evento médico.");
                }
            });
        }



    }
}