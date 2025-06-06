using System.Collections.ObjectModel;
using MediTrack.Frontend.Models;
using System.Globalization;

namespace MediTrack.Frontend.Services.Implementaciones
{
    public class EventosService
    {
        private static EventosService _instance;
        public static EventosService Instance => _instance ??= new EventosService();

        private Dictionary<DateTime, List<EventoAgenda>> _eventosCache = new();
        private readonly CultureInfo _culturaEspañola = new("es-ES");

        // Eventos para notificar cambios
        public event EventHandler<EventoAgenda> EventoActualizado;
        public event EventHandler<EventoAgenda> EventoEliminado;
        public event EventHandler<EventoAgenda> EventoAgregado;

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
                    Tipo = "Cita médica",
                    Color = "#FF5722",
                    Completado = false
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
                        Color = "#E91E63",
                        Completado = false
                    }
                };
            }
        }

        // ================== MÉTODOS DE CONSULTA ================== //

        /// <summary>
        /// Obtener eventos de una fecha específica ordenados por hora
        /// </summary>
        public List<EventoAgenda> ObtenerEventos(DateTime fecha)
        {
            try
            {
                if (_eventosCache.TryGetValue(fecha.Date, out var eventos))
                {
                    return eventos.OrderBy(e => e.FechaHora).ToList();
                }
                return new List<EventoAgenda>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo eventos: {ex.Message}");
                return new List<EventoAgenda>();
            }
        }

        /// <summary>
        /// Obtener solo medicamentos del día de hoy
        /// </summary>
        public List<EventoAgenda> ObtenerMedicamentosHoy()
        {
            var eventosHoy = ObtenerEventos(DateTime.Today);
            return eventosHoy.Where(e => e.Tipo == "Medicamento").ToList();
        }

        /// <summary>
        /// Obtener eventos por tipo en una fecha específica
        /// </summary>
        public List<EventoAgenda> ObtenerEventosPorTipo(DateTime fecha, string tipo)
        {
            try
            {
                var eventos = ObtenerEventos(fecha);
                return eventos.Where(e => string.Equals(e.Tipo, tipo, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo eventos por tipo: {ex.Message}");
                return new List<EventoAgenda>();
            }
        }

        /// <summary>
        /// Obtener todos los eventos en un rango de fechas
        /// </summary>
        public List<EventoAgenda> ObtenerEventosEnRango(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var eventosEnRango = new List<EventoAgenda>();

                for (var fecha = fechaInicio.Date; fecha <= fechaFin.Date; fecha = fecha.AddDays(1))
                {
                    var eventosDia = ObtenerEventos(fecha);
                    eventosEnRango.AddRange(eventosDia);
                }

                return eventosEnRango.OrderBy(e => e.FechaHora).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo eventos en rango: {ex.Message}");
                return new List<EventoAgenda>();
            }
        }

        /// <summary>
        /// Obtener estadísticas de eventos completados vs pendientes
        /// </summary>
        public (int completados, int pendientes, int total) ObtenerEstadisticas(DateTime fecha)
        {
            try
            {
                var eventos = ObtenerEventos(fecha);
                var completados = eventos.Count(e => e.Completado);
                var pendientes = eventos.Count(e => !e.Completado);
                return (completados, pendientes, eventos.Count);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo estadísticas: {ex.Message}");
                return (0, 0, 0);
            }
        }

        // ================== MÉTODOS DE MODIFICACIÓN ================== //

        /// <summary>
        /// Agregar nuevo evento
        /// </summary>
        public bool AgregarEvento(EventoAgenda evento)
        {
            try
            {
                if (evento == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Evento es null");
                    return false;
                }

                var fecha = evento.FechaHora.Date;

                if (!_eventosCache.ContainsKey(fecha))
                {
                    _eventosCache[fecha] = new List<EventoAgenda>();
                }

                _eventosCache[fecha].Add(evento);

                // Notificar cambios
                EventoAgregado?.Invoke(this, evento);
                EventoActualizado?.Invoke(this, evento);

                System.Diagnostics.Debug.WriteLine($"Evento agregado exitosamente: {evento.Titulo}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando evento: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Eliminar evento específico
        /// </summary>
        public bool EliminarEvento(EventoAgenda evento)
        {
            try
            {
                if (evento == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Evento a eliminar es null");
                    return false;
                }

                var fecha = evento.FechaHora.Date;

                if (_eventosCache.TryGetValue(fecha, out var eventos))
                {
                    bool eliminado = eventos.Remove(evento);

                    if (eliminado)
                    {
                        // Si la lista queda vacía, la removemos del cache
                        if (eventos.Count == 0)
                        {
                            _eventosCache.Remove(fecha);
                        }

                        // Notificar cambios
                        EventoEliminado?.Invoke(this, evento);

                        System.Diagnostics.Debug.WriteLine($"Evento eliminado exitosamente: {evento.Titulo}");
                    }

                    return eliminado;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error eliminando evento: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Actualizar evento completo (elimina el original y agrega el actualizado)
        /// </summary>
        public bool ActualizarEvento(EventoAgenda eventoOriginal, EventoAgenda eventoActualizado)
        {
            try
            {
                if (eventoOriginal == null || eventoActualizado == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Eventos para actualizar son null");
                    return false;
                }

                bool eliminado = EliminarEvento(eventoOriginal);
                if (eliminado)
                {
                    bool agregado = AgregarEvento(eventoActualizado);
                    if (agregado)
                    {
                        System.Diagnostics.Debug.WriteLine($"Evento actualizado exitosamente: {eventoOriginal.Titulo} → {eventoActualizado.Titulo}");
                        return true;
                    }
                    else
                    {
                        // Si falla agregar, intentar restaurar el original
                        AgregarEvento(eventoOriginal);
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando evento: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Actualizar solo el estado de completado de un evento
        /// </summary>
        public void ActualizarEstadoEvento(EventoAgenda evento)
        {
            try
            {
                if (evento == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Evento para actualizar estado es null");
                    return;
                }

                // El evento ya está actualizado en memoria (por referencia)
                // Solo notificar el cambio
                EventoActualizado?.Invoke(this, evento);

                System.Diagnostics.Debug.WriteLine($"Estado de evento actualizado: {evento.Titulo} → {(evento.Completado ? "completado" : "pendiente")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando estado de evento: {ex.Message}");
            }
        }

        // ================== MÉTODOS UTILITARIOS ================== //

        /// <summary>
        /// Buscar eventos por texto en título o descripción
        /// </summary>
        public List<EventoAgenda> BuscarEventos(string textoBusqueda, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textoBusqueda))
                {
                    return new List<EventoAgenda>();
                }

                var eventos = new List<EventoAgenda>();

                // Si no se especifican fechas, buscar en los próximos 30 días
                var inicio = fechaInicio ?? DateTime.Today;
                var fin = fechaFin ?? DateTime.Today.AddDays(30);

                eventos = ObtenerEventosEnRango(inicio, fin);

                // Filtrar por texto
                var textoLower = textoBusqueda.ToLower();
                return eventos.Where(e =>
                    e.Titulo.ToLower().Contains(textoLower) ||
                    e.Descripcion.ToLower().Contains(textoLower) ||
                    e.Tipo.ToLower().Contains(textoLower)
                ).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error buscando eventos: {ex.Message}");
                return new List<EventoAgenda>();
            }
        }

        /// <summary>
        /// Obtener tipos de eventos únicos
        /// </summary>
        public List<string> ObtenerTiposEventos()
        {
            try
            {
                var tipos = new HashSet<string>();

                foreach (var eventosPorFecha in _eventosCache.Values)
                {
                    foreach (var evento in eventosPorFecha)
                    {
                        tipos.Add(evento.Tipo);
                    }
                }

                return tipos.OrderBy(t => t).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo tipos de eventos: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Limpiar caché y reinicializar (para testing o reset)
        /// </summary>
        public void LimpiarCache()
        {
            try
            {
                _eventosCache.Clear();
                InicializarEventos();
                System.Diagnostics.Debug.WriteLine("Cache de eventos limpiado y reinicializado");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error limpiando cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtener total de eventos en cache
        /// </summary>
        public int ContarTotalEventos()
        {
            try
            {
                return _eventosCache.Values.Sum(eventos => eventos.Count);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error contando eventos: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Verificar si existe al menos un evento en una fecha
        /// </summary>
        public bool TieneEventos(DateTime fecha)
        {
            return _eventosCache.ContainsKey(fecha.Date) && _eventosCache[fecha.Date].Count > 0;
        }
    }
}