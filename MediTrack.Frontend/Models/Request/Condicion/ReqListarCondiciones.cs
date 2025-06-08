using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqListarCondiciones : ReqBase //BOrrar atributos de req listar
    {
        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a cero")]
        public int Pagina { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Los registros por página deben estar entre 1 y 100")]
        public int RegistrosPorPagina { get; set; } = 50;

        [StringLength(150, ErrorMessage = "El filtro de nombre no puede exceder 150 caracteres")]
        public string FiltroNombre { get; set; }
    }
}
