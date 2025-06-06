using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqLogin : ReqBase
    {
        public string Email { get; set; }
        public string Contraseña { get; set; }
        public bool RecordarSesion { get; set; } = false;
    }
}
