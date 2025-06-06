using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Models;

    public class ResBase
    {
        public bool resultado { get; set; }
        public List<Error> errores { get; set; } // Usa tu clase Error.cs existente
        public int Codigo { get; set; }
        public string Mensaje { get; set; }

        public ResBase()
        {
            errores = new List<Error>();
        }
    }
