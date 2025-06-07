using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class HistorialRecordatorios
    {
        public int id_historial { get; set; }
        public DateTime fecha_accion { get; set; }
        public int id_recordatorio { get; set; }
        public int id_usuario { get; set; }
        public int id_estado { get; set; }
        public Usuarios usuario { get; set; }
        public RecordatoriosMedicacion recordatorio { get; set; }
        public EstadoEnum estado { get; set; }
    }
}
