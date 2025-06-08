using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqObtenerAlergia : ReqBase
    {
        [Required(ErrorMessage = "El ID de alergia es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de alergia debe ser mayor a 0")]
        public int IdAlergia { get; set; }
    }
}
