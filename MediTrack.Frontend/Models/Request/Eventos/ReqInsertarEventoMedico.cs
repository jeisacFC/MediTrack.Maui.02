using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqInsertarEventoMedico : ReqBase
    {
        public int IdUsuario { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaEvento { get; set; }
        public DateTime? FechaRecordatorio { get; set; }
    }
}
