using MediTrack.Frontend.Models.Model;

namespace MediTrack.Frontend.Models.Response.Eventos
{
    public class ResListarEventosUsuario : ResBase
    {
        public List<EventoMedicoUsuario> Eventos { get; set; }
    }
}
