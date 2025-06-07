using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResLogout : ResBase
    {
        public bool LogoutExitoso { get; set; }
        public DateTime FechaLogout { get; set; }
        public int TokensInvalidados { get; set; }
    }
}
