using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class EventoMedicoUsuario
    {
        public int IdEventoMedico { get; set; }
        public int IdUsuario { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool EsRecurrente { get; set; }
        public int IdTipoEvento { get; set; }
        public int? IdTipoRecurrencia { get; set; }
        public string EstadoEvento { get; set; }
        public int? IdMedicamento { get; set; }
        public Guid? GrupoRecurrencia { get; set; }
        public string NombreMedicamento { get; set; }
        public string TipoEvento { get; set; }
        public string TipoRecurrencia { get; set; }
    }
}
