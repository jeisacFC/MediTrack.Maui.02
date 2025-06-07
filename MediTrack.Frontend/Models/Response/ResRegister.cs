using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResRegister : ResBase
    {
        public int IdUsuario { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
