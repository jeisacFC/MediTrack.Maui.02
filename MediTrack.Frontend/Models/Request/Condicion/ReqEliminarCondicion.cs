using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqEliminarCondicion : ReqBase
    {
        [Required(ErrorMessage = "El ID de la condición es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID debe ser mayor a cero")]
        public int IdCondicion { get; set; }
    }
}
