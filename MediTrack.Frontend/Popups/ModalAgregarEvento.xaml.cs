using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Models.Model;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales
{
    public partial class ModalAgregarEvento : ContentPage
    {




        //public EventosMedicos? EventoCreado => ViewModel?.EventoCreado;
        //public bool EventoGuardado => ViewModel?.EventoGuardado ?? false;

        //private AgregarEventoViewModel? ViewModel => BindingContext as AgregarEventoViewModel;

        //public ModalAgregarEvento(DateTime fechaSeleccionada)
        //{
        //    InitializeComponent();
        //    BindingContext = new AgregarEventoViewModel(fechaSeleccionada);
        //}

        //protected override async void OnAppearing()
        //{
        //    base.OnAppearing();

        //    if (ViewModel != null)
        //    {
        //        await ViewModel.InitializeAsync();
        //    }
        //}

        //// Método para manejar el botón de retroceso del dispositivo
        //protected override bool OnBackButtonPressed()
        //{
        //    if (ViewModel != null)
        //    {
        //        MainThread.BeginInvokeOnMainThread(async () =>
        //        {
        //            await ViewModel.OnBackButtonPressed();
        //        });
        //    }

        //    return true; // Prevenir el comportamiento por defecto
        //}
    }
}