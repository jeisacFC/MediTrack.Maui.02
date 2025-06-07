using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class Errores
    {
        public int codigo_error { get; set; }
        public string mensaje { get; set; }
        DateTime fecha_error { get; set; }
        public string stack_trace { get; set; }
        public int id_usuario { get; set; }
        public int id_sesion { get; set; }
        public string origen { get; set; }
    }
}
