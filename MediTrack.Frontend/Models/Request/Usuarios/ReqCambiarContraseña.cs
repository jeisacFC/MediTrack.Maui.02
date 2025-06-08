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
        public string ContrasenaAnterior { get; set; }
        public string ContrasenaNueva { get; set; }
    }
    
}
