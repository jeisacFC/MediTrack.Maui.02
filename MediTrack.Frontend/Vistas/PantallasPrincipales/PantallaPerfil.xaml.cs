using MediTrack.Frontend.ViewModels;
using MediTrack.Frontend.Vistas;
using System.Diagnostics;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaPerfil : BaseContentPage
{
    private PerfilViewModel _viewModel;

    public PantallaPerfil()
    {
        InitializeComponent();
    }

    public PantallaPerfil(PerfilViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (_viewModel != null)
            {
                await _viewModel.InitializeAsync();
                Debug.WriteLine("=== VIEWMODEL INICIALIZADO CORRECTAMENTE ===");
            }
            else
            {
                Debug.WriteLine("ERROR: ViewModel es null en OnAppearing");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR en OnAppearing de PantallaPerfil: {ex.Message}");
            Debug.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Maneja el evento de cambio en las condiciones médicas seleccionadas
    /// </summary>
    private void OnCondicionesMedicasSeleccionadas(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            Debug.WriteLine($"=== CONDICIONES MÉDICAS SELECCIONADAS: {e.CurrentSelection.Count} ===");
            _viewModel?.OnCondicionesMedicasSeleccionadas(sender, e);

            // Log de las condiciones seleccionadas
            foreach (var item in e.CurrentSelection)
            {
                if (item is MediTrack.Frontend.Models.Model.CondicionesMedicas condicion)
                {
                    Debug.WriteLine($"Condición seleccionada: {condicion.nombre_condicion}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en OnCondicionesMedicasSeleccionadas: {ex.Message}");
        }
    }

    /// <summary>
    /// Maneja el evento de cambio en las alergias seleccionadas
    /// </summary>
    private void OnAlergiasSeleccionadas(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            Debug.WriteLine($"=== ALERGIAS SELECCIONADAS: {e.CurrentSelection.Count} ===");
            _viewModel?.OnAlergiasSeleccionadas(sender, e);

            // Log de las alergias seleccionadas
            foreach (var item in e.CurrentSelection)
            {
                if (item is MediTrack.Frontend.Models.Model.Alergias alergia)
                {
                    Debug.WriteLine($"Alergia seleccionada: {alergia.nombre_alergia}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en OnAlergiasSeleccionadas: {ex.Message}");
        }
    }

    /// <summary>
    /// Maneja el evento de cambio del switch de notificaciones
    /// </summary>
    public async void OnToggleNotificaciones(object sender, ToggledEventArgs e)
    {
        try
        {
            Debug.WriteLine($"=== TOGGLE NOTIFICACIONES: {e.Value} ===");

            if (_viewModel != null)
            {
                // Actualizar el valor en el ViewModel
                _viewModel.Usuario.notificaciones_push = e.Value;

                // Ejecutar el comando para actualizar en el servidor
                if (_viewModel.AlternarNotificacionesCommand.CanExecute(null))
                {
                    await _viewModel.AlternarNotificacionesCommand.ExecuteAsync(null);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en OnToggleNotificaciones: {ex.Message}");

            // Mostrar mensaje de error al usuario
            if (_viewModel != null)
            {
                await _viewModel.ShowAlertAsync("Error",
                    "No se pudo actualizar la configuración de notificaciones. Inténtalo de nuevo.");
            }
        }
    }


    private async void OnRefresh(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("=== INICIANDO REFRESH MANUAL ===");

            if (_viewModel != null && _viewModel.RefrescarPerfilCommand.CanExecute(null))
            {
                await _viewModel.RefrescarPerfilCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en OnRefresh: {ex.Message}");
        }
    }

    /// <summary>
    /// Limpia recursos cuando la página desaparece
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Debug.WriteLine("=== PANTALLA PERFIL DESAPARECIENDO ===");
    }

    /// <summary>
    /// Manejo de eventos de navegación hacia atrás
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        Debug.WriteLine("=== BOTÓN ATRÁS PRESIONADO EN PERFIL ===");

        // Permitir navegación normal hacia atrás
        return base.OnBackButtonPressed();
    }
}

/// <summary>
/// Converter para convertir int a bool (para visibilidad de colecciones)
/// </summary>
public class IntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue > 0;
        }

        if (value is System.Collections.ICollection collection)
        {
            return collection.Count > 0;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}