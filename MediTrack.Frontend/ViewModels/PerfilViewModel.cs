using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MediTrack.Modelos;

namespace MediTrack.Vistas.PantallasPrincipales
{
    public class PerfilViewModel
    {
        public ObservableCollection<ElementoSeleccionable> Padecimientos { get; set; }
        public ObservableCollection<ElementoSeleccionable> Alergias { get; set; }

        public PerfilViewModel()
        {
            Padecimientos = new ObservableCollection<ElementoSeleccionable>
            {
                new() { Nombre = "Escoliosis" },
                new() { Nombre = "Diabetes" },
                new() { Nombre = "Hipertensión" }
            };

            Alergias = new ObservableCollection<ElementoSeleccionable>
            {
                new() { Nombre = "Camarones" },
                new() { Nombre = "Maní" },
                new() { Nombre = "Penicilina" }
            };
        }
    }
}