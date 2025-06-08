using MediTrack.Frontend.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResActualizarCondicion : ResBase
    {
        public int IdCondicion { get; set; }
        public string NombreCondicion { get; set; }
        public string Descripcion { get; set; }
    }
}
