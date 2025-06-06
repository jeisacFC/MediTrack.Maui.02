using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

<<<<<<< Updated upstream
=======
using System.Collections.Generic;

>>>>>>> Stashed changes
namespace MediTrack.Frontend.Models.Response
{
    public class ResEscanearMedicamento : ResBase
    {
        public int IdMedicamento { get; set; }
        public string NombreComercial { get; set; }
        public string PrincipioActivo { get; set; }
        public string Dosis { get; set; }
        public string Fabricante { get; set; }
        public List<string> EfectosSecundarios { get; set; }
        public List<string> Advertencias { get; set; }
        public List<string> Usos { get; set; }


        // Constructor para inicializar las listas y evitar errores de "null"
        public ResEscanearMedicamento()
        {
            EfectosSecundarios = new List<string>();
            Advertencias = new List<string>();
            Usos = new List<string>();
           
        }
    }
}
