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
        [JsonPropertyName("IdUsuario")]
        public int id_usuario { get; set; }

        [JsonPropertyName("IdMedicamento")]
        public int id_medicamento { get; set; }

        [JsonPropertyName("NombreComercial")]
        public string nombre_comercial { get; set; }

        [JsonPropertyName("Dosis")]
        public string dosis { get; set; }

        [JsonPropertyName("FechaGuardado")]
        public DateTime fecha_guardado { get; set; }
    }
}
