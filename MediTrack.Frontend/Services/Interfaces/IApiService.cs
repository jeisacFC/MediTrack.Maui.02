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
        // Medicamentos (existentes)
        Task<ResEscanearMedicamento> EscanearMedicamentoAsync(ReqEscanearMedicamento request);
        Task<ResBuscarMedicamento> BuscarMedicamentoManualAsync(ReqBuscarMedicamento request);

        // Autenticación (existentes)
        Task<ResLogin> LoginAsync(ReqLogin request);
        Task<ResLogout> LogoutAsync(ReqLogout request);
        Task<ResRegister> RegisterAsync(ReqRegister request);
        Task<ResObtenerUsuario> GetUserAsync(ReqObtenerUsuario request);

        // ========== NUEVOS MÉTODOS PARA EVENTOS ========== //

        /// Obtener eventos para un usuario (el backend filtra por fecha internamente)
        Task<ResListarEventosCalendario> ObtenerEventosAsync(ReqObtenerUsuario request);

        /// Insertar nuevo evento médico
        Task<ResInsertarEventoMedico> InsertarEventoAsync(ReqInsertarEventoMedico request);

        /// Actualizar evento existente
        Task<ResActualizarEventoMedico> ActualizarEventoAsync(ReqActualizarEventoMedico request);

        /// Marcar evento como completado/no completado
        Task<ResCompletarEvento> CompletarEventoAsync(ReqEvento request);

        /// Eliminar evento del usuario
        Task<ResEliminarEventoUsuario> EliminarEventoAsync(ReqEvento request);
    }
}