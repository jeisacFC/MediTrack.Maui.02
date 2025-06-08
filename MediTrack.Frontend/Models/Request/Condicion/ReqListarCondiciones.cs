using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqListarCondiciones : ReqBase //BOrrar atributos de req listar
    {
        public int Pagina { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 50;
        public string FiltroNombre { get; set; }
    }
}
