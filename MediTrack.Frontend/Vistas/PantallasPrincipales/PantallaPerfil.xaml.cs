using MediTrack.Frontend.ViewModels;
using MediTrack.Frontend.Vistas.Base;
using MediTrack.Frontend.Popups;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales
{
    public partial class PantallaPerfil : BaseContentPage
    {
        private PerfilViewModel _viewModel;

        public PantallaPerfil(PerfilViewModel viewModel)
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
                Debug.WriteLine("=== PantallaPerfil OnAppearing ===");
                await _viewModel.InitializeAsync();
                Debug.WriteLine("=== PantallaPerfil datos cargados ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnAppearing: {ex.Message}");
                await DisplayAlert("Error", "Error al cargar el perfil", "OK");
            }
        }

        private async void OnNotificacionesToggled(object sender, ToggledEventArgs e)
        {
            try
            {
                if (_viewModel.AlternarNotificacionesCommand?.CanExecute(null) == true)
                {
                    await _viewModel.AlternarNotificacionesCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al alternar notificaciones: {ex.Message}");
            }
        }

        private async void OnGestionarAlergiasClicked(object sender, EventArgs e)
        {
            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    await DisplayAlert("Error", "No se pudo obtener la información del usuario", "OK");
                    return;
                }

                var apiService = _viewModel.GetApiService();
                var alergiasViewModel = new AlergiasViewModel(apiService, userId);
                var popup = new GestionAlergiasPopup(alergiasViewModel);

                var result = await this.ShowPopupAsync(popup);

                if (result is bool success && success)
                {
                    if (_viewModel.RefrescarPerfilCommand.CanExecute(null))
                    {
                        await _viewModel.RefrescarPerfilCommand.ExecuteAsync(null);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir modal de alergias: {ex.Message}");
                await DisplayAlert("Error", "Error al abrir la gestión de alergias", "OK");
            }
        }
    }
}