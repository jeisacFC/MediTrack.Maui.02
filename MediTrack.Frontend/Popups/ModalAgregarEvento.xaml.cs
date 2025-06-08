using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales
{
    public partial class ModalAgregarEvento : ContentPage
    {
        public bool EventoGuardado => ViewModel?.EventoGuardado ?? false;

        private AgregarEventoViewModel? ViewModel => BindingContext as AgregarEventoViewModel;

        public ModalAgregarEvento(DateTime fechaSeleccionada)
        {
            InitializeComponent();

            // Obtener ApiService desde DI o usar DependencyService
            var apiService = Handler?.MauiContext?.Services?.GetService<IApiService>()
                           ?? Microsoft.Maui.Controls.DependencyService.Get<IApiService>();

            // Crear ViewModel con dependencias
            BindingContext = new AgregarEventoViewModel(fechaSeleccionada, apiService);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel != null)
            {
                await ViewModel.InitializeAsync();
            }
        }

        // Método para manejar el botón de retroceso del dispositivo
        protected override bool OnBackButtonPressed()
        {
            if (ViewModel != null)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await ViewModel.OnBackButtonPressed();
                });
            }

            return true; // Prevenir el comportamiento por defecto
        }
    }
}