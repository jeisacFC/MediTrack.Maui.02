using MediTrack.Frontend.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResInsertarRecordatorio : ResBase
    {
        public int id_recordatorio { get; set; }
        public int id_programacion { get; set; }
    }
}
