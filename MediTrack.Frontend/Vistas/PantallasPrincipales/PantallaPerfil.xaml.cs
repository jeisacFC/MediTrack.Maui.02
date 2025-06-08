using MediTrack.Frontend.ViewModels;
using MediTrack.Frontend.Models.Model;
using System.Diagnostics;
using MediTrack.Frontend.Vistas.Base;

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
        public PantallaPerfil()
        {
            // Resuelve el ViewModel desde el contenedor
#pragma warning disable CS8602 
            var viewModel = Application.Current.Handler.MauiContext.Services.GetService<PerfilViewModel>();
#pragma warning restore CS8602 

            _viewModel = viewModel ?? throw new Exception("No se pudo resolver PerfilViewModel");
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await _viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnAppearing de PantallaPerfil: {ex.Message}");
                await DisplayAlert("Error", "Error al cargar los datos del perfil", "OK");
            }
        }

        /// <summary>
        /// Maneja el evento de cambio en el switch de notificaciones
        /// </summary>
        private async void OnToggleNotificaciones(object sender, ToggledEventArgs e)
        {
            try
            {
                if (_viewModel != null)
                {
                    // Ejecutar el comando para alternar notificaciones
                    if (_viewModel.AlternarNotificacionesCommand.CanExecute(null))
                    {
                        await _viewModel.AlternarNotificacionesCommand.ExecuteAsync(null);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnToggleNotificaciones: {ex.Message}");
                await DisplayAlert("Error", "Error al cambiar la configuración de notificaciones", "OK");

                // Revertir el switch si hay error
                if (sender is Microsoft.Maui.Controls.Switch switchControl)
                {
                    switchControl.IsToggled = !e.Value;
                }
            }
        }

        /// <summary>
        /// Maneja la selección de condiciones médicas
        /// </summary>
        private void OnCondicionesMedicasSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _viewModel?.OnCondicionesMedicasSeleccionadas(sender, e);

                // Log para debugging
                Debug.WriteLine($"Condiciones médicas seleccionadas: {e.CurrentSelection.Count}");

                foreach (CondicionesMedicas condicion in e.CurrentSelection)
                {
                    Debug.WriteLine($"- {condicion.nombre_condicion}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnCondicionesMedicasSeleccionadas: {ex.Message}");
            }
        }

        /// <summary>
        /// Maneja la selección de alergias
        /// </summary>
        private void OnAlergiasSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _viewModel?.OnAlergiasSeleccionadas(sender, e);

                // Log para debugging
                Debug.WriteLine($"Alergias seleccionadas: {e.CurrentSelection.Count}");

                foreach (Alergias alergia in e.CurrentSelection)
                {
                    Debug.WriteLine($"- {alergia.nombre_alergia}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnAlergiasSeleccionadas: {ex.Message}");
            }
        }

        /// <summary>
        /// Se ejecuta cuando la página está desapareciendo
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("PantallaPerfil - OnDisappearing");
        }

        /// <summary>
        /// Maneja errores de navegación o inicialización
        /// </summary>
        private async void OnErrorOcurred(string titulo, string mensaje)
        {
            try
            {
                await DisplayAlert(titulo, mensaje, "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error mostrando alerta: {ex.Message}");
            }
        }

        /// <summary>
        /// Método para refrescar manualmente los datos desde la UI
        /// </summary>
        private async void OnRefreshRequested()
        {
            try
            {
                if (_viewModel?.RefrescarPerfilCommand.CanExecute(null) == true)
                {
                    await _viewModel.RefrescarPerfilCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnRefreshRequested: {ex.Message}");
                await DisplayAlert("Error", "Error al refrescar los datos", "OK");
            }
        }
    }
}