using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class CondicionesMedicas
    {
        public int id_condicion { get; set; }
        public string nombre_condicion { get; set; }
        public string descripcion { get; set; }
        public int? UsuariosAsociados { get; set; }
        public DateTime? FechaDiagnostico { get; set; }
    }
}
