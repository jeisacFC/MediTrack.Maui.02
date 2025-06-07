using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Interfaces;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediTrack.Frontend.Services.Implementaciones;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");

        // Cambiado a HTTPS - determina la URL base correcta dependiendo de la plataforma
        string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:44382"  // Para Android Emulator con HTTPS
            : "https://localhost:44382"; // Para otras plataformas con HTTPS

        _httpClient.BaseAddress = new Uri(baseUrl);

        // Configuración adicional para HTTPS en desarrollo (opcional)
        // Solo usar en desarrollo - NUNCA en producción
#if DEBUG
        // Esta configuración permite certificados SSL autofirmados en desarrollo
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        // Nota: Si usas IHttpClientFactory, esta configuración debe ir en el Startup/Program.cs
#endif
    }

    #region MEDICAMENTOS

    public async Task<ResEscanearMedicamento> EscanearMedicamentoAsync(ReqEscanearMedicamento request)
    {
        var endpoint = "api/medicamentos/escanear";

        try
        {
            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Debug.WriteLine($"Llamando a POST (HTTPS): {_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseStream = await response.Content.ReadAsStreamAsync();

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<ResEscanearMedicamento>(responseStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Error de API: {response.StatusCode}. Contenido: {errorContent}");
                return null;
            }
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"Error HTTPS en escanear medicamento: {httpEx.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error de conexión en ApiService: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region AUTENTICACIÓN

    public async Task<ResLogin> LoginAsync(ReqLogin request)
    {
        var endpoint = "api/usuarios/login";

        try
        {
            Debug.WriteLine("=== DATOS DE LOGIN ENVIADOS (HTTPS) ===");
            Debug.WriteLine($"Email: '{request.email}'");
            Debug.WriteLine($"Contraseña: '{request.contrasena}'");
            Debug.WriteLine("=======================================");

            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            Debug.WriteLine($"JSON Enviado: {jsonRequest}");

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Debug.WriteLine($"Llamando a POST (HTTPS): {_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"Status Code: {response.StatusCode}");
            Debug.WriteLine($"Response Headers: {response.Headers}");
            Debug.WriteLine($"Respuesta completa del servidor: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var resLogin = new ResLogin();

                    Debug.WriteLine("=== CAMPOS EN LA RESPUESTA ===");
                    foreach (var property in backendResponse.EnumerateObject())
                    {
                        Debug.WriteLine($"{property.Name}: {property.Value}");
                    }
                    Debug.WriteLine("===============================");

                    // Mapear campos básicos
                    if (backendResponse.TryGetProperty("resultado", out var resultado))
                        resLogin.resultado = resultado.GetBoolean();

                    if (backendResponse.TryGetProperty("mensaje", out var mensaje))
                        resLogin.Mensaje = mensaje.GetString();
                    else if (backendResponse.TryGetProperty("Mensaje", out var mensajeMayus))
                        resLogin.Mensaje = mensajeMayus.GetString();

                    // Mapear datos del usuario
                    if (backendResponse.TryGetProperty("idUsuario", out var idUsuario))
                        resLogin.IdUsuario = idUsuario.GetInt32();
                    else if (backendResponse.TryGetProperty("IdUsuario", out var idUsuarioMayus))
                        resLogin.IdUsuario = idUsuarioMayus.GetInt32();

                    if (backendResponse.TryGetProperty("email", out var email))
                        resLogin.Email = email.GetString();
                    else if (backendResponse.TryGetProperty("Email", out var emailMayus))
                        resLogin.Email = emailMayus.GetString();

                    if (backendResponse.TryGetProperty("nombre", out var nombre))
                        resLogin.Nombre = nombre.GetString();
                    else if (backendResponse.TryGetProperty("Nombre", out var nombreMayus))
                        resLogin.Nombre = nombreMayus.GetString();

                    if (backendResponse.TryGetProperty("apellido1", out var apellido1))
                        resLogin.Apellido1 = apellido1.GetString();
                    else if (backendResponse.TryGetProperty("Apellido1", out var apellido1Mayus))
                        resLogin.Apellido1 = apellido1Mayus.GetString();

                    if (backendResponse.TryGetProperty("apellido2", out var apellido2))
                        resLogin.Apellido2 = apellido2.GetString();
                    else if (backendResponse.TryGetProperty("Apellido2", out var apellido2Mayus))
                        resLogin.Apellido2 = apellido2Mayus.GetString();

                    // Mapear tokens
                    if (backendResponse.TryGetProperty("accessToken", out var accessToken))
                        resLogin.Token = accessToken.GetString();
                    else if (backendResponse.TryGetProperty("AccessToken", out var accessTokenMayus))
                        resLogin.Token = accessTokenMayus.GetString();
                    else if (backendResponse.TryGetProperty("token", out var token))
                        resLogin.Token = token.GetString();

                    if (backendResponse.TryGetProperty("refreshToken", out var refreshToken))
                        resLogin.RefreshToken = refreshToken.GetString();
                    else if (backendResponse.TryGetProperty("RefreshToken", out var refreshTokenMayus))
                        resLogin.RefreshToken = refreshTokenMayus.GetString();

                    Debug.WriteLine($"Resultado final mapeado - resultado: {resLogin.resultado}, mensaje: {resLogin.Mensaje}");
                    return resLogin;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deserializando respuesta exitosa: {ex.Message}");
                    return new ResLogin { resultado = false, Mensaje = "Error procesando respuesta del servidor" };
                }
            }
            else
            {
                Debug.WriteLine($"Error HTTP: {response.StatusCode}");
                Debug.WriteLine($"Contenido del error: {responseContent}");

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ResLogin>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (errorResponse != null)
                    {
                        Debug.WriteLine($"Error deserializado - resultado: {errorResponse.resultado}, mensaje: {errorResponse.Mensaje}");
                        return errorResponse;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deserializando respuesta de error: {ex.Message}");
                }

                return new ResLogin
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode} - {responseContent}"
                };
            }
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"Error HTTPS en login: {httpEx.Message}");
            return new ResLogin
            {
                resultado = false,
                Mensaje = "Error de conexión HTTPS. Verifica que el servidor esté corriendo con SSL."
            };
        }
        catch (TaskCanceledException timeoutEx)
        {
            Debug.WriteLine($"Timeout en login: {timeoutEx.Message}");
            return new ResLogin
            {
                resultado = false,
                Mensaje = "Tiempo de espera agotado. Intenta nuevamente."
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error general en login: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new ResLogin
            {
                resultado = false,
                Mensaje = "Error de conexión. No se pudo comunicar con el servidor."
            };
        }
    }

    #endregion
}