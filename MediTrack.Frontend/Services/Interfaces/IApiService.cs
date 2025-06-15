using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Models;

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

        // IA
        Task<ResHabitosSaludables> ObtenerHabitosAsync(ReqObtenerUsuario request);
        Task<ResRecomendacionesIA> ObtenerRecomendacionesAsync(ReqObtenerUsuario request);
        Task<ResInteraccionesMedicamentos?> ObtenerInteraccionesAsync(ReqObtenerUsuario request);
        Task<ResAlertaSalud?> ObtenerAlertasSaludAsync(ReqObtenerUsuario request);

        // Eventos Médicos
        Task<ResInsertarEventoMedico> InsertarEventoMedicoAsync(ReqInsertarEventoMedico request);
        Task<ResObtenerEventosPorUsuario> ObtenerEventosPorUsuarioAsync(ReqObtenerEventosPorUsuario request);
        Task<ResListarEventosUsuario> ListarEventosUsuarioAsync(ReqListarEventosUsuario request);
        Task<ResActualizarEventoMedico> ActualizarEventoMedicoAsync(ReqActualizarEventoMedico request);
        Task<ResEliminarEventoMedico> EliminarEventoMedicoAsync(ReqEliminarEventoMedico request);
        Task<ResActualizarEventosPorGrupo> ActualizarEventosPorGrupoRecurrenciaAsync(ReqActualizarEventosPorGrupo request);
        Task<ResEliminarEventosPorGrupo> EliminarEventosPorGrupoRecurrenciaAsync(ReqEliminarEventosPorGrupo request);
        Task<ResActualizarEstadoEvento> ActualizarEstadoEventoAsync(ReqActualizarEstadoEvento request);

        // Autenticación
        Task<ResLogin> LoginAsync(ReqLogin request);
        Task<ResLogout> LogoutAsync(ReqLogout request);
        Task<ResRegister> RegisterAsync(ReqRegister request);
        Task<ResObtenerUsuario> GetUserAsync(ReqObtenerUsuario request);
        Task<ResActualizarUsuario> ActualizarUsuarioAsync(ReqActualizarUsuario request);

        //Condiciones y Alergias
        Task<ResInsertarAlergia> InsertarAlergiaAsync(ReqInsertarAlergia request);
        Task<ResInsertarCondicion> InsertarCondicionAsync(ReqInsertarCondicion request);
        Task<ResListarCondiciones> ListarCondicionesMedicasAsync(ReqListarCondiciones request);
        Task<ResListarAlergias> ListarAlergiasAsync(ReqListarAlergias request);
        Task<ResObtenerAlergiasUsuario> ObtenerAlergiasUsuarioAsync(ReqObtenerAlergiasUsuario request);
        Task<ResObtenerCondicionesUsuario> ObtenerCondicionesMedicasAsync(ReqObtenerCondicionesUsuario request);
        Task<ResAsignarCondicionUsuario> AsignarCondicionUsuarioAsync(ReqAsignarCondicionUsuario request);
        Task<ResAsignarAlergiaUsuario> AsignarAlergiaUsuarioAsync(ReqAsignarAlergiaUsuario request);
        Task<ResDesasignarAlergiaUsuario> DesasignarAlergiaUsuarioAsync(ReqDesasignarAlergiaUsuario request);
        Task<ResDesasignarCondicionUsuario> DesasignarCondicionUsuarioAsync(ReqDesasignarCondicionUsuario request);

        // Recuperación de contraseña
        Task<ResSolicitarResetPassword> SolicitarResetPasswordAsync(ReqSolicitarResetPassword request);
        Task<ResRestablecerContrasena> RestablecerContrasenaAsync(ReqRestablecerContrasena request);

        // Síntomas - Métodos limpios con JWT
        Task<List<ResBuscarSintoma>> BuscarSintomasAsync(ReqBuscarSintoma request);
        Task<ResObtenerSintomasUsuario> ObtenerTodosLosSintomasAsync();
        Task<ResInsertarSintomaManual> InsertarSintomaManualAsync(ReqInsertarSintomaManual request);
        Task<ResAgregarSintomasSeleccionados> AgregarSintomasSeleccionadosAsync(ReqAgregarSintomasSeleccionados request);
        Task<ResObtenerSintomasUsuario> ObtenerSintomasUsuarioAsync(ReqObtenerUsuario request);
        Task<ResEliminarSintoma> EliminarSintomaUsuarioAsync(ReqEliminarSintoma request);
    }
}