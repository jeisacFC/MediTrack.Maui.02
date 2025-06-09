using MediTrack.Frontend.Services.Interfaces;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Vistas.Base
{
    public abstract class BaseContentView : ContentView
    {
        protected INavigationService NavigationService { get; }

        public BaseContentView()
        {
            NavigationService = GetService<INavigationService>();
        }

        protected T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }

        //  Acceso al ServiceProvider (igual que en BaseContentPage)
        private static IServiceProvider ServiceProvider =>
#if WINDOWS10_0_17763_0_OR_GREATER
            MauiWinUIApplication.Current.Services;
#elif ANDROID
            MauiApplication.Current.Services;
#elif IOS || MACCATALYST
            MauiUIApplicationDelegate.Current.Services;
#else
            throw new PlatformNotSupportedException();
#endif
    }
}