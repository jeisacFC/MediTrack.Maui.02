using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;

namespace MediTrack.Frontend.Services.Interfaces
{
    public interface IApiService
    {
        // Medicamentos
        Task<ResEscanearMedicamento> EscanearMedicamentoAsync(ReqEscanearMedicamento request);
        Task<ResBuscarMedicamento> BuscarMedicamentoManualAsync(ReqBuscarMedicamento request);
        Task<ResGuardarMedicamento> GuardarMedicamentoAsync(ReqGuardarMedicamento request);
        Task<ResListarMedicamentosUsuario> ListarMedicamentosUsuarioAsync(ReqObtenerUsuario request);
        Task<ResDetalleMedicamentoUsuario> ObtenerDetalleMedicamentoUsuarioAsync(ReqMedicamento request);
        Task<ResEliminarMedicamentoUsuario> EliminarMedicamentoUsuarioAsync(ReqMedicamento request);


        // Autenticación
        Task<ResLogin> LoginAsync(ReqLogin request);
        Task<ResLogout> LogoutAsync(ReqLogout request);
        Task<ResRegister> RegisterAsync(ReqRegister request);
        Task<ResObtenerUsuario> GetUserAsync(ReqObtenerUsuario request);
        Task<ResObtenerAlergiasUsuario> ObtenerAlergiasUsuarioAsync(ReqObtenerAlergiasUsuario request);
        Task<ResObtenerCondicionesUsuario> ObtenerCondicionesMedicasAsync(ReqObtenerCondicionesUsuario request);
    }
}