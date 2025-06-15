using System.Net.Http.Headers;
using System.Diagnostics;

namespace MediTrack.Frontend.Services.Implementaciones
{
    public class AuthHandler : DelegatingHandler
    {
        private string? _cachedToken = null;
        private DateTime _lastTokenCheck = DateTime.MinValue;
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(10); // ✅ Incrementado de 5 a 10 minutos
        private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);
        private bool _isRefreshing = false;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ PASO 1: Solo configurar token si no está ya configurado
                await EnsureTokenIsSetAsync(request);

                // ✅ PASO 2: Hacer la petición
                var response = await base.SendAsync(request, cancellationToken);

                // ✅ PASO 3: Solo manejar 401 si no estamos ya refrescando
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !_isRefreshing)
                {
                    Debug.WriteLine($"[AuthHandler] 401 Unauthorized en {request.RequestUri?.PathAndQuery} - intentando refrescar token");

                    _isRefreshing = true;
                    try
                    {
                        // Limpiar cache y recargar token
                        await RefreshTokenCacheAsync();
                        await EnsureTokenIsSetAsync(request);

                        // Reintentar la petición una sola vez
                        response.Dispose();
                        response = await base.SendAsync(request, cancellationToken);

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            Debug.WriteLine("[AuthHandler] ❌ Token definitivamente expirado");
                        }
                        else
                        {
                            Debug.WriteLine("[AuthHandler] ✅ Reintento exitoso");
                        }
                    }
                    finally
                    {
                        _isRefreshing = false;
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
            // ✅ Solo obtener token si realmente necesitamos uno nuevo
            if (_cachedToken == null || DateTime.Now - _lastTokenCheck > _cacheTimeout)
            {
                await _tokenSemaphore.WaitAsync();
                try
                {
                    // ✅ Double-check pattern para evitar múltiples lecturas
                    if (_cachedToken == null || DateTime.Now - _lastTokenCheck > _cacheTimeout)
                    {
                        try
                        {
                            _cachedToken = await SecureStorage.GetAsync("jwt_token");
                            _lastTokenCheck = DateTime.Now;

                            if (!string.IsNullOrEmpty(_cachedToken))
                            {
                                Debug.WriteLine($"[AuthHandler] ✓ Token cargado para {request.RequestUri?.Host}");
                            }
                            else
                            {
                                Debug.WriteLine($"[AuthHandler] ⚠️ Sin token para {request.RequestUri?.Host}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[AuthHandler] Error obteniendo token: {ex.Message}");
                            _cachedToken = null;
                        }
                    }
                }
                finally
                {
                    _tokenSemaphore.Release();
                }
            }

            // ✅ Aplicar header solo si tenemos token
            if (!string.IsNullOrEmpty(_cachedToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cachedToken);
            }
            else
            {
                request.Headers.Authorization = null;
            }
        }

        private async Task RefreshTokenCacheAsync()
        {
            await _tokenSemaphore.WaitAsync();
            try
            {
                _cachedToken = null;
                _lastTokenCheck = DateTime.MinValue;
                Debug.WriteLine("[AuthHandler] Cache de token limpiado");
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