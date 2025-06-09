using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Popups;
using MediTrack.Frontend.ViewModels;
using MediTrack.Frontend.Vistas.Base;
using System.Diagnostics;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales
{
    public partial class PantallaPerfil : BaseContentPage
    {
        private PerfilViewModel _viewModel;
        private bool _editandoCondiciones = false;
        private bool _editandoAlergias = false;
        private bool _editandoInfoPersonal = false;

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnAppearing: {ex.Message}");
                await DisplayAlert("Error", "Error al cargar el perfil", "OK");
            }
        }

        private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
        {
            // Calculamos la opacidad basada en el scroll
            double scrollY = e.ScrollY;
            double maxScroll = 100; // Distancia m�xima para desaparecer completamente

            double opacity = Math.Max(0, 1 - (scrollY / maxScroll));
            double scale = Math.Max(0.5, 1 - (scrollY / (maxScroll * 2)));

            // Aplicamos la transformaci�n al avatar
            AvatarContainer.Opacity = opacity;
            AvatarContainer.Scale = scale;

            // Tambi�n podemos mover el avatar hacia arriba
            AvatarContainer.TranslationY = -scrollY * 0.5;
        }

        private async void OnEditInfoPersonalClicked(object sender, EventArgs e)
        {
            try
            {
                _editandoInfoPersonal = !_editandoInfoPersonal;

                if (_editandoInfoPersonal)
                {
                    BtnEditarInfoPersonal.Text = "&#xE14C;"; // edit_off_24 (done/check icon)
                    BtnEditarInfoPersonal.BackgroundColor = Color.FromArgb("#28a745");

                    // Verificar que tenemos los datos necesarios
                    if (_viewModel.Usuario == null)
                    {
                        await DisplayAlert("Error", "No se pueden cargar los datos del usuario", "OK");
                        return;
                    }

                    // Obtener el ApiService del ViewModel
                    var apiService = _viewModel.GetApiService();
                    if (apiService == null)
                    {
                        await DisplayAlert("Error", "Error al acceder al servicio API", "OK");
                        return;
                    }

                    // Crear el ViewModel espec�fico del popup con los par�metros requeridos
                    var popupViewModel = new ActualizarPerfilPopupViewModel(
                        apiService,
                        _viewModel.Usuario
                    );

                    var popup = new ActualizarPerfilPopup(popupViewModel);

                    var result = await this.ShowPopupAsync(popup);

                    // Si el resultado es true, significa que se actualiz� exitosamente
                    if (result is bool success && success)
                    {
                        Debug.WriteLine("=== POPUP CERRADO - ACTUALIZACI�N EXITOSA ===");
                        // Refrescar los datos del perfil
                        if (_viewModel.RefrescarPerfilCommand.CanExecute(null))
                        {
                            await _viewModel.RefrescarPerfilCommand.ExecuteAsync(null);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("=== POPUP CERRADO - SIN CAMBIOS ===");
                    }

                    // Resetear el estado del bot�n
                    _editandoInfoPersonal = false;
                    BtnEditarInfoPersonal.Text = "&#xE3C9;"; // edit_24
                    BtnEditarInfoPersonal.BackgroundColor = Color.FromArgb("#3b71ff");
                }
                else
                {
                    BtnEditarInfoPersonal.Text = "&#xE3C9;"; // edit_24
                    BtnEditarInfoPersonal.BackgroundColor = Color.FromArgb("#3b71ff");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnEditInfoPersonalClicked: {ex.Message}");
                await DisplayAlert("Error", "Error al abrir el modal de datos personales", "OK");

                // Resetear el estado del bot�n en caso de error
                _editandoInfoPersonal = false;
                BtnEditarInfoPersonal.Text = "&#xE3C9;";
                BtnEditarInfoPersonal.BackgroundColor = Color.FromArgb("#3b71ff");
            }
        }

        // M�TODO PRINCIPAL PARA ABRIR EL MODAL DE CONDICIONES M�DICAS
        private async void OnEditCondicionesMedicasClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("=== Abriendo modal de condiciones m�dicas ===");

                // Verificar que tenemos los datos del usuario necesarios
                if (_viewModel.Usuario?.id_usuario == null)
                {
                    await DisplayAlert("Error", "No se pueden cargar los datos del usuario", "OK");
                    return;
                }

                // Obtener el ApiService del ViewModel
                var apiService = _viewModel.GetApiService();
                if (apiService == null)
                {
                    await DisplayAlert("Error", "Error al acceder al servicio API", "OK");
                    return;
                }

                // Crear el ViewModel del modal
                var condicionesViewModel = new CondicionesMedicasViewModel(apiService, _viewModel.Usuario.id_usuario);

                // Crear y mostrar el popup
                var popup = new GestionCondicionesMedicasPopup(condicionesViewModel);
                var result = await this.ShowPopupAsync(popup);

                // Si se realizaron cambios, refrescar el perfil
                if (result is bool success && success)
                {
                    Debug.WriteLine("=== Condiciones actualizadas - refrescando perfil ===");
                    if (_viewModel.RefrescarPerfilCommand.CanExecute(null))
                    {
                        await _viewModel.RefrescarPerfilCommand.ExecuteAsync(null);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir modal de condiciones: {ex.Message}");
                await DisplayAlert("Error", "Error al abrir el modal de condiciones m�dicas", "OK");
            }
        }

        private void OnEditCondicionesClicked(object sender, EventArgs e)
        {
            _editandoCondiciones = !_editandoCondiciones;

            // Cambiar �cono del bot�n y mostrar/ocultar bot�n agregar
            if (_editandoCondiciones)
            {
                BtnEditarCondiciones.Text = "&#xE14C;"; // edit_off_24 (done/check icon)
                BtnEditarCondiciones.BackgroundColor = Color.FromArgb("#28a745");
                BtnAgregarCondicion.IsVisible = true;
            }
            else
            {
                BtnEditarCondiciones.Text = "&#xE3C9;"; // edit_24
                BtnEditarCondiciones.BackgroundColor = Color.FromArgb("#3b71ff");
                BtnAgregarCondicion.IsVisible = false;
            }
        }

        private void OnEditAlergiasClicked(object sender, EventArgs e)
        {
            _editandoAlergias = !_editandoAlergias;

            // Cambiar �cono del bot�n y mostrar/ocultar bot�n agregar
            if (_editandoAlergias)
            {
                BtnEditarAlergias.Text = "&#xE14C;"; // edit_off_24 (done/check icon)
                BtnEditarAlergias.BackgroundColor = Color.FromArgb("#28a745");
                BtnAgregarAlergia.IsVisible = true;
            }
            else
            {
                BtnEditarAlergias.Text = "&#xE3C9;"; // edit_24
                BtnEditarAlergias.BackgroundColor = Color.FromArgb("#dc3545");
                BtnAgregarAlergia.IsVisible = false;
            }
        }

        private void OnCondicionesMedicasSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Debug.WriteLine("=== Condiciones m�dicas seleccionadas ===");

                // Llamar al m�todo del ViewModel
                _viewModel.OnCondicionesMedicasSeleccionadas(sender, e);

                // Log de las selecciones
                Debug.WriteLine($"Condiciones seleccionadas: {e.CurrentSelection.Count}");
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

        private void OnAlergiasSeleccionadas(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Debug.WriteLine("=== Alergias seleccionadas ===");

                // Llamar al m�todo del ViewModel
                _viewModel.OnAlergiasSeleccionadas(sender, e);

                // Log de las selecciones
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

        private async void OnRefreshRequested(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("=== Refresh solicitado ===");

                if (_viewModel.RefrescarPerfilCommand.CanExecute(null))
                {
                    await _viewModel.RefrescarPerfilCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnRefreshRequested: {ex.Message}");
                await DisplayAlert("Error", "Error al refrescar el perfil", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("=== PantallaPerfil OnDisappearing ===");
        }

        // ELIMINADO: El RelayCommand que causaba conflicto
        // [RelayCommand]
        // private async Task EditarCondicionesMedicas() { ... }

        // Helper method to show alerts
        private async Task ShowAlertAsync(string title, string message)
        {
            await DisplayAlert(title, message, "OK");
        }
    }
}