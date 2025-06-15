using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MediTrack.Frontend.Models.Model
{
    public class EventosMedicos
    {
        public int id_evento_medico { get; set; }
        public int id_usuario { get; set; }
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin { get; set; }
        public bool es_recurrente { get; set; }
        public int id_tipo_evento { get; set; }
        public int id_tipo_recurrencia { get; set; }
        public string estado_evento { get; set; }
        public int id_medicamento { get; set; } = 0;
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
    }
}
