using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class MedicamentoExterno
    {
        public int IdMedicamento { get; set; }
        public string CodigoBarras { get; set; }
        public string NombreComercial { get; set; }
        public string PrincipioActivo { get; set; }
        public string Dosis { get; set; }
        public string Fabricante { get; set; }
    }
}
