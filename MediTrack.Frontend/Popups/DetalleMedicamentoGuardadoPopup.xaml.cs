using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model; // Usamos el modelo que ya tienes
using MediTrack.Frontend.Models.Response;

namespace MediTrack.Frontend.Popups
{
    public partial class DetalleMedicamentoGuardadoPopup : Popup
    {
        // Almacenamos el comando que viene desde el ViewModel
        private readonly IAsyncRelayCommand<UsuarioMedicamentos> _eliminarCommand;

        // Almacenamos una referencia al medicamento para poder pasarlo al comando de eliminar
        private readonly UsuarioMedicamentos _medicamentoParaEliminar;

        public DetalleMedicamentoGuardadoPopup(ResDetalleMedicamentoUsuario detalle, IAsyncRelayCommand<UsuarioMedicamentos> eliminarCommand)
        {
            InitializeComponent();

            // Usamos el objeto de detalle como el contexto para que el XAML muestre los datos
            BindingContext = detalle;

            // Guardamos el comando y el objeto que necesitaremos si el usuario presiona "Eliminar"
            _eliminarCommand = eliminarCommand;
            _medicamentoParaEliminar = new UsuarioMedicamentos
            {
                id_medicamento = detalle.Medicamento.IdMedicamento,
                nombre_comercial = detalle.Medicamento.Nombre,
                dosis = detalle.Medicamento.Dosis,
                // Añade más propiedades si el comando las necesita
            };
        }

        private async void Eliminar_Clicked(object sender, EventArgs e)
        {
            // Cerramos el popup ANTES de ejecutar la acción
            await CloseAsync();

            // Ejecutamos el comando de eliminación que vive en el BusquedaViewModel
            if (_eliminarCommand != null)
            {
                await _eliminarCommand.ExecuteAsync(_medicamentoParaEliminar);
            }
        }

        private void Cerrar_Clicked(object sender, EventArgs e)
        {
            // Simplemente cierra el popup
            Close();
        }
    }
}