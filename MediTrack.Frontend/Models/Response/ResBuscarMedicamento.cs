using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediTrack.Frontend.Models.Model;


namespace MediTrack.Frontend.Models.Response;

public class ResBuscarMedicamento : ResBase
{
    public MedicamentoExterno Medicamento { get; set; }


    public List<string> Usos { get; set; }
    public List<string> Advertencias { get; set; }
    public List<string> EfectosSecundarios { get; set; }

    public ResBuscarMedicamento()
    {
        Usos = new List<string>();
        Advertencias = new List<string>();
        EfectosSecundarios = new List<string>();
    }
}