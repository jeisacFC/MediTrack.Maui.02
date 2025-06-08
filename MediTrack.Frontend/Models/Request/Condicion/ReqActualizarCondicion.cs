using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqActualizarCondicion : ReqBase
    {
        [Required(ErrorMessage = "El ID de la condición es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID debe ser mayor a cero")]
        public int IdCondicion { get; set; }

        [Required(ErrorMessage = "El nombre de la condición es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        public string NombreCondicion { get; set; }

        [StringLength(300, ErrorMessage = "La descripción no puede exceder 300 caracteres")]
        public string Descripcion { get; set; }
    }
}
