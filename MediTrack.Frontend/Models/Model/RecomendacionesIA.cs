using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class RecomendacionesIA
    {
        public int id_recomendacion { get; set; }
        public string consulta { get; set; }
        public string respuesta { get; set; }
        public int id_usuario { get; set; }
        public Usuarios usuario { get; set; }
        public int id_medicamento { get; set; }
        public Medicamentos medicamento { get; set; }
        public string sugerencia_habitos { get; set; }
        public string alerta { get; set; }
        public DateTime fecha_consulta { get; set; }
    }
}
