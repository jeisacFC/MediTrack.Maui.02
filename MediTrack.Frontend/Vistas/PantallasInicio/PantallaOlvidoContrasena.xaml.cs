using MediTrack.Frontend.ViewModels.PantallasInicio;
using MediTrack.Frontend.Popups;
using System;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.PantallasInicio;

public partial class PantallaOlvidoContrasena : ContentPage
{
    private OlvidoContrasenaViewModel _viewModel;

    public PantallaOlvidoContrasena(OlvidoContrasenaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Suscribirse a los eventos del ViewModel
        viewModel.CodigoEnviado += OnCodigoEnviado;
        viewModel.EnvioFallido += OnEnvioFallido;
    }

    // Manejadores de eventos del ViewModel
    private async void OnCodigoEnviado(object sender, string email)
    {
        // Mostrar mensaje de �xito brevemente
        await DisplayAlert("�C�digo enviado!", $"Se ha enviado un c�digo de verificaci�n a {email}", "OK");

        // Abrir modal de c�digo de verificaci�n
        await AbrirModalCodigoVerificacion(email);
    }

    private async void OnEnvioFallido(object sender, string mensaje)
    {
        await DisplayAlert("Error", mensaje, "OK");
    }

    // M�todo para abrir el modal de c�digo de verificaci�n - CORREGIDO
    private async Task AbrirModalCodigoVerificacion(string email)
    {
        try
        {
            // Crear directamente las instancias con los servicios
            var serviceProvider = Application.Current?.Handler?.MauiContext?.Services;

            if (serviceProvider == null)
            {
                await DisplayAlert("Error", "No se pudo acceder a los servicios", "OK");
                return;
            }

            var codigoViewModel = serviceProvider.GetService<CodigoVerificacionViewModel>();

            if (codigoViewModel == null)
            {
                await DisplayAlert("Error", "No se pudo crear el ViewModel", "OK");
                return;
            }

            // Crear el modal directamente
            var modalCodigo = new ModalCodigoVerificacion(codigoViewModel);

            // Inicializar el modal con el email
            modalCodigo.InicializarConEmail(email);

            // Suscribirse a los eventos del modal
            modalCodigo.CodigoVerificadoExitosamente += OnCodigoVerificadoExitosamente;
            modalCodigo.ModalCancelado += OnModalCodigoCancelado;

            // Mostrar el modal
            await Navigation.PushModalAsync(modalCodigo);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir el modal: {ex.Message}", "OK");
        }
    }

