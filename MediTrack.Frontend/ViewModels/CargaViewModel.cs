using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MediTrack.Frontend.ViewModels;

public class CargaViewModel
{
    public CargaViewModel()
    {
        IniciarTransicion();
    }

    private async void IniciarTransicion()
    {
        await Task.Delay(2000);
        await Shell.Current.GoToAsync("//bienvenida");
    }
}