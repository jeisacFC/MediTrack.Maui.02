using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqInsertarRecordatorio : ReqBase
    {
        public int IdUsuario { get; set; }
        public int IdMedicamento { get; set; }
        public int IdFrecuencia { get; set; }
        public DateTime FechaProgramacion { get; set; }
        public string Mensaje { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string DiasSemana { get; set; }
        public int? IntervaloDias { get; set; }
        public TimeSpan HoraProgramada { get; set; }
    }

}
