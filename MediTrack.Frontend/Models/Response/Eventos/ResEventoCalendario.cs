using MediTrack.Frontend.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResEventoCalendario : ResBase
    {
        public string Tipo { get; set; } // "Medicacion" o "Evento"
        public DateTime FechaHora { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
    }
}
