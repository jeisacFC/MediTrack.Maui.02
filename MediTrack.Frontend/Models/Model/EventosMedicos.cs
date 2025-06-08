using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class EventosMedicos
    {
        public int id_evento { get; set; }
        public DateTime fecha_evento { get; set; }
        public string descripcion { get; set; }
        public string titulo { get; set; }
        public int id_usuario { get; set; }
        public Usuarios usuario { get; set; }
    }
}
