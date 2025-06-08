using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqActualizarUsuario : ReqBase
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string IdGenero { get; set; }
        public bool? NotificacionesPush { get; set; } = true;
    }
    
}
