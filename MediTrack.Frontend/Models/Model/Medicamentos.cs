using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class Medicamentos
    {
        public int IdMedicamento { get; set; }
        public string Nombre { get; set; }
        public string PrincipioActivo { get; set; }
        public string Dosis { get; set; }
        public string Fabricante { get; set; }
        public List<string> Usos { get; set; } = new List<string>();
        public List<string> Advertencias { get; set; } = new List<string>();
        public List<string> EfectosSecundarios { get; set; } = new List<string>();
    }
}