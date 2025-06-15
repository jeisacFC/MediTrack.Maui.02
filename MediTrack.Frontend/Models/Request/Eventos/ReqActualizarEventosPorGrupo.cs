using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqActualizarEventosPorGrupo : ReqBase
    {
        public Guid GrupoRecurrencia { get; set; }
        public string NuevoTitulo { get; set; }
        public string NuevaDescripcion { get; set; }
        public DateTime? NuevaFechaInicio { get; set; }
        public DateTime? NuevaFechaFin { get; set; }
        public string NuevoEstadoEvento { get; set; }
    }
}
