using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Popups;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class InicioViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<EventosMedicos> medicamentosHoy = new();

        [ObservableProperty]
        private ObservableCollection<EscaneoReciente> escaneosRecientes = new();

        [ObservableProperty]
        private ObservableCollection<HabitoSaludable> habitosSaludables = new();

        [ObservableProperty]
        private ObservableCollection<RecomendacionesIA> recomendaciones = new();

        [ObservableProperty]
        private ObservableCollection<Interaccion> interacciones = new();

        [ObservableProperty]
        private ObservableCollection<AlertaSalud> alertas = new();

        [ObservableProperty]
        private ObservableCollection<SintomaUsuario> sintomasUsuario = new();

        [ObservableProperty]
        private string fechaHoy = "";

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool hayMedicamentos = false;

        [ObservableProperty]
        private bool hayEscaneos = false;

        [ObservableProperty]
        private bool hayHabitos = false;

        [ObservableProperty]
        private bool hayRecomendaciones = false;

        [ObservableProperty]
        private bool hayInteracciones = false;

        [ObservableProperty]
        private bool hayAlertas = false;

        [ObservableProperty]
        private bool haySintomas = false;

        private readonly ApiService _eventosService;
        private readonly CultureInfo _culturaEspañola = new("es-ES");
        private readonly int _idUsuarioActual = 1; // TODO: Obtener del servicio de autenticación

        private readonly IApiService _apiService;
        public IAsyncRelayCommand CargarHabitosCommand { get; }
        public IAsyncRelayCommand CargarRecomendacionesCommand { get; }
        public IAsyncRelayCommand CargarInteraccionesCommand { get; }
        public IAsyncRelayCommand CargarAlertasCommand { get; }

        public InicioViewModel(IApiService apiService)
        {
            try
            {
                _apiService = apiService;
                CargarHabitosCommand = new AsyncRelayCommand(CargarHabitosSaludables);
                CargarRecomendacionesCommand = new AsyncRelayCommand(CargarRecomendaciones);
                CargarInteraccionesCommand = new AsyncRelayCommand(CargarInteracciones);
                CargarAlertasCommand = new AsyncRelayCommand(CargarAlertasSalud);

                // Usar servicios
                _apiService = apiService;

                // Suscribirse a cambios en medicamentos REVISAR
                //_eventosService.EventoActualizado += OnEventoActualizado;

                // Configurar cultura española
                CultureInfo.CurrentCulture = _culturaEspañola;
                CultureInfo.CurrentUICulture = _culturaEspañola;

                ActualizarFechaHoy();

                // NO cargar datos inmediatamente para evitar bloquear la UI
                System.Diagnostics.Debug.WriteLine("InicioViewModel inicializado - datos se cargarán en OnAppearing");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en InicioViewModel: {ex.Message}");
            }
        }

        private async Task CargarDatosIniciales()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("Iniciando carga de datos...");

                // Cargar datos en el orden deseado: Medicamentos -> Síntomas -> resto
                //await CargarMedicamentosHoy();
                await CargarSintomasUsuario();
                await CargarEscaneosRecientes();

                // Pequeña pausa antes de cargar datos de IA
                await Task.Delay(100);

                await CargarHabitosSaludables();
                await CargarRecomendaciones();
                await CargarInteracciones();
                await CargarAlertasSalud();

                System.Diagnostics.Debug.WriteLine("Todos los datos cargados exitosamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos iniciales: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnEventoActualizado(object sender, EventosMedicos evento)
        {
            // Recargar medicamentos cuando hay cambios
            if (evento.id_evento_medico == 2 && (evento.fecha_inicio == DateTime.Today || evento.fecha_fin == DateTime.Today))
            {
                System.Diagnostics.Debug.WriteLine($"Evento actualizado: {evento.titulo} - recargando medicamentos");
                //_ = CargarMedicamentosHoy();
            }
        }

        private void ActualizarFechaHoy()
        {
            try
            {
                FechaHoy = DateTime.Today.ToString("dddd, dd 'de' MMMM", _culturaEspañola);

                // Capitalizar primera letra
                if (!string.IsNullOrEmpty(FechaHoy))
                {
                    FechaHoy = char.ToUpper(FechaHoy[0]) + FechaHoy[1..];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando fecha: {ex.Message}");
                FechaHoy = DateTime.Today.ToString("dd/MM/yyyy");
            }
        }

        /*private async Task CargarMedicamentosHoy()
        {
            try
            {
                // Limpiar lista
                MedicamentosHoy.Clear();

                // Usar servicio local (EventosService) por ahora
                var medicamentos = _eventosService.ObtenerMedicamentosHoy();

                foreach (var medicamento in medicamentos)
                {
                    MedicamentosHoy.Add(medicamento);
                }

                HayMedicamentos = MedicamentosHoy.Any();

                System.Diagnostics.Debug.WriteLine($"Cargados {MedicamentosHoy.Count} medicamentos para hoy");

                // TODO: Aquí conectar con el backend real
                // var response = await _apiService.ListarMedicamentosUsuarioAsync(_idUsuarioActual);
                // if (response?.EsExitoso == true) { ... }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando medicamentos: {ex.Message}");
                HayMedicamentos = false;
            }
        }*/

        private async Task CargarEscaneosRecientes()
        {
            try
            {
                Debug.WriteLine("[VM] Iniciando carga de escaneos recientes...");

                // Limpiar lista
                EscaneosRecientes.Clear();

                // TODO: Conectar con backend real
                // Por ahora, datos de ejemplo
                var escaneosEjemplo = new List<EscaneoReciente>
                {
                    new EscaneoReciente
                    {
                        NombreComercial = "Divalproato Sódico 250mg",
                        FechaEscaneo = DateTime.Now.AddDays(-2),
                        Fabricante = "Laboratorio ABC"
                    },
                    new EscaneoReciente
                    {
                        NombreComercial = "Amoxicilina 500mg",
                        FechaEscaneo = DateTime.Now.AddDays(-3),
                        Fabricante = "Pharma XYZ"
                    },
                    new EscaneoReciente
                    {
                        NombreComercial = "Ibuprofeno 400mg",
                        FechaEscaneo = DateTime.Now.AddDays(-5),
                        Fabricante = "Medicina S.A."
                    }
                };

                foreach (var escaneo in escaneosEjemplo)
                {
                    EscaneosRecientes.Add(escaneo);
                }

                HayEscaneos = EscaneosRecientes.Any();

                Debug.WriteLine($"[VM] Cargados {EscaneosRecientes.Count} escaneos recientes - HayEscaneos: {HayEscaneos}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Error cargando escaneos recientes: {ex.Message}");
                HayEscaneos = false;
            }
        }

        private async Task CargarHabitosSaludables()
        {
            try
            {
                HabitosSaludables.Clear();
                var userIdStr = await SecureStorage.GetAsync("user_id");
                Debug.WriteLine($"[VM] user_id en SecureStorage: {userIdStr}");

                if (!int.TryParse(userIdStr, out var idUsuario))
                {
                    Debug.WriteLine("[VM] Usuario no autenticado para hábitos");
                    HayHabitos = false;
                    return;
                }

                var req = new ReqObtenerUsuario { IdUsuario = idUsuario };
                Debug.WriteLine($"[VM] Enviando ReqObtenerUsuario.IdUsuario = {req.IdUsuario}");

                var res = await _apiService.ObtenerHabitosAsync(req);
                Debug.WriteLine($"[VM] Respuesta hábitos: resultado={res?.resultado} count={res?.Habitos?.Count}");

                if (res?.Habitos != null)
                {
                    foreach (var texto in res.Habitos)
                    {
                        Debug.WriteLine($"[VM] Añadiendo hábito: {texto}");
                        HabitosSaludables.Add(new HabitoSaludable
                        {
                            Titulo = "",
                            Descripcion = texto,
                        });
                    }
                }
                else
                {
                    Debug.WriteLine("[VM] No se encontraron hábitos saludables.");
                }

                HayHabitos = HabitosSaludables.Any();
                Debug.WriteLine($"[VM] HayHabitos = {HayHabitos}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar hábitos: {ex.Message}");
                HayHabitos = false;
            }
        }

        private async Task CargarRecomendaciones()
        {
            try
            {
                Recomendaciones.Clear();

                var userIdStr = await SecureStorage.GetAsync("user_id");
                Debug.WriteLine($"[VM] user_id para recomendaciones: {userIdStr}");
                if (!int.TryParse(userIdStr, out var idUsuario))
                {
                    Debug.WriteLine("[VM] Usuario no autenticado para recomendaciones");
                    HayRecomendaciones = false;
                    return;
                }

                var req = new ReqObtenerUsuario { IdUsuario = idUsuario };
                Debug.WriteLine($"[VM] Solicitando recomendaciones para {idUsuario}");
                var res = await _apiService.ObtenerRecomendacionesAsync(req);
                Debug.WriteLine($"[VM] Respuesta recomendaciones: resultado={res?.resultado} count={res?.Recomendaciones?.Count}");

                if (res?.Recomendaciones != null)
                {
                    foreach (var texto in res.Recomendaciones)
                    {
                        Debug.WriteLine($"[VM] Agregando recomendación: {texto}");
                        Recomendaciones.Add(new RecomendacionesIA
                        {
                            Titulo = "",
                            Descripcion = texto
                        });
                    }
                }

                HayRecomendaciones = Recomendaciones.Any();
                Debug.WriteLine($"[VM] HayRecomendaciones = {HayRecomendaciones}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Error al cargar recomendaciones: {ex}");
                HayRecomendaciones = false;
            }
        }

        private async Task CargarInteracciones()
        {
            try
            {
                Interacciones.Clear();

                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (!int.TryParse(userIdStr, out var idUsuario))
                {
                    Debug.WriteLine("[VM] Usuario no autenticado para interacciones");
                    HayInteracciones = false;
                    return;
                }

                var req = new ReqObtenerUsuario { IdUsuario = idUsuario };
                Debug.WriteLine($"[VM] Solicitando interacciones para {idUsuario}");
                var res = await _apiService.ObtenerInteraccionesAsync(req);
                Debug.WriteLine($"[VM] Respuesta interacciones: resultado={res?.resultado} count={res?.Interacciones?.Count}");

                if (res?.Interacciones != null)
                {
                    foreach (var texto in res.Interacciones)
                    {
                        Debug.WriteLine($"[VM] Agregando interacción: {texto}");
                        Interacciones.Add(new Interaccion
                        {
                            Titulo = "",
                            Descripcion = texto
                        });
                    }
                }

                HayInteracciones = Interacciones.Any();
                Debug.WriteLine($"[VM] HayInteracciones = {HayInteracciones}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Error al cargar interacciones: {ex}");
                HayInteracciones = false;
            }
        }

        private async Task CargarAlertasSalud()
        {
            try
            {
                Alertas.Clear();

                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (!int.TryParse(userIdStr, out var idUsuario))
                {
                    Debug.WriteLine("[VM] Usuario no autenticado para alertas");
                    HayAlertas = false;
                    return;
                }

                var req = new ReqObtenerUsuario { IdUsuario = idUsuario };
                Debug.WriteLine($"[VM] Solicitando alertas de salud para {idUsuario}");
                var res = await _apiService.ObtenerAlertasSaludAsync(req);
                Debug.WriteLine($"[VM] Respuesta alertas: resultado={res?.resultado} count={res?.Alertas?.Count}");

                if (res?.Alertas != null)
                {
                    foreach (var a in res.Alertas)
                    {
                        Debug.WriteLine($"[VM] Agregando alerta: Riesgo={a.Riesgo}");
                        Alertas.Add(a);
                    }
                }

                HayAlertas = Alertas.Any();
                Debug.WriteLine($"[VM] HayAlertas = {HayAlertas}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Error al cargar alertas: {ex}");
                HayAlertas = false;
            }
        }

        private async Task CargarSintomasUsuario()
        {
            try
            {
                sintomasUsuario.Clear();
                Debug.WriteLine("[VM] Iniciando carga de síntomas...");

                var userIdStr = await SecureStorage.GetAsync("user_id");
                Debug.WriteLine($"[VM] user_id para síntomas: {userIdStr}");

                if (!int.TryParse(userIdStr, out var userId))
                {
                    Debug.WriteLine("[VM] Usuario no autenticado para síntomas");
                    HaySintomas = false;
                    return;
                }

                var request = new ReqObtenerUsuario { IdUsuario = userId };
                Debug.WriteLine($"[VM] Enviando request síntomas para userId: {userId}");

                var response = await _apiService.ObtenerSintomasUsuarioAsync(request);
                Debug.WriteLine($"[VM] Respuesta síntomas: resultado={response?.resultado} count={response?.Sintomas?.Count}");

                if (response?.resultado == true && response.Sintomas != null && response.Sintomas.Any())
                {
                    foreach (var sintoma in response.Sintomas)
                    {
                        Debug.WriteLine($"[VM] Agregando síntoma: {sintoma.Sintoma} - {sintoma.FechaReporte}");
                        sintomasUsuario.Add(new SintomaUsuario
                        {
                            IdSintoma = 0,
                            Nombre = sintoma.Sintoma,
                            FechaReporte = sintoma.FechaReporte,
                            EsManual = false
                        });
                    }

                    Debug.WriteLine($"[VM] Cargados {sintomasUsuario.Count} síntomas desde backend");
                }
                else
                {
                    Debug.WriteLine("[VM] No se encontraron síntomas del usuario en el backend");

                    // Agregar síntomas de ejemplo para poder probar la funcionalidad
                    var sintomasEjemplo = new List<SintomaUsuario>
                    {
                        new SintomaUsuario
                        {
                            IdSintoma = 1,
                            Nombre = "Dolor de cabeza",
                            FechaReporte = DateTime.Today.AddDays(-1),
                            EsManual = false
                        },
                        new SintomaUsuario
                        {
                            IdSintoma = 2,
                            Nombre = "Fatiga",
                            FechaReporte = DateTime.Today.AddDays(-2),
                            EsManual = false
                        }
                    };

                    foreach (var sintoma in sintomasEjemplo)
                    {
                        sintomasUsuario.Add(sintoma);
                    }

                    Debug.WriteLine($"[VM] Agregados {sintomasEjemplo.Count} síntomas de ejemplo");
                }

                HaySintomas = sintomasUsuario.Any();
                Debug.WriteLine($"[VM] HaySintomas = {HaySintomas} - Total síntomas: {sintomasUsuario.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Error cargando síntomas del usuario: {ex.Message}");
                Debug.WriteLine($"[VM] StackTrace: {ex.StackTrace}");
                HaySintomas = false;
            }
        }

        /*[RelayCommand]
        private void MarcarTomado(EventoAgenda medicamento)
        {
            try
            {
                if (medicamento != null)
                {
                    medicamento.Completado = !medicamento.Completado;

                    // Notificar al servicio que hubo cambios
                    _eventosService.ActualizarEstadoEvento(medicamento);

                    System.Diagnostics.Debug.WriteLine($"Medicamento {medicamento.Titulo} marcado como {(medicamento.Completado ? "tomado" : "pendiente")}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marcando medicamento: {ex.Message}");
            }
        }*/

        [RelayCommand]
        private async Task IrAAgenda()
        {
            try
            {
                // Navegar a la pantalla de agenda
                await Shell.Current.GoToAsync("//agenda");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a agenda: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task VerTodosEscaneos()
        {
            try
            {
                // TODO: Navegar a pantalla de historial de escaneos
                await Shell.Current.DisplayAlert("Función", "Ir a historial de escaneos", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a escaneos: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task VerTodosHabitos()
        {
            try
            {
                // TODO: Navegar a pantalla de hábitos saludables
                await Shell.Current.DisplayAlert("Función", "Ver todos los hábitos saludables", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a hábitos: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AgregarSintoma(string nombreSintoma)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreSintoma)) return;

                // TODO: Conectar con backend para agregar síntoma
                // var response = await _apiService.AgregarSintomaAsync(_idUsuarioActual, nombreSintoma);

                await Shell.Current.DisplayAlert("Síntoma", $"Se agregó: {nombreSintoma}", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando síntoma: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task GestionarSintomas()
        {
            try
            {
                Debug.WriteLine("[VM] Abriendo popup de gestión de síntomas...");

                // Crear y mostrar el popup de gestión de síntomas
                var viewModel = new GestionarSintomasViewModel(_apiService);
                var popup = new ModalGestionarSintomas(viewModel);

                // Mostrar el popup
                Application.Current?.MainPage?.ShowPopup(popup);
                Debug.WriteLine("[VM] Popup de síntomas abierto exitosamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM] Error abriendo popup de síntomas: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo abrir la gestión de síntomas", "OK");
            }
        }

        [RelayCommand]
        private async Task RefrescarDatos()
        {
            try
            {
                await CargarDatosIniciales();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refrescando datos: {ex.Message}");
            }
        }

        // Método público para ser llamado desde OnAppearing
        public async Task RecargarDatos()
        {
            Debug.WriteLine("[VM] RecargarDatos llamado desde OnAppearing");
            await CargarDatosIniciales();
        }

        // Cleanup
        ~InicioViewModel()
        {
            if (_eventosService != null)
            {
                //_eventosService.EventoActualizado -= OnEventoActualizado;
            }
        }
    } 


    // Modelos para la vista
    public class EscaneoReciente
    {
        public string NombreComercial { get; set; } = "";
        public DateTime FechaEscaneo { get; set; }
        public string Fabricante { get; set; } = "";
        public string FechaFormateada => FechaEscaneo.ToString("dd 'de' MMMM, h:mm tt", new CultureInfo("es-ES"));
    }

    public class HabitoSaludable
    {
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }

    public class RecomendacionesIA
    {
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }

    public class Interaccion
    {
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }

    public class SintomaUsuario
    {
        public int IdSintoma { get; set; }
        public string Nombre { get; set; } = "";
        public DateTime FechaReporte { get; set; }
        public bool EsManual { get; set; }
        public string FechaFormateada => FechaReporte.ToString("dd/MM", new CultureInfo("es-ES"));
    }

}