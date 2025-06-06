using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

<<<<<<< Updated upstream
namespace MediTrack.Frontend.Models.Request;

    public class ReqEscanearMedicamento : ReqBase
=======
namespace MediTrack.Frontend.Models.Request
{
    public class ReqEscanearMedicamento
>>>>>>> Stashed changes
    {
        public string CodigoBarras { get; set; }
        public int IdUsuario { get; set; }
        public int IdMetodoEscaneo { get; set; }
    }

