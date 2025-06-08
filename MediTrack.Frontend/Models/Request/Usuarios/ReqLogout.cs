using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqLogout : ReqBase
    {
        public int? IdUsuario { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public bool InvalidarTodos { get; set; } = false;
    }
}
