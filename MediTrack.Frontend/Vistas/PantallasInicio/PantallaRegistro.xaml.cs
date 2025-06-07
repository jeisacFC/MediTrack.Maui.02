using System;
using Microsoft.Maui.Controls;
using MediTrack.Frontend.ViewModels.PantallasInicio;
using MediTrack.Frontend.Models.Response;

namespace MediTrack.Frontend.Vistas.PantallasInicio
{
    public partial class PantallaRegistro : BaseContentPage
    {
        private readonly RegisterViewModel _viewModel;

        public PantallaRegistro(RegisterViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Suscribirse a eventos del ViewModel
            _viewModel.RegistroExitoso += OnRegistroExitoso;
            _viewModel.RegistroFallido += OnRegistroFallido;
        }

        protected override void OnDisappearing()
        {
            // Desuscribirse de eventos para evitar memory leaks
            _viewModel.RegistroExitoso -= OnRegistroExitoso;
            _viewModel.RegistroFallido -= OnRegistroFallido;
            base.OnDisappearing();
        }

        // Sobrescribir el método de la clase base para lógica personalizada
        protected override async Task<bool> HandleBackNavigationAsync()
        {
            // Si hay datos en el formulario, preguntar antes de salir
            if (HasFormData())
            {
                var result = await DisplayAlert("Confirmar",
                    "¿Deseas salir sin guardar los cambios?",
                    "Salir", "Continuar");

                if (result)
                {
                    // Navegar hacia atrás
                    await Shell.Current.GoToAsync("..");
                    return true; // Indica que se manejó la navegación
                }

                return false; // No salir, continuar en la página
            }

            // Si no hay datos, permitir navegación normal
            await Shell.Current.GoToAsync("..");
            return true;
        }

        private bool HasFormData()
        {
            // Verificar si hay datos en el formulario usando el ViewModel
            return !string.IsNullOrEmpty(_viewModel.Nombre) ||
                   !string.IsNullOrEmpty(_viewModel.Apellido1) ||
                   !string.IsNullOrEmpty(_viewModel.Email) ||
                   !string.IsNullOrEmpty(_viewModel.Contraseña);
        }

        private async void OnRegistroExitoso(object sender, ResRegister response)
        {
            // Mostrar mensaje de éxito
            await DisplayAlert("Registro Exitoso",
                $"¡Bienvenido! Tu cuenta ha sido creada exitosamente. ID de Usuario: {response.IdUsuario}",
                "OK");

            // Limpiar formulario
            _viewModel.LimpiarFormulario();

            // Redirigir a la pantalla de inicio de sesión
            await Shell.Current.GoToAsync("//inicioSesion");
        }

        private async void OnRegistroFallido(object sender, string errorMessage)
        {
            // Mostrar mensaje de error
            await DisplayAlert("Error en el Registro", errorMessage, "OK");
        }

        // Evento atado al botón "Registrarse" (opcional, si prefieres usar eventos en lugar de Command binding)
        private async void OnRegistrarseClicked(object sender, EventArgs e)
        {
            // Este método puede estar vacío si usas Command binding en XAML
            // O puedes llamar directamente al comando:
            if (_viewModel.RegisterCommand.CanExecute(null))
            {
                await _viewModel.RegisterCommand.ExecuteAsync(null);
            }
        }

        // Evento atado al botón "Iniciar sesión" en la parte inferior
        private async void IrAInicioSesion(object sender, EventArgs e)
        {
            await _viewModel.IrALoginCommand.ExecuteAsync(null);
        }

        // Método para validar formulario en tiempo real (opcional)
        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            // Aquí puedes agregar validaciones en tiempo real si lo deseas
            // Por ejemplo, mostrar mensajes de validación de contraseña
            if (sender is Entry entry && entry.StyleId == "ContraseñaEntry")
            {
                var mensajeValidacion = _viewModel.ObtenerMensajeValidacionContraseña();
                // Mostrar el mensaje en algún Label de validación
            }
        }
    }
}