using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;

namespace MediTrack.Frontend.Models
{
    public class ResEscanearMedicamento
    {
        public int IdMedicamento { get; set; }
        public string NombreComercial { get; set; }
        public string PrincipioActivo { get; set; }
        public string Dosis { get; set; }
        public string Fabricante { get; set; }
        public List<string> Usos { get; set; }
        public List<string> Advertencias { get; set; }
        public EfectosSecundariosCategorizados EfectosSecundarios { get; set; }
        public List<Error> errores { get; set; }
        public bool resultado { get; set; }

        public ResEscanearMedicamento()
        {
            Usos = new List<string>();
            Advertencias = new List<string>();
            EfectosSecundarios = new EfectosSecundariosCategorizados();
            errores = new List<Error>();
        }
    }
}