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

        // Configurar el popup
        this.CanBeDismissedByTappingOutsideOfPopup = true;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null)
        {
            _ = _viewModel.CargarSintomasDisponibles();
        }
    }

    private void OnBusquedaTextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.FiltrarSintomas(e.NewTextValue);
    }

    private void OnSintomaSeleccionado(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is SintomaSeleccionable sintoma)
        {
            _viewModel.ToggleSintomaSeleccion(sintoma);
        }
    }
}