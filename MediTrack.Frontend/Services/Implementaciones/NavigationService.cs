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

        // propiedad para guardar la página anterior
        private string _paginaAnterior;

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
        // NUEVOS MÉTODOS PARA EL ESCENARIO DE ESCANEO
        public void GuardarPaginaActual()
        {
            try
            {
                _paginaAnterior = Shell.Current.CurrentState.Location.ToString();
                System.Diagnostics.Debug.WriteLine($"Página anterior guardada: {_paginaAnterior}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando página anterior: {ex.Message}");
            }
        }

        public async Task VolverAPaginaAnteriorAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_paginaAnterior))
                {
                    System.Diagnostics.Debug.WriteLine($"Volviendo a: {_paginaAnterior}");
                    await Shell.Current.GoToAsync(_paginaAnterior, true);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No hay página anterior, usando navegación normal");
                    if (CanGoBack())
                    {
                        await GoBackAsync();
                    }
                    else
                    {
                        await Shell.Current.GoToAsync("//MainPage", true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error volviendo a página anterior: {ex.Message}");
                // Fallback
                if (CanGoBack())
                {
                    await GoBackAsync();
                }
                else
                {
                    await Shell.Current.GoToAsync("//MainPage", true);
                }
            }
        }
    }
}
