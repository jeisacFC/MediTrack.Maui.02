using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models
{
    public class ReqEscanearMedicamento
    {
        public string CodigoBarras { get; set; }
        public int IdUsuario { get; set; }
        public int IdMetodoEscaneo { get; set; }
    }
}