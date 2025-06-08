using MediTrack.Frontend.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResAutenticarUsuario : ResBase
    {
        public int IdUsuario { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public DateTime UltimoAcceso { get; set; }
        public bool UsuarioValido { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; } // Para renovar tokens
        public DateTime TokenExpiration { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}
