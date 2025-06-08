using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class MedicamentoEfectos
    {
        public int id_efecto { get; set; }
        public int id_medicamento { get; set; }
        public Medicamentos medicamento { get; set; }
        public EfectosSecundarios efecto { get; set; }
    }
}
