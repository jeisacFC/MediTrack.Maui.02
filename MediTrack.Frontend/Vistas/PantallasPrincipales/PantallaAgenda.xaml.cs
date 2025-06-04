using MediTrack.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Calendar;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales
{
    public partial class PantallaAgenda : ContentPage
    {
        private AgendaViewModel _viewModel;

        public PantallaAgenda()
        {
            try
            {
                InitializeComponent();

                // Crear ViewModel
                _viewModel = new AgendaViewModel();
                BindingContext = _viewModel;

                // ✅ PERSONALIZAR COLORES DESPUÉS DE INICIALIZAR
                PersonalizarColoresCalendario();

                System.Diagnostics.Debug.WriteLine("PantallaAgenda con Syncfusion inicializada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en constructor: {ex.Message}");
            }
        }

        // ✅ MÉTODO COMPLETO - Configurar selección y días de semana
        private void PersonalizarColoresCalendario()
        {
            try
            {
                // Configurar el MonthView básico
                var monthView = new CalendarMonthView()
                {
                    Background = Colors.Transparent
                };

                // ✅ CONFIGURAR ESTILOS DE TEXTO
                try
                {
                    // Texto en blanco para números
                    monthView.TextStyle = new CalendarTextStyle()
                    {
                        TextColor = Colors.White,
                        FontSize = 16
                    };

                    // Texto para días deshabilitados
                    monthView.DisabledDatesTextStyle = new CalendarTextStyle()
                    {
                        TextColor = Color.FromArgb("#80FFFFFF"),
                        FontSize = 16
                    };

                    // Texto para días de otros meses
                    monthView.TrailingLeadingDatesTextStyle = new CalendarTextStyle()
                    {
                        TextColor = Color.FromArgb("#60FFFFFF"),
                        FontSize = 14
                    };

                    // ✅ SIN fondo para día actual (eliminar cuadrado)
                    monthView.TodayBackground = Colors.Transparent;

                    // Texto del día actual en blanco y bold
                    monthView.TodayTextStyle = new CalendarTextStyle()
                    {
                        TextColor = Colors.White,
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold
                    };

                    System.Diagnostics.Debug.WriteLine("✅ Estilos básicos aplicados");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en estilos básicos: {ex.Message}");
                }

                // Aplicar la configuración
                CalendarioSyncfusion.MonthView = monthView;
                CalendarioSyncfusion.SelectionMode = CalendarSelectionMode.Single;

                // ✅ CONFIGURAR COLOR DE SELECCIÓN (círculo)
                try
                {
                    // Intentar configurar el color de selección a celeste claro
                    var selectionBackground = Color.FromArgb("#87CEEB"); // Celeste claro

                    // Usar reflexión para configurar SelectionBackground si existe
                    var monthViewType = monthView.GetType();
                    var selectionProperty = monthViewType.GetProperty("SelectionBackground");

                    if (selectionProperty != null && selectionProperty.CanWrite)
                    {
                        selectionProperty.SetValue(monthView, selectionBackground);
                        System.Diagnostics.Debug.WriteLine("✅ SelectionBackground configurado a celeste");
                    }
                    else
                    {
                        // Intentar en el calendario principal
                        var calendarType = CalendarioSyncfusion.GetType();
                        var calendarSelectionProperty = calendarType.GetProperty("SelectionBackground");

                        if (calendarSelectionProperty != null && calendarSelectionProperty.CanWrite)
                        {
                            calendarSelectionProperty.SetValue(CalendarioSyncfusion, selectionBackground);
                            System.Diagnostics.Debug.WriteLine("✅ Calendar SelectionBackground configurado");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error configurando selección: {ex.Message}");
                }

                // ✅ CONFIGURAR DÍAS DE LA SEMANA (do, lu, ma, etc.)
                try
                {
                    // Intentar configurar ViewHeaderTextStyle usando reflexión
                    var monthViewType = monthView.GetType();
                    var viewHeaderProperty = monthViewType.GetProperty("ViewHeaderTextStyle");

                    if (viewHeaderProperty != null && viewHeaderProperty.CanWrite)
                    {
                        var headerStyle = new CalendarTextStyle()
                        {
                            TextColor = Colors.White,
                            FontSize = 14,
                            FontAttributes = FontAttributes.Bold
                        };

                        viewHeaderProperty.SetValue(monthView, headerStyle);
                        System.Diagnostics.Debug.WriteLine("✅ ViewHeaderTextStyle configurado a blanco");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ ViewHeaderTextStyle no disponible");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error configurando días de semana: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine("✅ Calendario configurado completamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en configuración: {ex.Message}");
            }
        }

        // Manejar el evento de selección del calendario Syncfusion
        private void OnCalendarSelectionChanged(object sender, CalendarSelectionChangedEventArgs e)
        {
            try
            {
                if (e.NewValue != null && _viewModel != null)
                {
                    _viewModel.FechaSeleccionada = (DateTime)e.NewValue;
                    System.Diagnostics.Debug.WriteLine($"Fecha seleccionada: {e.NewValue:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en selección de calendario: {ex.Message}");
            }
        }

        // MEJORADO: Navegación anterior con animación
        private async void AnteriorMes(object sender, EventArgs e)
        {
            try
            {
                var fechaActual = _viewModel.FechaSeleccionada;
                var nuevaFecha = fechaActual.AddMonths(-1);

                // Animación visual del botón
                var boton = sender as Button;
                if (boton != null)
                {
                    await boton.ScaleTo(0.9, 100, Easing.CubicOut);
                    await boton.ScaleTo(1.0, 100, Easing.CubicOut);
                }

                // Actualizar PRIMERO el ViewModel
                _viewModel.FechaSeleccionada = nuevaFecha;

                // DESPUÉS actualizar Syncfusion
                CalendarioSyncfusion.SelectedDate = nuevaFecha;
                CalendarioSyncfusion.DisplayDate = nuevaFecha;

                System.Diagnostics.Debug.WriteLine($"Navegado a mes anterior: {nuevaFecha:yyyy-MM}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a mes anterior: {ex.Message}");
            }
        }

        // MEJORADO: Navegación siguiente con animación
        private async void SiguienteMes(object sender, EventArgs e)
        {
            try
            {
                var fechaActual = _viewModel.FechaSeleccionada;
                var nuevaFecha = fechaActual.AddMonths(1);

                // Animación visual del botón
                var boton = sender as Button;
                if (boton != null)
                {
                    await boton.ScaleTo(0.9, 100, Easing.CubicOut);
                    await boton.ScaleTo(1.0, 100, Easing.CubicOut);
                }

                // Actualizar PRIMERO el ViewModel
                _viewModel.FechaSeleccionada = nuevaFecha;

                // DESPUÉS actualizar Syncfusion
                CalendarioSyncfusion.SelectedDate = nuevaFecha;
                CalendarioSyncfusion.DisplayDate = nuevaFecha;

                System.Diagnostics.Debug.WriteLine($"Navegado a mes siguiente: {nuevaFecha:yyyy-MM}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a mes siguiente: {ex.Message}");
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();

                // Asegurar que el calendario esté en el mes correcto
                if (_viewModel != null)
                {
                    CalendarioSyncfusion.DisplayDate = _viewModel.FechaSeleccionada;
                    CalendarioSyncfusion.SelectedDate = _viewModel.FechaSeleccionada;
                    _viewModel.CargarEventosDelDia();
                }

                // Reconfigurar colores por si acaso
                PersonalizarColoresCalendario();

                // ✅ CONFIGURACIÓN ADICIONAL DESPUÉS DE CARGAR
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(100); // Esperar que el calendario termine de cargar
                    ConfigurarColoresFinales();
                });

                System.Diagnostics.Debug.WriteLine("PantallaAgenda OnAppearing");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en OnAppearing: {ex.Message}");
            }
        }

        // ✅ MÉTODO ADICIONAL PARA CONFIGURACIÓN FINAL
        private void ConfigurarColoresFinales()
        {
            try
            {
                // Intentar configurar usando diferentes aproximaciones
                var calendar = CalendarioSyncfusion;

                // Aproximación 1: Buscar todas las propiedades de selección posibles
                var calendarType = calendar.GetType();
                var properties = calendarType.GetProperties();

                foreach (var prop in properties)
                {
                    try
                    {
                        if (prop.Name.Contains("Selection") && prop.Name.Contains("Background") && prop.CanWrite)
                        {
                            prop.SetValue(calendar, Color.FromArgb("#87CEEB"));
                            System.Diagnostics.Debug.WriteLine($"✅ Configurado {prop.Name} a celeste");
                        }

                        if (prop.Name.Contains("Header") && prop.Name.Contains("Text") && prop.CanWrite)
                        {
                            var headerStyle = new CalendarTextStyle()
                            {
                                TextColor = Colors.White,
                                FontSize = 14,
                                FontAttributes = FontAttributes.Bold
                            };
                            prop.SetValue(calendar, headerStyle);
                            System.Diagnostics.Debug.WriteLine($"✅ Configurado {prop.Name} a blanco");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignorar errores de propiedades individuales
                        System.Diagnostics.Debug.WriteLine($"No se pudo configurar {prop.Name}: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("✅ Configuración final completada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en configuración final: {ex.Message}");
            }
        }
    }
}