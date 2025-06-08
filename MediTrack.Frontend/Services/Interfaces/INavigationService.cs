using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Services.Interfaces
{
    public interface INavigationService
    {
        Task<bool> HandleBackNavigationAsync(ContentPage currentPage);
        Task GoBackAsync();
        Task GoToAsync(string route);
        bool CanGoBack();

        // NUEVOS MÉTODOS PARA EL ESCENARIO DE ESCANEO (VOLVER PAGINA ANTERIOR SIN ESPICIFAR RUTA)
        void GuardarPaginaActual();
        Task VolverAPaginaAnteriorAsync();
    }
}
