using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class Usuarios
    {
        [JsonProperty("id_usuario")]
        [JsonPropertyName("id_usuario")]
        public int id_usuario { get; set; }

        [JsonProperty("email")]
        [JsonPropertyName("email")]
        public string email { get; set; }

        [JsonProperty("contraseña")]
        [JsonPropertyName("contraseña")]
        public string contraseña { get; set; }

        [JsonProperty("fecha_registro")]
        [JsonPropertyName("fecha_registro")]
        public DateTime fecha_registro { get; set; }

        [JsonProperty("notificaciones_push")]
        [JsonPropertyName("notificaciones_push")]
        public bool notificaciones_push { get; set; }

        [JsonProperty("ultimo_acceso")]
        [JsonPropertyName("ultimo_acceso")]
        public DateTime ultimo_acceso { get; set; }

        [JsonProperty("intentos_fallidos")]
        [JsonPropertyName("intentos_fallidos")]
        public int intentos_fallidos { get; set; }

        [JsonProperty("cuenta_bloqueada")]
        [JsonPropertyName("cuenta_bloqueada")]
        public bool cuenta_bloqueada { get; set; }

        [JsonProperty("nombre")]
        [JsonPropertyName("nombre")]
        public string nombre { get; set; }

        [JsonProperty("apellido1")]
        [JsonPropertyName("apellido1")]
        public string apellido1 { get; set; }

        [JsonProperty("apellido2")]
        [JsonPropertyName("apellido2")]
        public string apellido2 { get; set; }

        [JsonProperty("fecha_nacimiento")]
        [JsonPropertyName("fecha_nacimiento")]
        public DateTime fecha_nacimiento { get; set; }

        [JsonProperty("id_genero")]
        [JsonPropertyName("id_genero")]
        public string id_genero { get; set; }
    }
}
