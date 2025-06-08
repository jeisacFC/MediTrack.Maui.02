using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqBuscarSintoma : ReqBase
    {
        public string Filtro { get; set; }
    }
}
