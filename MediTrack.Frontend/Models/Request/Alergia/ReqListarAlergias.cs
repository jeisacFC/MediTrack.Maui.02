using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqListarAlergias : ReqBase
    {
        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")]
        public int Pagina { get; set; } = 1;

        [Range(1, 1000, ErrorMessage = "Los registros por página deben estar entre 1 y 1000")]
        public int RegistrosPorPagina { get; set; } = 50;

        [StringLength(150, ErrorMessage = "El filtro de nombre no puede exceder los 150 caracteres")]
        public string FiltroNombre { get; set; }
    }
}
