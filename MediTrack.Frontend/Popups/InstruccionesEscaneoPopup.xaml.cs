using CommunityToolkit.Maui.Views; // Necesario para Popup

namespace MediTrack.Frontend.Popups;

public partial class InstruccionesEscaneoPopup : Popup
{
    public InstruccionesEscaneoPopup()
    {
        InitializeComponent();
    }

    // Método para cerrar el popup
    private void Entendido_Clicked(object sender, EventArgs e)
    {
        Close(); // Cierra el popup
    }
}