using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqBuscarMedicamento : ReqBase
    {
        public string NombreMedicamento { get; set; }
        public string PrincipioActivo { get; set; }
        public string Dosis { get; set; }
    }
}

