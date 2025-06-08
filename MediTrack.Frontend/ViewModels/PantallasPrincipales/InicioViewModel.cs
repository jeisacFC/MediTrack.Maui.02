using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Interfaces;
using System.Diagnostics;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class InicioViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ResEventoCalendario> medicamentosHoy = new();

        [ObservableProperty]
        private ObservableCollection<EscaneoReciente> escaneosRecientes = new();

        [ObservableProperty]
        private ObservableCollection<HabitoSaludable> habitosSaludables = new();

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
        private bool haySintomas = false;

        private readonly IApiService _apiService;
        private readonly CultureInfo _culturaEspañola = new("es-ES");
        private int _idUsuarioActual = 0;

        public InicioViewModel(IApiService apiService)
        {
            try
            {
                _apiService = apiService;

                // Configurar cultura española
                CultureInfo.CurrentCulture = _culturaEspañola;
                CultureInfo.CurrentUICulture = _culturaEspañola;

                ActualizarFechaHoy();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en InicioViewModel: {ex.Message}");
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                await ObtenerIdUsuarioActual();
                await CargarDatosIniciales();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inicializando InicioViewModel: {ex.Message}");
            }
        }

        private async Task ObtenerIdUsuarioActual()
        {
            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var userId))
                {
                    _idUsuarioActual = userId;
                    Debug.WriteLine($"ID Usuario obtenido para Inicio: {_idUsuarioActual}");
                }
                else
                {
                    Debug.WriteLine("No se pudo obtener ID del usuario desde SecureStorage");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo ID usuario: {ex.Message}");
            }
        }

        private async Task CargarDatosIniciales()
        {
            try
            {
                IsLoading = true;

                if (_idUsuarioActual <= 0)
                {
                    Debug.WriteLine("No hay usuario autenticado para cargar datos");
                    return;
                }

                // Cargar todos los datos en paralelo
                var tareas = new List<Task>
                {
                    CargarMedicamentosHoy(),
                    CargarEscaneosRecientes(),
                    CargarHabitosSaludables(),
                    CargarSintomasUsuario()
                };

                await Task.WhenAll(tareas);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando datos iniciales: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
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
                Debug.WriteLine($"Error actualizando fecha: {ex.Message}");
                FechaHoy = DateTime.Today.ToString("dd/MM/yyyy");
            }
        }

        private async Task CargarMedicamentosHoy()
        {
            try
            {
                // Limpiar lista
                MedicamentosHoy.Clear();

                // Llamar al backend para obtener eventos del usuario
                var request = new ReqObtenerUsuario { IdUsuario = _idUsuarioActual };
                var response = await _apiService.ObtenerEventosAsync(request);

                if (response != null && response.resultado && response.Eventos != null)
                {
                    // Filtrar solo medicamentos de hoy
                    var medicamentosDeHoy = response.Eventos
                        .Where(e => e.FechaHora.Date == DateTime.Today &&
                                   e.Tipo?.ToLower().Contains("medicamento") == true)
                        .OrderBy(e => e.FechaHora)
                        .ToList();

                    foreach (var medicamento in medicamentosDeHoy)
                    {
                        MedicamentosHoy.Add(medicamento);
                    }

                    Debug.WriteLine($"Cargados {MedicamentosHoy.Count} medicamentos para hoy desde backend");
                }
                else
                {
                    Debug.WriteLine($"No se obtuvieron eventos del backend: {response?.Mensaje ?? "Respuesta nula"}");
                }

                HayMedicamentos = MedicamentosHoy.Any();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando medicamentos: {ex.Message}");
                HayMedicamentos = false;
            }
        }

        private async Task CargarEscaneosRecientes()
        {
            try
            {
                // Limpiar lista
                EscaneosRecientes.Clear();

                // TODO: Conectar con backend real cuando esté disponible el endpoint
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

                Debug.WriteLine($"Cargados {EscaneosRecientes.Count} escaneos recientes (datos de ejemplo)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando escaneos recientes: {ex.Message}");
                HayEscaneos = false;
            }
        }

        private async Task CargarHabitosSaludables()
        {
            try
            {
                // Limpiar lista
                HabitosSaludables.Clear();

                // TODO: Conectar con endpoint de IA para hábitos cuando esté disponible
                // var request = new ReqObtenerUsuario { IdUsuario = _idUsuarioActual };
                // var response = await _apiService.ObtenerHabitosAsync(request);

                // Por ahora, datos de ejemplo
                var habitosEjemplo = new List<HabitoSaludable>
                {
                    new HabitoSaludable
                    {
                        Titulo = "Beber al menos 8 vasos de agua al día",
                        Descripcion = "Mantenerte hidratado ayuda a tu organismo a funcionar mejor.",
                        Icono = "droplet",
                        Prioridad = 1
                    },
                    new HabitoSaludable
                    {
                        Titulo = "Caminar 30 minutos diarios",
                        Descripcion = "El ejercicio ligero mejora tu salud cardiovascular.",
                        Icono = "directions_walk",
                        Prioridad = 2
                    },
                    new HabitoSaludable
                    {
                        Titulo = "Dormir 7-8 horas cada noche",
                        Descripcion = "Un buen descanso es fundamental para tu recuperación.",
                        Icono = "hotel",
                        Prioridad = 3
                    }
                };

                foreach (var habito in habitosEjemplo)
                {
                    HabitosSaludables.Add(habito);
                }

                HayHabitos = HabitosSaludables.Any();

                Debug.WriteLine($"Cargados {HabitosSaludables.Count} hábitos saludables (datos de ejemplo)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando hábitos saludables: {ex.Message}");
                HayHabitos = false;
            }
        }

        private async Task CargarSintomasUsuario()
        {
            try
            {
                // Limpiar lista
                SintomasUsuario.Clear();

                // TODO: Conectar con backend real para síntomas del usuario cuando esté disponible
                // var request = new ReqObtenerUsuario { IdUsuario = _idUsuarioActual };
                // var response = await _apiService.ObtenerSintomasUsuarioAsync(request);

                // Por ahora, datos de ejemplo
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
                        Nombre = "Fiebre",
                        FechaReporte = DateTime.Today.AddDays(-2),
                        EsManual = false
                    },
                    new SintomaUsuario
                    {
                        IdSintoma = 3,
                        Nombre = "Náuseas",
                        FechaReporte = DateTime.Today,
                        EsManual = true
                    }
                };

                foreach (var sintoma in sintomasEjemplo)
                {
                    SintomasUsuario.Add(sintoma);
                }

                HaySintomas = SintomasUsuario.Any();

                Debug.WriteLine($"Cargados {SintomasUsuario.Count} síntomas del usuario (datos de ejemplo)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando síntomas del usuario: {ex.Message}");
                HaySintomas = false;
            }
        }

        [RelayCommand]
        private async Task MarcarTomado(ResEventoCalendario medicamento)
        {
            try
            {
                if (medicamento == null) return;

                // TODO: Implementar cuando tengamos IDs y estado en el backend
                await Shell.Current.DisplayAlert("Función", "Marcar medicamento como tomado estará disponible cuando se implemente el estado en el backend", "OK");

                Debug.WriteLine($"Medicamento {medicamento.Titulo} - función pendiente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marcando medicamento: {ex.Message}");
            }
        }

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
                Debug.WriteLine($"Error navegando a agenda: {ex.Message}");
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
                Debug.WriteLine($"Error navegando a escaneos: {ex.Message}");
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
                Debug.WriteLine($"Error navegando a hábitos: {ex.Message}");
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
                Debug.WriteLine($"Error agregando síntoma: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task GestionarSintomas()
        {
            try
            {
                // TODO: Navegar a pantalla de gestión de síntomas
                await Shell.Current.DisplayAlert("Función", "Gestionar síntomas", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navegando a síntomas: {ex.Message}");
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
                Debug.WriteLine($"Error refrescando datos: {ex.Message}");
            }
        }

        // Método público para ser llamado desde OnAppearing
        public async Task RecargarDatos()
        {
            await CargarDatosIniciales();
        }
    }

    // Modelos para la vista (mantener los mismos)
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
        public string Icono { get; set; } = ""; // Usar nombres de Material Icons
        public int Prioridad { get; set; }
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