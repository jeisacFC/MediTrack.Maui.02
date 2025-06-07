using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class FechasRecordatorios
    {
        public int id_fecha { get; set; }
        public int dia { get; set; }
        public int mes { get; set; }
        public int anio { get; set; }
        public int hora { get; set; }
        public int minuto { get; set; }
    }
}
