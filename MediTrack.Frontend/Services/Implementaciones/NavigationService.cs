using MediTrack.Frontend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Services.Implementaciones
{
    public class NavigationService : INavigationService
    {
        public async Task<bool> HandleBackNavigationAsync(ContentPage currentPage)
        {
            // Si la página implementa IBackNavigationHandler, usar su lógica
            if (currentPage is IBackNavigationHandler handler)
            {
                return await handler.OnBackNavigationAsync();
            }

            // Comportamiento por defecto
            if (CanGoBack())
            {
                await GoBackAsync();
                return true;
            }

            return false;
        }

        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task GoToAsync(string route)
        {
            await Shell.Current.GoToAsync(route);
        }

        public bool CanGoBack()
        {
            return Shell.Current.Navigation.NavigationStack.Count > 1;
        }
    }
}
