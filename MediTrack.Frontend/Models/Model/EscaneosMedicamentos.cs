using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class EscaneosMedicamentos
    {
        public int id_escaneo { get; set; }
        public DateTime fecha_escaneo { get; set; }
        public string codigo_barras { get; set; }
        public int id_medicamento { get; set; }
        public int id_usuario { get; set; }
        public int id_metodo { get; set; }

        public Usuarios usuario { get; set; }
        public Medicamentos medicamento { get; set; }
        public MetodoEscaneoEnum metodo { get; set; }
    }
}
