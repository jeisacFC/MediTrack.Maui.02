using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Popups;
using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using System.Diagnostics;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales;

public partial class PantallaBusqueda : ContentPage
{
    private BusquedaViewModel _viewModel;

    public PantallaBusqueda(BusquedaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // SUSCRIBIRSE A LOS EVENTOS DEL VIEWMODEL
        viewModel.BusquedaExitosa += OnBusquedaExitosa;
        viewModel.BusquedaFallida += OnBusquedaFallida;
        viewModel.MostrarDetalleMedicamento += OnMostrarDetalle;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("PantallaBusqueda: OnAppearing - Cargando medicamentos");

        // Llama al comando para cargar la lista de medicamentos guardados
        if (_viewModel.CargarMisMedicamentosCommand.CanExecute(null))
        {
            await _viewModel.CargarMisMedicamentosCommand.ExecuteAsync(null);
        }
    }

    // LIMPIAR EVENTOS AL SALIR
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
        try
        {
            Debug.WriteLine("PantallaBusqueda: Mostrando popup de búsqueda exitosa");
            var infoPopup = new InfoBusquedaManualPopup(resultado);

            // Manejador para cuando se agrega un medicamento
            async void ManejadorMedicamentoAgregado(object s, ResBuscarMedicamento medParaGuardar)
            {
                try
                {
                    Debug.WriteLine("PantallaBusqueda: Medicamento agregado, guardando...");
                    if (_viewModel?.GuardarMedicamentoCommand != null)
                    {
                        await _viewModel.GuardarMedicamentoCommand.ExecuteAsync(medParaGuardar);
                        // No necesitamos llamar manualmente a CargarMisMedicamentosCommand aquí
                        // porque ya se hace en el ViewModel después de guardar exitosamente
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al manejar medicamento agregado: {ex.Message}");
                }
            }

            infoPopup.MedicamentoAgregado += ManejadorMedicamentoAgregado;

            await this.ShowPopupAsync(infoPopup);

            // Desuscribirse del evento
            infoPopup.MedicamentoAgregado -= ManejadorMedicamentoAgregado;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al mostrar popup de búsqueda: {ex.Message}");
            await DisplayAlert("Error", "No se pudo mostrar la información del medicamento", "OK");
        }
    }

    private async void OnBusquedaFallida(object sender, string mensajeError)
    {
        try
        {
            Debug.WriteLine($"PantallaBusqueda: Búsqueda fallida - {mensajeError}");
            await DisplayAlert("Error de Búsqueda", mensajeError, "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al mostrar alerta de búsqueda fallida: {ex.Message}");
        }
    }

    private async void OnMostrarDetalle(object sender, ResDetalleMedicamentoUsuario detalle)
    {
        try
        {
            Debug.WriteLine($"PantallaBusqueda: Mostrando detalle del medicamento");

            // Creamos el popup de detalle pasándole los datos y el comando de eliminar
            var popup = new DetalleMedicamentoGuardadoPopup(detalle, _viewModel.EliminarMedicamentoCommand);

            // Suscribirse al evento de medicamento eliminado si el popup lo tiene
            if (popup != null)
            {
                // Si el popup tiene un evento para notificar cuando se elimina un medicamento,
                // puedes suscribirte aquí para recargar la lista
                // popup.MedicamentoEliminado += OnMedicamentoEliminado;
            }

            await this.ShowPopupAsync(popup);

            // Desuscribirse si era necesario
            // popup.MedicamentoEliminado -= OnMedicamentoEliminado;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al mostrar popup de detalle: {ex.Message}");
            await DisplayAlert("Error", "No se pudo mostrar el detalle del medicamento", "OK");
        }
    }

    // Método opcional si necesitas manejar la eliminación desde el popup
    private async void OnMedicamentoEliminado(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("PantallaBusqueda: Medicamento eliminado, recargando lista");
            // Recargar la lista después de eliminar
            if (_viewModel.CargarMisMedicamentosCommand.CanExecute(null))
            {
                await _viewModel.CargarMisMedicamentosCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al recargar lista después de eliminar: {ex.Message}");
        }
    }
}