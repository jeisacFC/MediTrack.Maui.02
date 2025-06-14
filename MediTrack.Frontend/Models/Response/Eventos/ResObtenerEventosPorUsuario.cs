using MediTrack.Frontend.Models.Model;

namespace MediTrack.Frontend.Models.Response.Eventos
{
    public class ResObtenerEventosPorUsuario : ResBase
    {
        public List<EventosMedicos> Eventos { get; set; }

    }
}
