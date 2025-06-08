using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class Usuarios
    {
        public int id_usuario { get; set; }
        public string email { get; set; }
        public string contrasena { get; set; }
        public DateTime fecha_registro { get; set; }
        public bool notificaciones_push { get; set; }
        public  DateTime ultimo_acceso { get; set; }
        public int intentos_fallidos { get; set; }
        public bool cuenta_bloqueada { get; set; }
        public string nombre { get; set; }
        public string apellido1 { get; set; }
        public string apellido2 { get; set; }
        public DateTime fecha_nacimiento { get; set; }
        public string id_genero { get; set; }


    }
}
