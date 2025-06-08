using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class ProgramacionRecordatorio
    {
        public int IdProgramacion { get; set; }
        public int IdRecordatorio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string DiasSemana { get; set; }
        public int? IntervaloDias { get; set; }
        public TimeSpan HoraProgramada { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
