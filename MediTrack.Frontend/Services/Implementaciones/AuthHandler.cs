using System.Net.Http.Headers;
using System.Diagnostics;

namespace MediTrack.Frontend.Services.Implementaciones
{
    public class AuthHandler : DelegatingHandler
    {
        private string? _cachedToken = null;
        private DateTime _lastTokenCheck = DateTime.MinValue;
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);
        private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // PASO 1: Asegurar que el token está configurado
                await EnsureTokenIsSetAsync(request);

                // PASO 2: Hacer la petición
                var response = await base.SendAsync(request, cancellationToken);

                // PASO 3: Manejar 401 Unauthorized
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine($"[AuthHandler] 401 Unauthorized en {request.RequestUri} - intentando refrescar token");

                    // Limpiar cache y recargar token
                    await RefreshTokenCacheAsync();
                    await EnsureTokenIsSetAsync(request);

                    // Reintentar la petición una sola vez
                    response.Dispose(); // Liberar la respuesta anterior
                    response = await base.SendAsync(request, cancellationToken);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("[AuthHandler] ❌ Segundo intento también falló - token probablemente expirado");
                    }
                    else
                    {
                        Debug.WriteLine("[AuthHandler] ✅ Reintento exitoso después de refrescar token");
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthHandler] Error procesando petición: {ex.Message}");
                throw;
            }
        }

        private async Task EnsureTokenIsSetAsync(HttpRequestMessage request)
        {
            await _tokenSemaphore.WaitAsync();
            try
            {
                // Verificar si necesitamos recargar el token
                var needsRefresh = _cachedToken == null ||
                                 DateTime.Now - _lastTokenCheck > _cacheTimeout;

                if (needsRefresh)
                {
                    try
                    {
                        _cachedToken = await SecureStorage.GetAsync("jwt_token");
                        _lastTokenCheck = DateTime.Now;

                        var tokenStatus = !string.IsNullOrEmpty(_cachedToken) ? "✓ Token cargado" : "⚠️ Sin token";
                        Debug.WriteLine($"[AuthHandler] {tokenStatus} para {request.RequestUri?.Host}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[AuthHandler] Error obteniendo token: {ex.Message}");
                        _cachedToken = null;
                    }
                }

                // Aplicar o remover header de autorización
                if (!string.IsNullOrEmpty(_cachedToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cachedToken);
                }
                else
                {
                    request.Headers.Authorization = null;
                }
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }

        private async Task RefreshTokenCacheAsync()
        {
            await _tokenSemaphore.WaitAsync();
            try
            {
                _cachedToken = null;
                _lastTokenCheck = DateTime.MinValue;
                Debug.WriteLine("[AuthHandler] Cache de token limpiado para recarga");
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tokenSemaphore?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}