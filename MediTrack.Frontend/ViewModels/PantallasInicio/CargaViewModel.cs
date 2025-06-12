using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.ViewModels.Base;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.Models.Request;
using System.Threading;

namespace MediTrack.Frontend.ViewModels.PantallasInicio
{
    public partial class CargaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private bool isAnimating = true;

        private CancellationTokenSource? _transitionCts;
        private readonly IApiService _apiService;

        public CargaViewModel(IApiService apiService)
        {
            Title = "Cargando MediTrack";
            _apiService = apiService;
        }

        public override async Task InitializeAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[CargaViewModel] ===== INICIANDO =====");

                await ExecuteAsync(async () =>
                {
                    _transitionCts = new CancellationTokenSource();
                    await IniciarTransicionOptimizada(_transitionCts.Token);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CargaViewModel] ERROR CRÍTICO en InitializeAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CargaViewModel] StackTrace: {ex.StackTrace}");

                // Fallback de emergencia
                await Task.Delay(1000);
                await NavegarABienvenida();
            }
        }

        private async Task IniciarTransicionOptimizada(CancellationToken cancellationToken)
        {
            try
            {
                // TIEMPO AÚN MÁS REDUCIDO - Solo 1.5 segundos para startup súper rápido
                await Task.Delay(1500, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    IsAnimating = false;

                    // Verificar sesión en paralelo con animación final
                    await VerificarSesionYNavegar();
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Transición cancelada correctamente");
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
                // Fallback: ir a bienvenida en caso de error
                await NavegarABienvenida();
            }
        }

        private async Task VerificarSesionYNavegar()
        {
            try
            {
                // Verificar si hay token guardado
                var token = await SecureStorage.GetAsync("jwt_token");
                var userId = await SecureStorage.GetAsync("user_id");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
                {
                    System.Diagnostics.Debug.WriteLine("Token encontrado - verificando validez...");

                    // VALIDAR TOKEN CON EL SERVIDOR
                    var esTokenValido = await ValidarTokenConServidor(token);

                    if (esTokenValido)
                    {
                        System.Diagnostics.Debug.WriteLine("Token válido - navegando a inicio");
                        await NavegarAInicio();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Token inválido - limpiando y navegando a bienvenida");
                        await LimpiarSesionExpirada();
                        await NavegarABienvenida();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Sin sesión guardada - navegando a bienvenida");
                    await NavegarABienvenida();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error verificando sesión: {ex.Message}");
                // En caso de error, limpiar sesión y ir a bienvenida
                await LimpiarSesionExpirada();
                await NavegarABienvenida();
            }
        }

        private async Task<bool> ValidarTokenConServidor(string token)
        {
            try
            {
                // PASO 1: Verificar si el token está expirado localmente (sin llamada al servidor)
                if (EsTokenExpiradoLocalmente(token))
                {
                    System.Diagnostics.Debug.WriteLine("Token expirado localmente");
                    return false;
                }

                // PASO 2: Validación súper rápida con timeout muy corto
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

                if (_apiService == null)
                {
                    System.Diagnostics.Debug.WriteLine("ApiService no disponible");
                    return false;
                }

                // VALIDACIÓN OPTIMIZADA: Solo verificar que el token no dé 401
                try
                {
                    var userRequest = new ReqObtenerUsuario();
                    var response = await _apiService.GetUserAsync(userRequest);

                    var esValido = response?.resultado == true;
                    System.Diagnostics.Debug.WriteLine($"Validación rápida: {(esValido ? "OK" : "FAIL")}");

                    return esValido;
                }
                catch (HttpRequestException httpEx) when (httpEx.Message.Contains("401") || httpEx.Message.Contains("Unauthorized"))
                {
                    System.Diagnostics.Debug.WriteLine("Token no autorizado - 401");
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Timeout validación - asumiendo válido para evitar bloqueo");
                // En caso de timeout, asumir válido y dejar que la app maneje errores durante uso
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validando token: {ex.Message}");
                // En caso de error de red, asumir válido para no bloquear
                return true;
            }
        }

        private bool EsTokenExpiradoLocalmente(string token)
        {
            try
            {
                // Parsear JWT básico para verificar expiración
                var parts = token.Split('.');
                if (parts.Length != 3) return true;

                var payload = parts[1];
                // Agregar padding si es necesario
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));

                // Buscar el campo "exp" en el JSON
                if (json.Contains("\"exp\":"))
                {
                    var expIndex = json.IndexOf("\"exp\":");
                    var expStart = json.IndexOf(':', expIndex) + 1;
                    var expEnd = json.IndexOfAny(new[] { ',', '}' }, expStart);

                    if (long.TryParse(json.Substring(expStart, expEnd - expStart).Trim(), out var exp))
                    {
                        var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                        var isExpired = DateTime.UtcNow > expDateTime;

                        System.Diagnostics.Debug.WriteLine($"Token expira: {expDateTime}, ¿Expirado?: {isExpired}");
                        return isExpired;
                    }
                }

                return false; // Si no se puede parsear, asumir que es válido
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parseando token: {ex.Message}");
                return false;
            }
        }

        private async Task LimpiarSesionExpirada()
        {
            try
            {
                // Limpiar todos los datos de sesión expirada
                SecureStorage.Remove("jwt_token");
                SecureStorage.Remove("refresh_token");
                SecureStorage.Remove("user_id");
                SecureStorage.Remove("user_email");
                SecureStorage.Remove("user_name");

                System.Diagnostics.Debug.WriteLine("Sesión expirada limpiada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error limpiando sesión: {ex.Message}");
            }
        }

        private async Task NavegarAInicio()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//inicio");
                });
                System.Diagnostics.Debug.WriteLine("Navegación a inicio completada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a inicio: {ex.Message}");
                await NavegarABienvenida();
            }
        }

        private async Task NavegarABienvenida()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//bienvenida");
                });
                System.Diagnostics.Debug.WriteLine("Navegación a bienvenida completada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a bienvenida: {ex.Message}");
                ErrorMessage = "Error de navegación";
            }
        }

        public void Cleanup()
        {
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = null;
        }

        [RelayCommand]
        private async Task SaltarCarga()
        {
            _transitionCts?.Cancel();
            await NavegarABienvenida();
        }
    }
}