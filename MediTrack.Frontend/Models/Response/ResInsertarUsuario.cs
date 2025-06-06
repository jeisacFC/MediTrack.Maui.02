using System;

namespace MediTrack.Frontend.Models.Response
{
    public class ResInsertarUsuario : ResBase
    {
        public int IdUsuario { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}