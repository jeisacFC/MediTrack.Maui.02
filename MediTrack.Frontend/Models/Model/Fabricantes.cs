using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models.Model
{
    public class Fabricantes
    {
        public int id_fabricante { get; set; }
        public string nombre { get; set; }  

        //no lo veo necesario
        public string pais { get; set; }
        public string informacion_contacto { get; set; }
    }
}
