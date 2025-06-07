using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class EfectosSecundarios
    {
        public int id_efecto { get; set; }
        public string nombre_efecto { get; set; }
        public string descripcion { get; set; }
        public int id_severidad { get; set; }

        public SeveridadEnum severidad { get; set; }
    }
}
