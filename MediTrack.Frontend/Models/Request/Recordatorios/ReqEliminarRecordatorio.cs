using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Frontend.Models.Request
{
    public class ReqEliminarRecordatorio : ReqBase
    {
        public int IdRecordatorio { get; set; }
    }
}
