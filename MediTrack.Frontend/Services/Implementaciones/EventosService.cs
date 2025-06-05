using System.Collections.ObjectModel;
using MediTrack.Frontend.Models;
using System.Globalization;

using MediTrack;
using MediTrack.Frontend;
using MediTrack.Frontend.Services;
using MediTrack.Frontend.Services.Implementaciones;

namespace MediTrack.Frontend.Services.Implementaciones
{
    public class EventosService
    {
        private static EventosService _instance;
        public static EventosService Instance => _instance ??= new EventosService();

        private Dictionary<DateTime, List<EventoAgenda>> _eventosCache = new();
        private readonly CultureInfo _culturaEspañola = new("es-ES");

        // Evento para notificar cambios
        public event EventHandler<EventoAgenda> EventoActualizado;

        private EventosService()
        {
            InicializarEventos();
        }

        private void InicializarEventos()
        {
            _eventosCache.Clear();

            // Eventos para HOY
            _eventosCache[DateTime.Today] = new List<EventoAgenda>
            {
                new EventoAgenda
                {
                    Titulo = "Omeprazol 20mg",
                    Descripcion = "En ayunas",
                    FechaHora = DateTime.Today.AddHours(7),
                    Tipo = "Medicamento",
                    Color = "#2196F3",
                    Completado = false
                },
                new EventoAgenda
                {
                    Titulo = "Paracetamol 500mg",
                    Descripcion = "Con alimentos",
                    FechaHora = DateTime.Today.AddHours(8),
                    Tipo = "Medicamento",
                    Color = "#4CAF50",
                    Completado = false
                }
            };

            // Eventos para MAÑANA
            _eventosCache[DateTime.Today.AddDays(1)] = new List<EventoAgenda>
            {
                new EventoAgenda
                {
                    Titulo = "Cita médica",
                    Descripcion = "Dr. González - Chequeo general",
                    FechaHora = DateTime.Today.AddDays(1).AddHours(10),
                    Tipo = "Cita",
                    Color = "#FF5722"
                }
            };

            // Eventos para el día 15
            var fecha15 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 15);
            if (fecha15 >= DateTime.Today)
            {
                _eventosCache[fecha15] = new List<EventoAgenda>
                {
                    new EventoAgenda
                    {
                        Titulo = "Análisis de sangre",
                        Descripcion = "Laboratorio - En ayunas",
                        FechaHora = fecha15.AddHours(8),
                        Tipo = "Análisis",
                        Color = "#E91E63"
                    }
                };
            }
        }

        // Obtener eventos de una fecha
        public List<EventoAgenda> ObtenerEventos(DateTime fecha)
        {
            if (_eventosCache.TryGetValue(fecha.Date, out var eventos))
            {
                return eventos.OrderBy(e => e.FechaHora).ToList();
            }
            return new List<EventoAgenda>();
        }

        // Obtener solo medicamentos de hoy
        public List<EventoAgenda> ObtenerMedicamentosHoy()
        {
            var eventosHoy = ObtenerEventos(DateTime.Today);
            return eventosHoy.Where(e => e.Tipo == "Medicamento").ToList();
        }

        // Agregar nuevo evento
        public void AgregarEvento(EventoAgenda evento)
        {
            var fecha = evento.FechaHora.Date;

            if (!_eventosCache.ContainsKey(fecha))
            {
                _eventosCache[fecha] = new List<EventoAgenda>();
            }

            _eventosCache[fecha].Add(evento);
            EventoActualizado?.Invoke(this, evento);
        }

        // Actualizar estado de un evento
        public void ActualizarEstadoEvento(EventoAgenda evento)
        {
            // El evento ya está actualizado en memoria (por referencia)
            // Solo notificar el cambio
            EventoActualizado?.Invoke(this, evento);

            System.Diagnostics.Debug.WriteLine($"Evento {evento.Titulo} actualizado a {(evento.Completado ? "completado" : "pendiente")}");
        }

        // Limpiar caché (para testing)
        public void LimpiarCache()
        {
            _eventosCache.Clear();
            InicializarEventos();
        }
    }
}