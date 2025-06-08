using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Entidades.Entity
{
    public class Sesiones
    {
        public int id_sesion { get; set; }
        public int id_usuario { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin { get; set; }
        public string origen { get; set; }
        public int id_estado_sesion { get; set; }  
    }
}
