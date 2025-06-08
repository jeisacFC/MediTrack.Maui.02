using MediTrack.Frontend.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Response
{
    public class ResInteraccionesMedicamentos : ResBase
    {
        public List<string> Interacciones { get; set; }
    }
}
