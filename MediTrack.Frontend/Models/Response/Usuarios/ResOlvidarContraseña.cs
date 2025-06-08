using MediTrack.Frontend.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResOlvidarContraseña : ResBase
    {
        public string Email { get; set; }
        public bool EmailEnviado { get; set; }
        public DateTime? FechaEnvio { get; set; }
    }
}
