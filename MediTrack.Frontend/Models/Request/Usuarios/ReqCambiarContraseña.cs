using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqCambiarContraseña : ReqBase
    {
        public int IdUsuario { get; set; }
        public string ContraseñaAnterior { get; set; }
        public string ContraseñaNueva { get; set; }
    }
    
}
