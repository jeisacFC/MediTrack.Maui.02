using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqAutenticarUsuario : ReqBase
    {
        public string email { get; set; }
        public string contrasena { get; set; }
    }
}
