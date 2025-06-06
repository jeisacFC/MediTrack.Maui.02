using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;

namespace MediTrack.Frontend.Models.Response
{
    public class ResEscanearMedicamento : ResBase
    {
        public int IdMedicamento { get; set; }
        public string NombreComercial { get; set; }
        public string PrincipioActivo { get; set; }
        public string Dosis { get; set; }
        public string Fabricante { get; set; }
        public EfectosSecundariosCategorizados EfectosSecundarios { get; set; }
        public List<string> Advertencias { get; set; }
        public List<string> Usos { get; set; }

        public ResEscanearMedicamento()
        {
            EfectosSecundarios = new EfectosSecundariosCategorizados();
            Advertencias = new List<string>();
            Usos = new List<string>();
        }

    }
}
