using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Popups; // Asumiendo que crearás un popup para los resultados
using MediTrack.Frontend.ViewModels.PantallasPrincipales;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaBusqueda : ContentPage
{
    private BusquedaViewModel _viewModel;

    public PantallaBusqueda(BusquedaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel; // Usar directamente el parámetro como en PantallaScan

        // SUSCRIBIRSE A LOS EVENTOS DEL VIEWMODEL (como en PantallaScan)
        viewModel.BusquedaExitosa += OnBusquedaExitosa;
        viewModel.BusquedaFallida += OnBusquedaFallida;
        viewModel.MostrarDetalleMedicamento += OnMostrarDetalle;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Llama al comando para cargar la lista de medicamentos guardados
        if (_viewModel.CargarMisMedicamentosCommand.CanExecute(null))
        {
            await _viewModel.CargarMisMedicamentosCommand.ExecuteAsync(null);
        }
    }



    // LIMPIAR EVENTOS AL SALIR (como en PantallaScan)
    protected override void OnDisappearing()
    {
        if (_viewModel != null)
        {
            _viewModel.BusquedaExitosa -= OnBusquedaExitosa;
            _viewModel.BusquedaFallida -= OnBusquedaFallida;
            _viewModel.MostrarDetalleMedicamento -= OnMostrarDetalle;
        }
        base.OnDisappearing();
    }

    private async void OnBusquedaExitosa(object sender, ResBuscarMedicamento resultado)
    {
        var infoPopup = new InfoBusquedaManualPopup(resultado);

        async void ManejadorMedicamentoAgregado(object s, ResBuscarMedicamento medParaGuardar)
        {
            // 3. Llamamos al nuevo comando del ViewModel para que haga la magia
            if (_viewModel?.GuardarMedicamentoCommand != null)
            {
                await _viewModel.GuardarMedicamentoCommand.ExecuteAsync(medParaGuardar);
                await _viewModel.CargarMisMedicamentosCommand.ExecuteAsync(null);
            }
        }

        infoPopup.MedicamentoAgregado += ManejadorMedicamentoAgregado;
        await this.ShowPopupAsync(infoPopup);
        infoPopup.MedicamentoAgregado -= ManejadorMedicamentoAgregado;

    }

    private async void OnBusquedaFallida(object sender, string mensajeError)
    {
        await DisplayAlert("Error de Búsqueda", mensajeError, "OK");
    }

    private async void OnMostrarDetalle(object sender, ResDetalleMedicamentoUsuario detalle)
    {
        // Creamos el nuevo popup pasándole los datos del detalle
        // y el comando de eliminar que vive en el ViewModel.
        var popup = new DetalleMedicamentoGuardadoPopup(detalle, _viewModel.EliminarMedicamentoCommand);
        await this.ShowPopupAsync(popup);
    }
}