using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Interfaces;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MediTrack.Frontend.Models.Model;

namespace MediTrack.Frontend.Services.Implementaciones;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");

        //NO cambiar, ip del backend en la nube
        string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://34.171.62.172:44382/"
            : "http://34.171.62.172:44382/";

        _httpClient.BaseAddress = new Uri(baseUrl);


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
#pragma warning disable CS8603 // Posible tipo de valor devuelto de referencia nulo
                return await JsonSerializer.DeserializeAsync<ResEscanearMedicamento>(responseStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
#pragma warning restore CS8603 // Posible tipo de valor devuelto de referencia nulo
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

    public async Task<ResRegister> RegisterAsync(ReqRegister request)
    {
        var endpoint = "api/usuarios/registrar";

        try
        {
            Debug.WriteLine("=== DATOS DE REGISTRO ENVIADOS (HTTPS) ===");
            Debug.WriteLine($"Email: '{request.Email}'");
            Debug.WriteLine($"Nombre: '{request.Nombre}'");
            Debug.WriteLine($"Apellido1: '{request.Apellido1}'");
            Debug.WriteLine($"Apellido2: '{request.Apellido2}'");
            Debug.WriteLine($"FechaNacimiento: '{request.FechaNacimiento}'");
            Debug.WriteLine($"IdGenero: '{request.IdGenero}'");
            Debug.WriteLine($"NotificacionesPush: '{request.NotificacionesPush}'");
            Debug.WriteLine("==========================================");

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
                    var resRegister = new ResRegister();

                    Debug.WriteLine("=== CAMPOS EN LA RESPUESTA ===");
                    foreach (var property in backendResponse.EnumerateObject())
                    {
                        Debug.WriteLine($"{property.Name}: {property.Value}");
                    }
                    Debug.WriteLine("===============================");

                    // Mapear campos básicos
                    if (backendResponse.TryGetProperty("resultado", out var resultado))
                        resRegister.resultado = resultado.GetBoolean();

                    if (backendResponse.TryGetProperty("mensaje", out var mensaje))
                        resRegister.Mensaje = mensaje.GetString();
                    else if (backendResponse.TryGetProperty("Mensaje", out var mensajeMayus))
                        resRegister.Mensaje = mensajeMayus.GetString();

                    // Mapear datos específicos del registro
                    if (backendResponse.TryGetProperty("idUsuario", out var idUsuario))
                        resRegister.IdUsuario = idUsuario.GetInt32();
                    else if (backendResponse.TryGetProperty("IdUsuario", out var idUsuarioMayus))
                        resRegister.IdUsuario = idUsuarioMayus.GetInt32();

                    if (backendResponse.TryGetProperty("fechaRegistro", out var fechaRegistro))
                        resRegister.FechaRegistro = fechaRegistro.GetDateTime();
                    else if (backendResponse.TryGetProperty("FechaRegistro", out var fechaRegistroMayus))
                        resRegister.FechaRegistro = fechaRegistroMayus.GetDateTime();

                    // Mapear errores si existen
                    if (backendResponse.TryGetProperty("errores", out var errores) && errores.ValueKind == JsonValueKind.Array)
                    {
                        var erroresList = new List<Error>();
                        foreach (var error in errores.EnumerateArray())
                        {
                            var errorDetail = new Error();
                            if (error.TryGetProperty("message", out var errorMessage))
                                errorDetail.Message = errorMessage.GetString();
                            else if (error.TryGetProperty("Message", out var errorMessageMayus))
                                errorDetail.Message = errorMessageMayus.GetString();

                            erroresList.Add(errorDetail);
                        }
                        resRegister.errores = erroresList;
                    }

                    Debug.WriteLine($"Resultado final mapeado - resultado: {resRegister.resultado}, mensaje: {resRegister.Mensaje}");
                    return resRegister;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deserializando respuesta exitosa: {ex.Message}");
                    return new ResRegister
                    {
                        resultado = false,
                        Mensaje = "Error procesando respuesta del servidor",
                        errores = new List<Error>
                    {
                        new Error { Message = "Error procesando respuesta del servidor" }
                    }
                    };
                }
            }
            else
            {
                Debug.WriteLine($"Error HTTP: {response.StatusCode}");
                Debug.WriteLine($"Contenido del error: {responseContent}");

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ResRegister>(responseContent, new JsonSerializerOptions
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

                return new ResRegister
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode} - {responseContent}",
                    errores = new List<Error>
                {
                    new Error { Message = $"Error del servidor: {response.StatusCode}" }
                }
                };
            }
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"Error HTTPS en registro: {httpEx.Message}");
            return new ResRegister
            {
                resultado = false,
                Mensaje = "Error de conexión HTTPS. Verifica que el servidor esté corriendo con SSL.",
                errores = new List<Error>
            {
                new Error { Message = "Error de conexión HTTPS" }
            }
            };
        }
        catch (TaskCanceledException timeoutEx)
        {
            Debug.WriteLine($"Timeout en registro: {timeoutEx.Message}");
            return new ResRegister
            {
                resultado = false,
                Mensaje = "Tiempo de espera agotado. Intenta nuevamente.",
                errores = new List<Error>
            {
                new Error { Message = "Tiempo de espera agotado" }
            }
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error general en registro: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new ResRegister
            {
                resultado = false,
                Mensaje = "Error de conexión. No se pudo comunicar con el servidor.",
                errores = new List<Error>
            {
                new Error { Message = "Error de conexión general" }
            }
            };
        }
    }

    #endregion


}