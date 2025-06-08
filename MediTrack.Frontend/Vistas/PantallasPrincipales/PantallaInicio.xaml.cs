using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaInicio : ContentPage
{
    private InicioViewModel _viewModel;

    public PantallaInicio()
    {
        InitializeComponent();

        // Obtener ApiService desde DI o usar DependencyService como fallback
        var apiService = Handler?.MauiContext?.Services?.GetService<IApiService>()
                       ?? Microsoft.Maui.Controls.DependencyService.Get<IApiService>();

        // Crear ViewModel con dependencia
        _viewModel = new InicioViewModel(apiService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Inicializar y recargar datos cada vez que aparece la pantalla
            await _viewModel.InitializeAsync();

            System.Diagnostics.Debug.WriteLine("PantallaInicio: Datos recargados desde backend en OnAppearing");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en OnAppearing de PantallaInicio: {ex.Message}");
        }
    }
}