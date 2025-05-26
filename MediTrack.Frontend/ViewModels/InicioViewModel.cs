using System.Collections.ObjectModel;
using MediTrack.Frontend.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MediTrack.Frontend.ViewModels
{
    public partial class InicioViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<MedicamentoModel> medicamentosHoy;

        public InicioViewModel()
        {
            MedicamentosHoy = new ObservableCollection<MedicamentoModel>
            {
                new MedicamentoModel { Nombre = "Paracetamol 500mg", Hora = "8:00 a. m.", Tomado = true },
                new MedicamentoModel { Nombre = "Omeprazol 20mg", Hora = "5:00 p. m.", Tomado = false },
                new MedicamentoModel { Nombre = "Ibuprofeno 200mg", Hora = "12:00 m. d.", Tomado = false }
            };
        }

        [RelayCommand]
        public void CambiarEstado(MedicamentoModel medicamento)
        {
            medicamento.Tomado = !medicamento.Tomado;
        }
    }
}