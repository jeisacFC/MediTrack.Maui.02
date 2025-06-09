using CommunityToolkit.Maui.Views;
using System.Linq;
using MediTrack.Frontend.Models.Response;

namespace MediTrack.Frontend.Popups;

public partial class InformacionMedicamentoEscaneo : Popup
{
    public ResEscanearMedicamento Medicamento { get; set; }

    public event EventHandler<ResEscanearMedicamento> MedicamentoAgregado;

    public InformacionMedicamentoEscaneo(ResEscanearMedicamento medicamento)
    {
        InitializeComponent();
        Medicamento = medicamento;
        CargarDatosMedicamento();
    }

    private void CargarDatosMedicamento()
    {
        // Cargar datos básicos
        NombreComercialLabel.Text = Medicamento.NombreComercial ?? "Medicamento sin nombre";
        PrincipioActivoLabel.Text = Medicamento.PrincipioActivo ?? "No especificado";
        DosisLabel.Text = Medicamento.Dosis ?? "No especificada";
        FabricanteLabel.Text = Medicamento.Fabricante ?? "No especificado";

        // Cargar usos
        CargarUsos();

        // Cargar advertencias
        CargarAdvertencias();
    }

    private void CargarUsos()
    {
        UsosContainer.Children.Clear();

        if (Medicamento.Usos != null && Medicamento.Usos.Any())
        {
            foreach (var uso in Medicamento.Usos)
            {
                var usoLayout = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 8
                };

                var bulletLabel = new Label
                {
                    Text = "•",
                    FontSize = 16,
                    TextColor = Color.FromArgb("#ff9800"),
                    VerticalTextAlignment = TextAlignment.Start,
                    FontAttributes = FontAttributes.Bold
                };

                var usoLabel = new Label
                {
                    Text = uso,
                    FontSize = 14,
                    TextColor = Color.FromArgb("#e65100"),
                    LineBreakMode = LineBreakMode.WordWrap,
                    VerticalTextAlignment = TextAlignment.Start
                };

                usoLayout.Children.Add(bulletLabel);
                usoLayout.Children.Add(usoLabel);
                UsosContainer.Children.Add(usoLayout);
            }
        }
        else
        {
            UsosSection.IsVisible = false;
        }
    }

    private void CargarAdvertencias()
    {
        AdvertenciasContainer.Children.Clear();

        if (Medicamento.Advertencias != null && Medicamento.Advertencias.Any())
        {
            foreach (var advertencia in Medicamento.Advertencias)
            {
                var advertenciaLayout = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 8
                };

                var warningLabel = new Label
                {
                    Text = "??",
                    FontSize = 14,
                    VerticalTextAlignment = TextAlignment.Start
                };

                var advertenciaLabel = new Label
                {
                    Text = advertencia,
                    FontSize = 13,
                    TextColor = Color.FromArgb("#d32f2f"),
                    LineBreakMode = LineBreakMode.WordWrap,
                    VerticalTextAlignment = TextAlignment.Start,
                    FontAttributes = FontAttributes.Bold
                };

                advertenciaLayout.Children.Add(warningLabel);
                advertenciaLayout.Children.Add(advertenciaLabel);
                AdvertenciasContainer.Children.Add(advertenciaLayout);
            }
        }
        else
        {
            AdvertenciasSection.IsVisible = false;
        }
    }

    private async void OnAgregarClicked(object sender, EventArgs e)
    {
        // Notificar que el usuario quiere agregar el medicamento
        MedicamentoAgregado?.Invoke(this, Medicamento);

        // Cerrar el popup
        await CloseAsync();
    }

    private async void OnCerrarClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}