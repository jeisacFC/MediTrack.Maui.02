using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request.Eventos
{
    public class ReqActualizarEstadoEvento : ReqBase
    {
        public int IdEventoMedico { get; set; }
        public string EstadoEvento { get; set; }
    }
}
