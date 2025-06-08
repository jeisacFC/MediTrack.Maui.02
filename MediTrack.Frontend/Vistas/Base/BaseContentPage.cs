using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediTrack.Frontend.Services.Interfaces;

namespace MediTrack.Frontend.Vistas.Base
{ 
    public abstract class BaseContentPage : ContentPage
{
    protected INavigationService NavigationService { get; }

    public BaseContentPage()
    {
        NavigationService = GetService<INavigationService>();
    }

    protected override bool OnBackButtonPressed()
    {
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

    protected T GetService<T>()
    {
        return ServiceProvider.GetService<T>();
    }

    // ✅ Acceso al ServiceProvider
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