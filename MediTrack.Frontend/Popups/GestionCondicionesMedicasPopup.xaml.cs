using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.ViewModels;

namespace MediTrack.Frontend.Popups;

public partial class GestionCondicionesMedicasPopup : Popup
{
    private CondicionesMedicasViewModel _viewModel;

    public GestionCondicionesMedicasPopup(CondicionesMedicasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnCerrarClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }

    private void OnCondicionesSeleccionadas(object sender, SelectionChangedEventArgs e)
    {
        _viewModel?.OnCondicionesSeleccionadas(sender, e);
    }

    protected override async void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler != null && _viewModel != null)
        {
            await _viewModel.InitializeAsync();
        }
    }
}