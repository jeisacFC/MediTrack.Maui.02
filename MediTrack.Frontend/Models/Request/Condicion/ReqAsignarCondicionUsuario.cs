using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqAsignarCondicionUsuario : ReqBase
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del usuario debe ser mayor a cero")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El ID de la condición es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la condición debe ser mayor a cero")]
        public int IdCondicion { get; set; }

        public DateTime? FechaDiagnostico { get; set; }
    }
}
