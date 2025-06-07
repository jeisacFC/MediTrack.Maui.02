using MediTrack.Frontend.ViewModels;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaPerfil : BaseContentPage
{
    private readonly PerfilViewModel _viewModel;

    // Constructor sin parámetros (requerido por XAML)
    public PantallaPerfil()
    {
        InitializeComponent();
        _viewModel = App.Current.Handler.MauiContext.Services.GetService<PerfilViewModel>();
        BindingContext = _viewModel;

        System.Diagnostics.Debug.WriteLine("=== PantallaPerfil creada con constructor sin parámetros ===");
        System.Diagnostics.Debug.WriteLine($"ViewModel obtenido: {_viewModel != null}");
    }

    // Constructor con parámetros (para inyección de dependencias manual)
    public PantallaPerfil(PerfilViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        System.Diagnostics.Debug.WriteLine("=== PantallaPerfil creada con constructor con parámetros ===");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        System.Diagnostics.Debug.WriteLine("=== OnAppearing de PantallaPerfil ===");
        System.Diagnostics.Debug.WriteLine($"ViewModel es null: {_viewModel == null}");
        System.Diagnostics.Debug.WriteLine($"BindingContext es null: {BindingContext == null}");

        if (_viewModel != null)
        {
            System.Diagnostics.Debug.WriteLine("Llamando a InitializeAsync...");
            await _viewModel.InitializeAsync();
            System.Diagnostics.Debug.WriteLine("InitializeAsync completado");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("ERROR: ViewModel es null!");
        }
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