using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Services.Interfaces;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class InicioViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<EventoAgenda> medicamentosHoy = new();

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

        private readonly EventosService _eventosService;
        private readonly CultureInfo _culturaEspañola = new("es-ES");
        private readonly int _idUsuarioActual = 1; // TODO: Obtener del servicio de autenticación

        public InicioViewModel()
        {
            try
            {
                // Usar servicios
                _eventosService = EventosService.Instance;

                // Suscribirse a cambios en medicamentos
                _eventosService.EventoActualizado += OnEventoActualizado;

                // Configurar cultura española
                CultureInfo.CurrentCulture = _culturaEspañola;
                CultureInfo.CurrentUICulture = _culturaEspañola;

                ActualizarFechaHoy();
                _ = CargarDatosIniciales();
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
                System.Diagnostics.Debug.WriteLine($"Error cargando datos iniciales: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnEventoActualizado(object sender, EventoAgenda evento)
        {
            // Recargar medicamentos cuando hay cambios
            if (evento.Tipo == "Medicamento" && evento.FechaHora.Date == DateTime.Today)
            {
                _ = CargarMedicamentosHoy();
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

        private async Task CargarMedicamentosHoy()
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
        }

        private async Task CargarEscaneosRecientes()
        {
            try
            {
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

                System.Diagnostics.Debug.WriteLine($"Cargados {EscaneosRecientes.Count} escaneos recientes");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando escaneos recientes: {ex.Message}");
                HayEscaneos = false;
            }
        }

        private async Task CargarHabitosSaludables()
        {
            try
            {
                // Limpiar lista
                HabitosSaludables.Clear();

                // TODO: Conectar con endpoint de IA para hábitos
                // var response = await _apiService.ObtenerHabitosAsync(_idUsuarioActual);

                // Por ahora, datos de ejemplo
                var habitosEjemplo = new List<HabitoSaludable>
                {
                    new HabitoSaludable
                    {
                        Titulo = "Beber al menos 8 vasos de agua al día",
                        Descripcion = "Mantenerte hidratado ayuda a tu organismo a funcionar mejor.",
                        Icono = "💧",
                        Prioridad = 1
                    },
                    new HabitoSaludable
                    {
                        Titulo = "Caminar 30 minutos diarios",
                        Descripcion = "El ejercicio ligero mejora tu salud cardiovascular.",
                        Icono = "🚶",
                        Prioridad = 2
                    },
                    new HabitoSaludable
                    {
                        Titulo = "Dormir 7-8 horas cada noche",
                        Descripcion = "Un buen descanso es fundamental para tu recuperación.",
                        Icono = "😴",
                        Prioridad = 3
                    }
                };

                foreach (var habito in habitosEjemplo)
                {
                    HabitosSaludables.Add(habito);
                }

                HayHabitos = HabitosSaludables.Any();

                System.Diagnostics.Debug.WriteLine($"Cargados {HabitosSaludables.Count} hábitos saludables");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando hábitos saludables: {ex.Message}");
                HayHabitos = false;
            }
        }

        private async Task CargarSintomasUsuario()
        {
            try
            {
                // Limpiar lista
                SintomasUsuario.Clear();

                // TODO: Conectar con backend real para síntomas del usuario
                // var response = await _apiService.ObtenerSintomasUsuarioAsync(_idUsuarioActual);

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

                System.Diagnostics.Debug.WriteLine($"Cargados {SintomasUsuario.Count} síntomas del usuario");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando síntomas del usuario: {ex.Message}");
                HaySintomas = false;
            }
        }

        [RelayCommand]
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
                // TODO: Navegar a pantalla de gestión de síntomas
                await Shell.Current.DisplayAlert("Función", "Gestionar síntomas", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a síntomas: {ex.Message}");
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
            await CargarDatosIniciales();
        }

        // Cleanup
        ~InicioViewModel()
        {
            if (_eventosService != null)
            {
                _eventosService.EventoActualizado -= OnEventoActualizado;
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
        public string Icono { get; set; } = "";
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