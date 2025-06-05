using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// using Newtonsoft.Json; // Solo si vas a usar [JsonProperty] y recibes JSON con nombres diferentes
using System.Collections.Generic;

namespace MediTrack.Frontend.Models
{
    public class EfectosSecundariosCategorizados
    {
        // Si el JSON de Gemini usa "Leve", "Moderado", "Grave" (con mayúscula inicial)
        // y las propiedades de tu clase también (lo cual es estándar en C#),
        // Newtonsoft.Json a menudo mapea correctamente sin [JsonProperty] si la configuración
        // del deserializador es insensible a mayúsculas/minúsculas o si coinciden.
        // Por ahora, para el mock service, no es crucial el [JsonProperty].
        public List<string> Leve { get; set; } = new List<string>();
        public List<string> Moderado { get; set; } = new List<string>();
        public List<string> Grave { get; set; } = new List<string>();
    }
}