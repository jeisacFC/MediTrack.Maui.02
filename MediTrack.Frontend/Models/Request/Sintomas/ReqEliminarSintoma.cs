using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqEliminarSintoma : ReqBase
    {
        public int IdUsuario { get; set; }
        public int? IdSintomaManual { get; set; }
        public int? IdSintomaEnum { get; set; }
    }
}
