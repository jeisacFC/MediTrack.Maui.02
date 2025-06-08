using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqActualizarAlergia : ReqBase
    {

        [Required(ErrorMessage = "El ID de alergia es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de alergia debe ser mayor a 0")]
        public int IdAlergia { get; set; }

        [Required(ErrorMessage = "El nombre de la alergia es obligatorio")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre de la alergia debe tener entre 2 y 150 caracteres")]
        public string NombreAlergia { get; set; }

        [StringLength(300, ErrorMessage = "La descripción no puede exceder los 300 caracteres")]
        public string Descripcion { get; set; }
    }
}