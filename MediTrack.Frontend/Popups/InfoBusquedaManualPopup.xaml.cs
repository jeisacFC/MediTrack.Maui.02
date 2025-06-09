using CommunityToolkit.Maui.Views;
using MediTrack.Frontend.Models.Response;
using System.Linq;

namespace MediTrack.Frontend.Popups
{
    public partial class InfoBusquedaManualPopup : Popup
    {
        // Almacena el resultado completo de la búsqueda
        public ResBuscarMedicamento ResultadoBusqueda { get; set; }

        // Evento para notificar cuando el usuario quiere agregar el medicamento a su perfil
        public event EventHandler<ResBuscarMedicamento> MedicamentoAgregado;

        // El constructor ahora acepta el objeto de respuesta de la búsqueda manual
        public InfoBusquedaManualPopup(ResBuscarMedicamento resultado)
        {
            InitializeComponent();
            ResultadoBusqueda = resultado;

            // Asignamos el objeto al BindingContext para que el XAML pueda usarlo
            BindingContext = ResultadoBusqueda;

            // Llamamos a un método para poblar los campos que no se pueden enlazar directamente (listas)
            CargarDatosDinamicos();
        }

        private void CargarDatosDinamicos()
        {
            if (ResultadoBusqueda == null) return;

            // Cargar usos, advertencias y efectos secundarios desde el code-behind
            CargarListaEnContenedor(UsosContainer, ResultadoBusqueda.Usos, UsosSection, "#ff9800", "#e65100");
            CargarListaEnContenedor(AdvertenciasContainer, ResultadoBusqueda.Advertencias, AdvertenciasSection, "#d32f2f", "#d32f2f", true);
            CargarListaEnContenedor(EfectosSecundariosContainer, ResultadoBusqueda.EfectosSecundarios, EfectosSecundariosSection, "#673ab7", "#4527a0");
        }

        // Método genérico para rellenar las listas de Usos, Advertencias, etc.
        private void CargarListaEnContenedor(StackLayout container, List<string> items, VisualElement section, string bulletColor, string textColor, bool isWarning = false)
        {
            container.Children.Clear();

            if (items != null && items.Any())
            {
                section.IsVisible = true;
                foreach (var item in items)
                {
                    var itemLayout = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 8 };
                    var bulletLabel = new Label
                    {
                        Text = isWarning ? "-" : "-",
                        FontSize = 16,
                        TextColor = Color.FromArgb(bulletColor),
                        VerticalTextAlignment = TextAlignment.Start,
                        FontAttributes = FontAttributes.Bold
                    };
                    var itemLabel = new Label
                    {
                        Text = item,
                        FontSize = 14,
                        TextColor = Color.FromArgb(textColor),
                        LineBreakMode = LineBreakMode.WordWrap
                    };

                    itemLayout.Children.Add(bulletLabel);
                    itemLayout.Children.Add(itemLabel);
                    container.Children.Add(itemLayout);
                }
            }
            else
            {
                section.IsVisible = false;
            }
        }

        private void OnAgregarClicked(object sender, EventArgs e)
        {
            // Notificamos con el objeto completo del resultado de la búsqueda
            MedicamentoAgregado?.Invoke(this, ResultadoBusqueda);
            Close();
        }

        private void OnCerrarClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}