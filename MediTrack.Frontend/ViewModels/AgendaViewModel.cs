using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace MediTrack.Frontend.ViewModels;

public partial class AgendaViewModel : ObservableObject
{
    [ObservableProperty]
    private DateTime fechaSeleccionada = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<string> eventosDelDia = new();

    public AgendaViewModel()
    {
        CargarEventosDelDia();
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(FechaSeleccionada))
                CargarEventosDelDia();
        };
    }

    private void CargarEventosDelDia()
    {
        EventosDelDia.Clear();

        if (FechaSeleccionada.Date == DateTime.Today)
        {
            EventosDelDia.Add("Paracetamol 500mg - 8:00 a.m.");
            EventosDelDia.Add("Omeprazol 20mg - 5:00 p.m.");
        }
        else
        {
            EventosDelDia.Add("No hay eventos para esta fecha.");
        }
    }
}
