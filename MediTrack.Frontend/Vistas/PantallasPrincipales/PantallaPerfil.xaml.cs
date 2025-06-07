using MediTrack.Frontend.ViewModels;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaPerfil : BaseContentPage
{
    private readonly PerfilViewModel _viewModel;
    public PantallaPerfil()
    {
        InitializeComponent();

        _viewModel = App.Current.Handler.MauiContext.Services.GetService<PerfilViewModel>();
        BindingContext = _viewModel;
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
        await _viewModel.InitializeAsync();
    }

    // Eventos requeridos por el XAML
    private void OnCondicionesMedicasSeleccionadas(object sender, SelectionChangedEventArgs e)
    {
        _viewModel.OnCondicionesMedicasSeleccionadas(sender, e);
    }

    private void OnAlergiasSeleccionadas(object sender, SelectionChangedEventArgs e)
    {
        _viewModel.OnAlergiasSeleccionadas(sender, e);
    }

    private async void OnEditarPerfilClicked(object sender, EventArgs e)
    {
        await _viewModel.EditarPerfilCommand.ExecuteAsync(null);
    }

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        await _viewModel.CerrarSesionCommand.ExecuteAsync(null);
    }
}