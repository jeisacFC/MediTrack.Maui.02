using MediTrack.Frontend.ViewModels;
using CommunityToolkit.Maui.Views;

namespace MediTrack.Frontend.Popups;

public partial class ModalGestionarSintomas : Popup
{
    private GestionarSintomasViewModel _viewModel;
    private bool _isUpdatingFromViewModel = false;

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
            // Evitar bucle infinito: si el cambio viene del ViewModel, no procesarlo
            if (_isUpdatingFromViewModel)
            {
                System.Diagnostics.Debug.WriteLine($"[Modal] Cambio desde ViewModel ignorado");
                return;
            }

            if (sender is CheckBox checkBox && checkBox.BindingContext is SintomaSeleccionable sintoma)
            {
                System.Diagnostics.Debug.WriteLine($"[Modal] CheckBox evento usuario: {sintoma.Nombre} -> {e.Value}");

                // AGREGAR ESTOS LOGS:
                System.Diagnostics.Debug.WriteLine($"[Modal] sintoma.EstaSeleccionado ANTES: {sintoma.EstaSeleccionado}");
                System.Diagnostics.Debug.WriteLine($"[Modal] e.Value: {e.Value}");
                System.Diagnostics.Debug.WriteLine($"[Modal] ¿Son diferentes? {sintoma.EstaSeleccionado != e.Value}");

                // Marcar que estamos actualizando desde el ViewModel para evitar bucles
                _isUpdatingFromViewModel = true;

                try
                {
                    // Ejecutar el toggle solo si el estado realmente cambió
                    if (sintoma.EstaSeleccionado != e.Value)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Modal] LLAMANDO ToggleSintomaSeleccion...");
                        _viewModel.ToggleSintomaSeleccion(sintoma);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Modal] NO LLAMANDO ToggleSintomaSeleccion - estados iguales");
                    }
                }
                finally
                {
                    // Siempre restaurar la bandera
                    _isUpdatingFromViewModel = false;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error seleccionando síntoma: {ex.Message}");
            _isUpdatingFromViewModel = false;
        }
    }
}