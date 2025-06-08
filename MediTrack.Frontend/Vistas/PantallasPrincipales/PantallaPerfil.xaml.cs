using MediTrack.Frontend.ViewModels;
using MediTrack.Frontend.Models.Model;
using System.Diagnostics;
using MediTrack.Frontend.Vistas.Base;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaPerfil : BaseContentPage
{
    private PerfilViewModel _viewModel;

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
            Debug.WriteLine("=== PantallaPerfil OnAppearing ===");
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en OnAppearing: {ex.Message}");
            await DisplayAlert("Error", "Error al cargar el perfil", "OK");
        }
    }

    // Evento para manejar cambios en el switch de notificaciones
    private async void OnNotificacionesToggled(object sender, ToggledEventArgs e)
    {
        try
        {
            Debug.WriteLine($"Notificaciones toggled: {e.Value}");

            // Ejecutar el comando de alternar notificaciones del ViewModel
            if (_viewModel.AlternarNotificacionesCommand.CanExecute(null))
            {
                await _viewModel.AlternarNotificacionesCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en OnNotificacionesToggled: {ex.Message}");
            await DisplayAlert("Error", "Error al cambiar configuración de notificaciones", "OK");
        }
    }

    // Evento para manejar selección de condiciones médicas
    private void OnCondicionesMedicasSeleccionadas(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            Debug.WriteLine("=== Condiciones médicas seleccionadas ===");

            // Llamar al método del ViewModel
            _viewModel.OnCondicionesMedicasSeleccionadas(sender, e);

            // Log de las selecciones
            Debug.WriteLine($"Condiciones seleccionadas: {e.CurrentSelection.Count}");
            foreach (CondicionesMedicas condicion in e.CurrentSelection)
            {
                Debug.WriteLine($"- {condicion.nombre_condicion}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en OnCondicionesMedicasSeleccionadas: {ex.Message}");
        }
    }

    // Evento para manejar selección de alergias
    private void OnAlergiasSeleccionadas(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            Debug.WriteLine("=== Alergias seleccionadas ===");

            // Llamar al método del ViewModel
            _viewModel.OnAlergiasSeleccionadas(sender, e);

            // Log de las selecciones
            Debug.WriteLine($"Alergias seleccionadas: {e.CurrentSelection.Count}");
            foreach (Alergias alergia in e.CurrentSelection)
            {
                Debug.WriteLine($"- {alergia.nombre_alergia}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en OnAlergiasSeleccionadas: {ex.Message}");
        }
    }

    // Método para refrescar la página cuando se hace pull-to-refresh (opcional)
    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("=== Refresh solicitado ===");

            if (_viewModel.RefrescarPerfilCommand.CanExecute(null))
            {
                await _viewModel.RefrescarPerfilCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en OnRefreshRequested: {ex.Message}");
            await DisplayAlert("Error", "Error al refrescar el perfil", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Debug.WriteLine("=== PantallaPerfil OnDisappearing ===");
    }
}