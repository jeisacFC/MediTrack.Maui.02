using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request.Eventos
{
    public class ReqEliminarEventoMedico : ReqBase
    {
        public int IdEventoMedico { get; set; }
    }
}
