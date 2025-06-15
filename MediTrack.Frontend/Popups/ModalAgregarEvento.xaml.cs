using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Services.Interfaces;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales
{
    public partial class ModalAgregarEvento : ContentPage
    {
        private AgregarEventoViewModel _viewModel;

        public bool EventoGuardado => _viewModel?.EventoGuardado ?? false;
        public EventoMedicoUsuario? EventoCreado => _viewModel?.EventoCreado;

        // Constructor para CREAR evento nuevo
        public ModalAgregarEvento(DateTime fechaSeleccionada, IApiService apiService)
        {
            InitializeComponent();
            _viewModel = new AgregarEventoViewModel(fechaSeleccionada, apiService);
            BindingContext = _viewModel;
        }

        // Constructor para EDITAR evento existente
        public ModalAgregarEvento(EventoMedicoUsuario eventoExistente, IApiService apiService)
        {
            InitializeComponent();
            _viewModel = new AgregarEventoViewModel(eventoExistente, apiService);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.InitializeAsync();
            }
        }

        // Método para manejar el botón de retroceso del dispositivo
        protected override bool OnBackButtonPressed()
        {
            if (_viewModel != null)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _viewModel.OnBackButtonPressed();
                });
            }
            return true; // Prevenir el comportamiento por defecto
        }
    }
}