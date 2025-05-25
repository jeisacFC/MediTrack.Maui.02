using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MediTrack.Frontend.ViewModels;

public class PantallaInicioViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Medicamento> MedicamentosHoy { get; set; } = new();
    public ObservableCollection<string> EscaneosRecientes { get; set; } = new();
    public ObservableCollection<Recomendacion> Recomendaciones { get; set; } = new();
    public ObservableCollection<Sintoma> Sintomas { get; set; } = new();

    public PantallaInicioViewModel()
    {
        // Simular datos
        MedicamentosHoy.Add(new Medicamento("Paracetamol 500mg", "8:00 a. m.", true));
        MedicamentosHoy.Add(new Medicamento("Omeprazol 20mg", "5:00 p. m.", false));
        MedicamentosHoy.Add(new Medicamento("Ibuprofeno 200mg", "12:00 m. d.", false));

        EscaneosRecientes.Add("Divalproato Sódico 250mg - 5 de Abril, 7:23 p. m.");
        EscaneosRecientes.Add("Amoxicilina 500mg - 4 de Abril, 10:43 a. m.");

        Recomendaciones.Add(new Recomendacion
        {
            Titulo = "Recomendación Personalizada",
            Descripcion = "Según tu historial y síntomas recientes, podrías considerar el uso de Paracetamol 500 mg cada 8 horas por 3 días."
        });

        Sintomas.Add(new Sintoma("Dolor de cabeza"));
        Sintomas.Add(new Sintoma("Fiebre"));
        Sintomas.Add(new Sintoma("Náuseas"));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}

// Clases de apoyo
public class Medicamento : INotifyPropertyChanged
{
    public string Nombre { get; set; }
    public string Hora { get; set; }

    private bool _tomado;
    public bool Tomado
    {
        get => _tomado;
        set
        {
            _tomado = value;
            OnPropertyChanged();
        }
    }

    public Medicamento(string nombre, string hora, bool tomado)
    {
        Nombre = nombre;
        Hora = hora;
        Tomado = tomado;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}

public class Recomendacion
{
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
}

public class Sintoma : INotifyPropertyChanged
{
    public string Nombre { get; set; }

    private bool _seleccionado;
    public bool Seleccionado
    {
        get => _seleccionado;
        set
        {
            _seleccionado = value;
            OnPropertyChanged();
        }
    }

    public Sintoma(string nombre)
    {
        Nombre = nombre;
        Seleccionado = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}