    // M�todo para abrir el modal de nueva contrase�a
    private async Task AbrirModalNuevaContrasena(string email)
    {
        System.Diagnostics.Debug.WriteLine("=== INICIO AbrirModalNuevaContrasena ===");
        System.Diagnostics.Debug.WriteLine($"Email recibido: '{email}'");

        try
        {
            System.Diagnostics.Debug.WriteLine("Obteniendo serviceProvider...");

            // Crear directamente las instancias con los servicios
            var serviceProvider = Application.Current?.Handler?.MauiContext?.Services;

            if (serviceProvider == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: serviceProvider es null");
                await DisplayAlert("Error", "No se pudo acceder a los servicios", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine("serviceProvider obtenido correctamente");
            System.Diagnostics.Debug.WriteLine("Obteniendo NuevaContrasenaViewModel...");

            var nuevaContrasenaViewModel = serviceProvider.GetService<NuevaContrasenaViewModel>();

            if (nuevaContrasenaViewModel == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: nuevaContrasenaViewModel es null");
                await DisplayAlert("Error", "No se pudo crear el ViewModel", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine("NuevaContrasenaViewModel obtenido correctamente");
            System.Diagnostics.Debug.WriteLine("Creando ModalNuevaContrasena...");

            // Crear el modal directamente
            var modalNuevaContrasena = new ModalNuevaContrasena(nuevaContrasenaViewModel);

            System.Diagnostics.Debug.WriteLine("ModalNuevaContrasena creado correctamente");
            System.Diagnostics.Debug.WriteLine($"Inicializando modal con email: '{email}'");

            // Inicializar el modal con el email
            modalNuevaContrasena.InicializarConEmail(email);

            System.Diagnostics.Debug.WriteLine("Modal inicializado");
            System.Diagnostics.Debug.WriteLine("Suscribiendo eventos...");

            // Suscribirse a los eventos del modal
            modalNuevaContrasena.ContrasenaActualizadaExitosamente += OnContrasenaActualizadaExitosamente;
            modalNuevaContrasena.ModalCancelado += OnModalNuevaContrasenaCancelado;

            System.Diagnostics.Debug.WriteLine("Eventos suscritos");
            System.Diagnostics.Debug.WriteLine("Mostrando modal...");

            // Mostrar el modal
            await Navigation.PushModalAsync(modalNuevaContrasena);

            System.Diagnostics.Debug.WriteLine("Modal mostrado exitosamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EXCEPCI�N en AbrirModalNuevaContrasena: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            await DisplayAlert("Error", $"No se pudo abrir el modal: {ex.Message}", "OK");
        }
        finally
        {
            System.Diagnostics.Debug.WriteLine("=== FIN AbrirModalNuevaContrasena ===");
        }
    }

    // Manejadores de eventos del modal de c�digo
    // Manejadores de eventos del modal de c�digo - SOLUCION CORRECTA
    private async void OnCodigoVerificadoExitosamente(object sender, string email)
    {
        System.Diagnostics.Debug.WriteLine($"=== OnCodigoVerificadoExitosamente INICIO ===");
        System.Diagnostics.Debug.WriteLine($"Email recibido: '{email}'");
        System.Diagnostics.Debug.WriteLine($"Sender: {sender?.GetType()?.Name}");

        // IMPORTANTE: Cerrar el modal actual ANTES de abrir el siguiente
        System.Diagnostics.Debug.WriteLine("Cerrando modal de c�digo actual...");

        // Agregar un peque�o delay para que la UI se actualice
        await Task.Delay(300);

        // El c�digo fue verificado exitosamente, abrir modal de nueva contrase�a
        System.Diagnostics.Debug.WriteLine("Llamando a AbrirModalNuevaContrasena...");
        await AbrirModalNuevaContrasena(email);

        System.Diagnostics.Debug.WriteLine("=== OnCodigoVerificadoExitosamente FIN ===");
    }

    private void OnModalCodigoCancelado(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== OnModalCodigoCancelado INICIO ===");
        System.Diagnostics.Debug.WriteLine($"Sender: {sender?.GetType()?.Name}");

        // El usuario cancel� el modal de c�digo, volver a la pantalla actual
        System.Diagnostics.Debug.WriteLine("Modal de c�digo cancelado");
        // El modal ya se cerr� autom�ticamente, no necesitamos hacer nada m�s

        System.Diagnostics.Debug.WriteLine("=== OnModalCodigoCancelado FIN ===");
    }

    // Manejadores de eventos del modal de nueva contrase�a
    private async void OnContrasenaActualizadaExitosamente(object sender, string mensaje)
    {
        // La contrase�a fue actualizada exitosamente
        // Mostrar mensaje final y regresar a inicio de sesi�n
        await DisplayAlert("�Proceso completado!",
            "Tu contrase�a ha sido actualizada correctamente. Ahora puedes iniciar sesi�n con tu nueva contrase�a.",
            "Ir a inicio de sesi�n");

        // Regresar a la pantalla de inicio de sesi�n
        await Shell.Current.GoToAsync("//inicioSesion");
    }

    private void OnModalNuevaContrasenaCancelado(object sender, EventArgs e)
    {
        // El usuario cancel� el modal de nueva contrase�a
        System.Diagnostics.Debug.WriteLine("Modal de nueva contrase�a cancelado");
        // El modal ya se cerr� autom�ticamente, no necesitamos hacer nada m�s
    }

    // Limpiar eventos al salir de la p�gina
    protected override void OnDisappearing()
    {
        if (_viewModel != null)
        {
            _viewModel.CodigoEnviado -= OnCodigoEnviado;
            _viewModel.EnvioFallido -= OnEnvioFallido;
        }
        base.OnDisappearing();
    }

    // MANTENER estos m�todos por compatibilidad con el XAML actual
    // pero que deleguen al ViewModel
    private async void EnviarEnlace(object sender, EventArgs e)
    {
        if (_viewModel?.EnviarCodigoCommand?.CanExecute(null) == true)
            await _viewModel.EnviarCodigoCommand.ExecuteAsync(null);
    }

    private async void VolverInicioSesion(object sender, EventArgs e)
    {
        if (_viewModel?.VolverInicioSesionCommand?.CanExecute(null) == true)
            await _viewModel.VolverInicioSesionCommand.ExecuteAsync(null);
    }
}