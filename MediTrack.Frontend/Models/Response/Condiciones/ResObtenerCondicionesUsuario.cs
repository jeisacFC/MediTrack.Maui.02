using MediTrack.Frontend.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResObtenerCondicionesUsuario : ResBase
    {
        public List<CondicionesMedicas> Condiciones { get; set; }
        public int TotalCondiciones { get { return Condiciones?.Count ?? 0; } }

        public ResObtenerCondicionesUsuario()
        {
            Condiciones = new List<CondicionesMedicas>();
        }
    }
}
