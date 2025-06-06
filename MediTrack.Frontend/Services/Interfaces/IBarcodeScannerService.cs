using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using MediTrack.Frontend.Models.Response;
namespace MediTrack.Frontend.Services.Interfaces;

public interface IBarcodeScannerService
{
    Task<ResEscanearMedicamento> ObtenerDatosMedicamentoAsync(string codigoBarras);

}

