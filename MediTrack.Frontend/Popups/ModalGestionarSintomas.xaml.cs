using MediTrack.Frontend.ViewModels;
using CommunityToolkit.Maui.Views;

namespace MediTrack.Frontend.Popups;

public partial class ModalGestionarSintomas : Popup
{
    private GestionarSintomasViewModel _viewModel;

    public ModalGestionarSintomas(GestionarSintomasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Pasar la referencia del popup al ViewModel
        _viewModel.SetPopupReference(this);

        // Configurar el popup
        this.CanBeDismissedByTappingOutsideOfPopup = true;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        
        // Usar Dispatcher para evitar problemas de timing
        if (Handler != null)
        {
            Dispatcher.Dispatch(async () =>
            {
                try
                {
                    await _viewModel.CargarSintomasDisponibles();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error cargando síntomas en popup: {ex.Message}");
                }
            });
        }
    }

    private void OnBusquedaTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            _viewModel.FiltrarSintomas(e.NewTextValue);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en búsqueda: {ex.Message}");
        }
    }

    private void OnSintomaSeleccionado(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is SintomaSeleccionable sintoma)
            {
                _viewModel.ToggleSintomaSeleccion(sintoma);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error seleccionando síntoma: {ex.Message}");
        }
    }
}