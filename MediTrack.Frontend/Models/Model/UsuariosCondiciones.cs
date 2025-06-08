using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class UsuariosCondiciones
    {
        public int id_usuario {  get; set; }
        public int id_condicion {  get; set; }

        public Usuarios usuario { get; set; }
        public CondicionesMedicas condiciones { get; set; }
    }
}
