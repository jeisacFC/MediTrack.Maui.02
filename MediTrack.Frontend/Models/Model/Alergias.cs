using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class Alergias
    {
        public int id_alergia { get; set; }
        public string nombre_alergia { get; set; }
        public string descripcion { get; set; }
        public int? UsuariosAsociados { get; set; }
        public DateTime? FechaDiagnostico { get; set; }
    }
}
