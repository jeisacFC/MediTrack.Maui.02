using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class UsuarioMedicamentos
    {
        
        public int id_usuario { get; set; }

        public int id_medicamento { get; set; }

        public string nombre_comercial { get; set; }

        public string dosis { get; set; }

        public DateTime fecha_guardado { get; set; }
    }
}
