using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqMarcarEstadoRecordatorio : ReqBase
    {
        public int IdInstancia { get; set; }
        public int IdEstado { get; set; }
        public string Notas { get; set; }
    }
}
