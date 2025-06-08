using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class RecordatoriosMedicacion
    {
        public int id_recordatorio { get; set; }
        public int id_usuario { get; set; }
        public int id_medicamento { get; set; }
        public int id_frecuencia { get; set; }
        public DateTime FechaProgramacion { get; set; }
        public string Mensaje { get; set; }
        public bool Activo { get; set; }
        public ProgramacionRecordatorio Programacion { get; set; }
    }
}
