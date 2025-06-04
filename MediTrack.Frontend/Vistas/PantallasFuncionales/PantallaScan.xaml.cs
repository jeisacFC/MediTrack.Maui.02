using CommunityToolkit.Maui.Views; // Para this.ShowPopupAsync()
using MediTrack.Frontend.Popups;   // Para InstruccionesEscaneoPopup
using MediTrack.Frontend.ViewModels; // Para ScanViewModel
using System.Diagnostics;

namespace MediTrack.Frontend.Vistas.PantallasFuncionales;

public partial class PantallaScan : ContentPage
{
    public PantallaScan(ScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Mostrar el popup de instrucciones solo la primera vez o según alguna lógica
        // Por simplicidad, lo mostramos cada vez que aparece la página de escaneo.
        var instruccionesPopup = new InstruccionesEscaneoPopup();
        await this.ShowPopupAsync(instruccionesPopup); // Muestra el popup

        // Una vez que el popup se cierra (por el botón "Entendido"), activamos la cámara
        if (BindingContext is ScanViewModel vm)
        {
            vm.IsDetecting = true;
            Debug.WriteLine("PantallaScan: OnAppearing - IsDetecting puesto a true después del popup");
        }
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is ScanViewModel vm)
        {
            vm.IsDetecting = false;
        }
        Debug.WriteLine("PantallaScan: OnDisappearing - IsDetecting puesto a false");
        base.OnDisappearing();
    }

    // Manejador para el botón Cancelar en el Header
    private async void Cancelar_Clicked_Header(object sender, EventArgs e)
    {
        if (BindingContext is ScanViewModel vm && vm.CancelarEscaneoCommand.CanExecute(null))
        {
            await vm.CancelarEscaneoCommand.ExecuteAsync(null);
        }
    }
}