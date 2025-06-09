using Microsoft.Maui.Controls;
using MediTrack.Frontend.Vistas.Base;

namespace MediTrack.Frontend.Vistas;

public partial class BotonFlotanteEscaner : BaseContentView
{
    public BotonFlotanteEscaner()
    {
        InitializeComponent();
    }

    private async void OnFabClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== INICIO OnFabClicked ===");
        try
        {
            // Animación del botón
            await FabButton.ScaleTo(0.8, 100);
            await FabButton.ScaleTo(1.0, 100);

            System.Diagnostics.Debug.WriteLine($"Página actual antes de guardar: {Shell.Current.CurrentState.Location}");

            // Ahora puedes usar NavigationService directamente
            NavigationService.GuardarPaginaActual();

            System.Diagnostics.Debug.WriteLine("Navegando a escaneo...");

            // Navegar a la pantalla de escaneo
            await NavigationService.GoToAsync("///pantallaScan");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navegando: {ex.Message}");
        }
    }
}