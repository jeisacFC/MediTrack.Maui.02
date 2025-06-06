using System;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqInsertarUsuario
    {
        public string Email { get; set; }
        public string Contraseña { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string IdGenero { get; set; }
        public bool NotificacionesPush { get; set; } = true;
    }
}