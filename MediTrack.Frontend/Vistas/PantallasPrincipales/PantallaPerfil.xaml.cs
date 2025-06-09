using MediTrack.Frontend.ViewModels;
using MediTrack.Frontend.Models.Model;
using System.Diagnostics;
using MediTrack.Frontend.Vistas.Base;

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

        // Manejo del scroll para hacer desaparecer el avatar
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

        // Manejo de edici�n de informaci�n personal
        private void OnEditInfoPersonalClicked(object sender, EventArgs e)
        {
            _editandoInfoPersonal = !_editandoInfoPersonal;

            // Cambiar �cono del bot�n
            if (_editandoInfoPersonal)
            {
                BtnEditarInfoPersonal.Text = "&#xE14C;"; // edit_off_24 (done/check icon)
                BtnEditarInfoPersonal.BackgroundColor = Color.FromArgb("#28a745");
            }
            else
            {
                BtnEditarInfoPersonal.Text = "&#xE3C9;"; // edit_24
                BtnEditarInfoPersonal.BackgroundColor = Color.FromArgb("#3b71ff");
            }
        }

        // Manejo de edici�n de condiciones m�dicas
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

        // Manejo de edici�n de alergias
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

        // Evento para manejar selecci�n de condiciones m�dicas
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

        // Evento para manejar selecci�n de alergias
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

        // M�todo para refrescar la p�gina cuando se hace pull-to-refresh (opcional)
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
    }
}