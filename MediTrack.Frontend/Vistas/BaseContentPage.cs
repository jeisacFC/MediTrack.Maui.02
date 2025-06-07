using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Services.Interfaces;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Vistas
{
    public abstract class BaseContentPage : ContentPage
    {
        protected INavigationService NavigationService { get; }

        public BaseContentPage()
        {
            NavigationService = DependencyService.Get<INavigationService>() ??
                               new NavigationService();
        }

        protected override bool OnBackButtonPressed()
        {
            // Ejecutar de forma asíncrona sin bloquear
            Device.BeginInvokeOnMainThread(async () =>
            {
                await NavigationService.HandleBackNavigationAsync(this);
            });


            return true; 
        }

        protected virtual async Task<bool> HandleBackNavigationAsync()
        {
            return await NavigationService.HandleBackNavigationAsync(this);
        }
    }
}
