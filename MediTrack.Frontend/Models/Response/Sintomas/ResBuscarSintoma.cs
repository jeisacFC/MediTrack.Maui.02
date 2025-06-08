using MediTrack.Frontend.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResBuscarSintoma : ResBase
    {
        public int IdSintoma { get; set; }
        public string Nombre { get; set; }
    }
}
