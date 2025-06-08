using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqInsertarAlergia : ReqBase
    {
        [Required(ErrorMessage = "El nombre de la alergia es obligatorio")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre de la alergia debe tener entre 2 y 150 caracteres")]
        public string NombreAlergia { get; set; }

        [StringLength(300, ErrorMessage = "La descripción no puede exceder los 300 caracteres")]
        public string Descripcion { get; set; }
    }
}
