using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.ViewModels;

namespace MediTrack.Frontend.Popups;

public partial class GestionAlergiasPopup : Popup
{
    private AlergiasViewModel _viewModel;

    public GestionAlergiasPopup(AlergiasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnCerrarClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }

    private void OnAlergiasSeleccionadas(object sender, SelectionChangedEventArgs e)
    {
        _viewModel?.OnAlergiasSeleccionadas(sender, e);
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