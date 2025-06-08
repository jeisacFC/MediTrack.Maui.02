using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqAsignarAlergiaUsuario : ReqBase
    {
        public int IdUsuario { get; set; }
        public int IdAlergia { get; set; }
        public DateTime? FechaDiagnostico { get; set; }
    }
}
