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

        private string _paginaAnterior;
        private static int _instanceCounter = 0;
        private readonly int _instanceId;

        public NavigationService()
        {
            _instanceId = ++_instanceCounter;
            System.Diagnostics.Debug.WriteLine($"NEW NavigationService creado - ID: {_instanceId} (Singleton)");
        }

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
                System.Diagnostics.Debug.WriteLine($"GuardarPaginaActual - Instancia ID: {_instanceId}");

                var currentLocation = Shell.Current.CurrentState.Location.ToString();
                System.Diagnostics.Debug.WriteLine($"Location crudo: {currentLocation}");

                // Limpiar la ruta para obtener solo la página
                _paginaAnterior = LimpiarRuta(currentLocation);
                System.Diagnostics.Debug.WriteLine($"Página anterior guardada: '{_paginaAnterior}'");

                if (string.IsNullOrEmpty(_paginaAnterior))
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: _paginaAnterior está vacío!");
                }
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
                System.Diagnostics.Debug.WriteLine($"VolverAPaginaAnteriorAsync - Instancia ID: {_instanceId}");
                System.Diagnostics.Debug.WriteLine($"_paginaAnterior actual: '{_paginaAnterior}'");

                if (!string.IsNullOrEmpty(_paginaAnterior))
                {
                    System.Diagnostics.Debug.WriteLine($"Volviendo a: {_paginaAnterior}");
                    await Shell.Current.GoToAsync(_paginaAnterior, true);

                    // Limpiar después de usar
                    _paginaAnterior = string.Empty;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(" No hay página anterior, usando navegación normal");
                    await UsarNavegacionPorDefecto();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Error volviendo a página anterior: {ex.Message}");
                await UsarNavegacionPorDefecto();
            }
        }

        private async Task UsarNavegacionPorDefecto()
        {
            if (CanGoBack())
            {
                await GoBackAsync();
            }
            else
            {
                await Shell.Current.GoToAsync("///bienvenida", true);
            }
        }

        private string LimpiarRuta(string ruta)
        {
            if (string.IsNullOrEmpty(ruta))
                return string.Empty;

            // Remover parámetros de query si los hay
            var rutaLimpia = ruta.Split('?')[0];

            // Si es una ruta absoluta, mantenerla
            if (rutaLimpia.StartsWith("///") || rutaLimpia.StartsWith("//"))
                return rutaLimpia;

            return rutaLimpia;
        }
    }
}
