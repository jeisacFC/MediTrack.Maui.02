using CommunityToolkit.Maui;
using MediTrack.Frontend.Popups;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels;
using MediTrack.Frontend.ViewModels.PantallasInicio;
using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Vistas.PantallasInicio;
using MediTrack.Frontend.Vistas.PantallasPrincipales;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using System.Globalization;
using System.Net.Http.Headers;
using ZXing.Net.Maui.Controls;

namespace MediTrack.Frontend;

public class AuthHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Obtener token del almacenamiento seguro
            var token = await SecureStorage.GetAsync("jwt_token");

            if (!string.IsNullOrEmpty(token))
            {
                // Agregar el header Authorization automáticamente
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                System.Diagnostics.Debug.WriteLine($"Token agregado automáticamente a la petición: {request.RequestUri}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No hay token disponible para: {request.RequestUri}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error agregando token: {ex.Message}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}