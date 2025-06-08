using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqActualizarCondicion : ReqBase
    {
        public int IdCondicion { get; set; }
        public string NombreCondicion { get; set; }
        public string Descripcion { get; set; }
    }
}
