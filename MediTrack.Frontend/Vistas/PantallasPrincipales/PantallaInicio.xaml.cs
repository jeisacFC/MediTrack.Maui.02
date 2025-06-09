using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Vistas.Base;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaInicio : BaseContentPage
{
    private InicioViewModel _viewModel;

    public PantallaInicio(InicioViewModel viewModel)
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
            // Recargar datos cada vez que aparece la pantalla
            await _viewModel.RecargarDatos();

            System.Diagnostics.Debug.WriteLine("PantallaInicio: Datos recargados en OnAppearing");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en OnAppearing de PantallaInicio: {ex.Message}");
        }
    }
}