using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediTrack.Frontend.Models; // Para ResEscanearMedicamento
using System.Threading.Tasks;
namespace MediTrack.Frontend.Services;

public interface IBarcodeScannerService
{
    Task<ResEscanearMedicamento> ObtenerDatosMedicamentoAsync(string codigoBarras);

}

