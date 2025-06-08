using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqGuardarMedicamento : ReqBase
    {
        public int IdUsuario { get; set; }
        public string NombreComercial { get; set; }
        public string PrincipioActivo { get; set; }
        public string Dosis { get; set; }
        public string Fabricante { get; set; }
        public List<string> Usos { get; set; }
        public List<string> Advertencias { get; set; }
        public List<string> EfectosSecundarios { get; set; }
        public int IdMetodoEscaneo { get; set; }
    }
}
