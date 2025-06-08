using MediTrack.Frontend.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResListarMedicamentosUsuario : ResBase
    {
        public List<UsuarioMedicamentos> Medicamentos { get; set; } = new List<UsuarioMedicamentos>();
    }
}
