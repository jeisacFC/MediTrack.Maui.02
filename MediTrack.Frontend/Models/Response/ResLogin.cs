using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResLogin : ResBase
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiracion { get; set; }
        public string Email { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public DateTime UltimoAcceso { get; set; }
        public string TipoToken { get; set; } = "Bearer";
    }
}
