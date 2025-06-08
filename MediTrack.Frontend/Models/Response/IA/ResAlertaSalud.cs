using MediTrack.Frontend.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class AlertaSalud
    {
        public string Riesgo { get; set; }
        public string Causa { get; set; }
        public string Recomendacion { get; set; }
    }
    public class ResAlertaSalud : ResBase
    {
        public List<AlertaSalud> Alertas { get; set; } = new List<AlertaSalud>();
    }
}
