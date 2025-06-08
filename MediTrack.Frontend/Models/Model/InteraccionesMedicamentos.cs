using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class InteraccionesMedicamentos
    {
        public int id_interaccion { get; set; }
        public int id_medicamento1 { get; set; }
        public int id_medicamento2 { get; set; }
        public string descripcion_interaccion { get; set; }
        public Medicamentos medicamento { get; set; }
    }
}
