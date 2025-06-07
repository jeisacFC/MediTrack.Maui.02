using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Services.Interfaces;
using System.Diagnostics;
using System.Net;
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

        //NO cambiar, ip del backend en la nube
        string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://34.171.62.172:44382/"
            : "http://34.171.62.172:44382/";

        _httpClient.BaseAddress = new Uri(baseUrl);

        _ = Task.Run(async () => await ConfigurarTokenAsync());
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

                    if (!string.IsNullOrEmpty(resLogin.Token))
                    {
                        // Guardar el access token
                        await SecureStorage.SetAsync("jwt_token", resLogin.Token);

                        // Guardar el refresh token si existe
                        if (!string.IsNullOrEmpty(resLogin.RefreshToken))
                        {
                            await SecureStorage.SetAsync("refresh_token", resLogin.RefreshToken);
                        }

                        // Guardar datos del usuario para el logout
                        if (resLogin.IdUsuario > 0)
                        {
                            await SecureStorage.SetAsync("user_id", resLogin.IdUsuario.ToString());
                        }

                        if (!string.IsNullOrEmpty(resLogin.Email))
                        {
                            await SecureStorage.SetAsync("user_email", resLogin.Email);
                        }

                        if (!string.IsNullOrEmpty(resLogin.Nombre))
                        {
                            await SecureStorage.SetAsync("user_name", resLogin.Nombre);
                        }

                        Debug.WriteLine("=== DATOS DE SESIÓN GUARDADOS ===");
                        Debug.WriteLine($"Access Token: {resLogin.Token}");
                        Debug.WriteLine($"Refresh Token: {resLogin.RefreshToken}");
                        Debug.WriteLine($"User ID: {resLogin.IdUsuario}");
                        Debug.WriteLine($"Email: {resLogin.Email}");
                        Debug.WriteLine("=================================");
                    }

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
    public async Task<ResObtenerUsuario> GetUserAsync(ReqObtenerUsuario request)
    {
        var endpoint = "api/usuarios/perfil";

        try
        {

            Debug.WriteLine("=== DATOS DE OBTENER USUARIO ENVIADOS (HTTPS) ===");
            Debug.WriteLine($"IdUsuario: '{request.IdUsuario}'");
            Debug.WriteLine($"Token configurado: {_httpClient.DefaultRequestHeaders.Authorization != null}");
            Debug.WriteLine("=================================================");

            Debug.WriteLine($"Llamando a GET (HTTPS): {_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.GetAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"Status Code: {response.StatusCode}");
            Debug.WriteLine($"Response Headers: {response.Headers}");
            Debug.WriteLine($"Respuesta completa del servidor: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var resObtenerUsuario = new ResObtenerUsuario();

                    Debug.WriteLine("=== CAMPOS EN LA RESPUESTA ===");
                    foreach (var property in backendResponse.EnumerateObject())
                    {
                        Debug.WriteLine($"{property.Name}: {property.Value}");
                    }
                    Debug.WriteLine("===============================");

                    // Mapear campos básicos
                    if (backendResponse.TryGetProperty("resultado", out var resultado))
                        resObtenerUsuario.resultado = resultado.GetBoolean();

                    if (backendResponse.TryGetProperty("mensaje", out var mensaje))
                        resObtenerUsuario.Mensaje = mensaje.GetString();
                    else if (backendResponse.TryGetProperty("Mensaje", out var mensajeMayus))
                        resObtenerUsuario.Mensaje = mensajeMayus.GetString();

                    // Mapear datos del usuario si existen
                    if (backendResponse.TryGetProperty("usuario", out var usuario) && usuario.ValueKind == JsonValueKind.Object)
                    {
                        var usuarioObj = new Usuario();

                        if (usuario.TryGetProperty("id_usuario", out var idUsuario))
                            usuarioObj.id_usuario = idUsuario.GetInt32();
                        else if (usuario.TryGetProperty("Id_Usuario", out var idUsuarioMayus))
                            usuarioObj.id_usuario = idUsuarioMayus.GetInt32();

                        if (usuario.TryGetProperty("email", out var email))
                            usuarioObj.email = email.GetString();
                        else if (usuario.TryGetProperty("Email", out var emailMayus))
                            usuarioObj.email = emailMayus.GetString();

                        if (usuario.TryGetProperty("contraseña", out var contraseña))
                            usuarioObj.contraseña = contraseña.GetString();
                        else if (usuario.TryGetProperty("Contraseña", out var contraseñaMayus))
                            usuarioObj.contraseña = contraseñaMayus.GetString();

                        if (usuario.TryGetProperty("fecha_registro", out var fechaRegistro))
                            usuarioObj.fecha_registro = fechaRegistro.GetDateTime();
                        else if (usuario.TryGetProperty("Fecha_Registro", out var fechaRegistroMayus))
                            usuarioObj.fecha_registro = fechaRegistroMayus.GetDateTime();

                        if (usuario.TryGetProperty("notificaciones_push", out var notificacionesPush))
                            usuarioObj.notificaciones_push = notificacionesPush.GetBoolean();
                        else if (usuario.TryGetProperty("Notificaciones_Push", out var notificacionesPushMayus))
                            usuarioObj.notificaciones_push = notificacionesPushMayus.GetBoolean();

                        if (usuario.TryGetProperty("ultimo_acceso", out var ultimoAcceso))
                            usuarioObj.ultimo_acceso = ultimoAcceso.GetDateTime();
                        else if (usuario.TryGetProperty("Ultimo_Acceso", out var ultimoAccesoMayus))
                            usuarioObj.ultimo_acceso = ultimoAccesoMayus.GetDateTime();

                        if (usuario.TryGetProperty("intentos_fallidos", out var intentosFallidos))
                            usuarioObj.intentos_fallidos = intentosFallidos.GetInt32();
                        else if (usuario.TryGetProperty("Intentos_Fallidos", out var intentosfallidosMayus))
                            usuarioObj.intentos_fallidos = intentosfallidosMayus.GetInt32();

                        if (usuario.TryGetProperty("cuenta_bloqueada", out var cuentaBloqueada))
                            usuarioObj.cuenta_bloqueada = cuentaBloqueada.GetBoolean();
                        else if (usuario.TryGetProperty("Cuenta_Bloqueada", out var cuentaBloqueadaMayus))
                            usuarioObj.cuenta_bloqueada = cuentaBloqueadaMayus.GetBoolean();

                        if (usuario.TryGetProperty("nombre", out var nombre))
                            usuarioObj.nombre = nombre.GetString();
                        else if (usuario.TryGetProperty("Nombre", out var nombreMayus))
                            usuarioObj.nombre = nombreMayus.GetString();

                        if (usuario.TryGetProperty("apellido1", out var apellido1))
                            usuarioObj.apellido1 = apellido1.GetString();
                        else if (usuario.TryGetProperty("Apellido1", out var apellido1Mayus))
                            usuarioObj.apellido1 = apellido1Mayus.GetString();

                        if (usuario.TryGetProperty("apellido2", out var apellido2))
                            usuarioObj.apellido2 = apellido2.GetString();
                        else if (usuario.TryGetProperty("Apellido2", out var apellido2Mayus))
                            usuarioObj.apellido2 = apellido2Mayus.GetString();

                        if (usuario.TryGetProperty("fecha_nacimiento", out var fechaNacimiento))
                            usuarioObj.fecha_nacimiento = fechaNacimiento.GetDateTime();
                        else if (usuario.TryGetProperty("Fecha_Nacimiento", out var fechaNacimientoMayus))
                            usuarioObj.fecha_nacimiento = fechaNacimientoMayus.GetDateTime();

                        if (usuario.TryGetProperty("id_genero", out var idGenero))
                            usuarioObj.id_genero = idGenero.GetString();
                        else if (usuario.TryGetProperty("Id_Genero", out var idGeneroMayus))
                            usuarioObj.id_genero = idGeneroMayus.GetString();

                        resObtenerUsuario.Usuario = usuarioObj;
                    }

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
                        resObtenerUsuario.errores = erroresList;
                    }

                    Debug.WriteLine($"Resultado final mapeado - resultado: {resObtenerUsuario.resultado}, mensaje: {resObtenerUsuario.Mensaje}");
                    return resObtenerUsuario;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deserializando respuesta exitosa: {ex.Message}");
                    return new ResObtenerUsuario
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
                    var errorResponse = JsonSerializer.Deserialize<ResObtenerUsuario>(responseContent, new JsonSerializerOptions
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

                // Manejar códigos de estado específicos
                string mensajeError = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Usuario no autenticado",
                    HttpStatusCode.NotFound => "Usuario no encontrado",
                    HttpStatusCode.Forbidden => "No tienes permisos para acceder a este recurso",
                    _ => $"Error del servidor: {response.StatusCode} - {responseContent}"
                };

                return new ResObtenerUsuario
                {
                    resultado = false,
                    Mensaje = mensajeError,
                    errores = new List<Error>
                {
                    new Error { Message = $"Error del servidor: {response.StatusCode}" }
                }
                };
            }
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"Error HTTPS en obtener usuario: {httpEx.Message}");
            return new ResObtenerUsuario
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
            Debug.WriteLine($"Timeout en obtener usuario: {timeoutEx.Message}");
            return new ResObtenerUsuario
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
            Debug.WriteLine($"Error general en obtener usuario: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new ResObtenerUsuario
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
    public async Task<ResLogout> LogoutAsync(ReqLogout request)
    {
        var endpoint = "api/usuarios/logout";

        try
        {
            // Crear request si no se proporciona
            if (request == null)
            {
                request = new ReqLogout { InvalidarTodos = false };
            }

            // Obtener datos guardados para completar el request
            var userIdStr = await SecureStorage.GetAsync("user_id");
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var userId))
            {
                request.IdUsuario = userId;
            }

            var accessToken = await SecureStorage.GetAsync("jwt_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.AccessToken = accessToken;
            }

            var refreshToken = await SecureStorage.GetAsync("refresh_token");
            if (!string.IsNullOrEmpty(refreshToken))
            {
                request.RefreshToken = refreshToken;
            }

            Debug.WriteLine("=== DATOS DE LOGOUT ENVIADOS ===");
            Debug.WriteLine($"IdUsuario: {request.IdUsuario}");
            Debug.WriteLine($"InvalidarTodos: {request.InvalidarTodos}");
            Debug.WriteLine($"AccessToken presente: {!string.IsNullOrEmpty(request.AccessToken)}");
            Debug.WriteLine($"RefreshToken presente: {!string.IsNullOrEmpty(request.RefreshToken)}");
            Debug.WriteLine("=================================");

            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            Debug.WriteLine($"JSON Enviado: {jsonRequest}");

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Debug.WriteLine($"Llamando a POST: {_httpClient.BaseAddress}{endpoint}");

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
                    var resLogout = new ResLogout();

                    Debug.WriteLine("=== CAMPOS EN LA RESPUESTA DE LOGOUT ===");
                    foreach (var property in backendResponse.EnumerateObject())
                    {
                        Debug.WriteLine($"{property.Name}: {property.Value}");
                    }
                    Debug.WriteLine("========================================");

                    // Mapear campos básicos
                    if (backendResponse.TryGetProperty("resultado", out var resultado))
                        resLogout.resultado = resultado.GetBoolean();

                    if (backendResponse.TryGetProperty("mensaje", out var mensaje))
                        resLogout.Mensaje = mensaje.GetString();
                    else if (backendResponse.TryGetProperty("Mensaje", out var mensajeMayus))
                        resLogout.Mensaje = mensajeMayus.GetString();

                    // Mapear campos específicos de logout
                    if (backendResponse.TryGetProperty("logoutExitoso", out var logoutExitoso))
                        resLogout.LogoutExitoso = logoutExitoso.GetBoolean();
                    else if (backendResponse.TryGetProperty("LogoutExitoso", out var logoutExitosoMayus))
                        resLogout.LogoutExitoso = logoutExitosoMayus.GetBoolean();

                    if (backendResponse.TryGetProperty("fechaLogout", out var fechaLogout))
                        resLogout.FechaLogout = fechaLogout.GetDateTime();
                    else if (backendResponse.TryGetProperty("FechaLogout", out var fechaLogoutMayus))
                        resLogout.FechaLogout = fechaLogoutMayus.GetDateTime();

                    if (backendResponse.TryGetProperty("tokensInvalidados", out var tokensInvalidados))
                        resLogout.TokensInvalidados = tokensInvalidados.GetInt32();
                    else if (backendResponse.TryGetProperty("TokensInvalidados", out var tokensInvalidadosMayus))
                        resLogout.TokensInvalidados = tokensInvalidadosMayus.GetInt32();

                    // Si el logout fue exitoso, limpiar datos locales
                    if (resLogout.resultado && resLogout.LogoutExitoso)
                    {
                        Debug.WriteLine("=== LIMPIANDO DATOS LOCALES ===");

                        var userRemoved = SecureStorage.Remove("user_id");
                        var jwtRemoved = SecureStorage.Remove("jwt_token");
                        var refreshRemoved = SecureStorage.Remove("refresh_token");
                        var emailRemoved = SecureStorage.Remove("user_email");
                        var nameRemoved = SecureStorage.Remove("user_name");

                        Debug.WriteLine($"Datos eliminados - User: {userRemoved}, JWT: {jwtRemoved}, Refresh: {refreshRemoved}, Email: {emailRemoved}, Name: {nameRemoved}");
                        Debug.WriteLine("===============================");
                    }

                    Debug.WriteLine($"Resultado final mapeado - resultado: {resLogout.resultado}, logoutExitoso: {resLogout.LogoutExitoso}, mensaje: {resLogout.Mensaje}");
                    return resLogout;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deserializando respuesta exitosa de logout: {ex.Message}");
                    return new ResLogout
                    {
                        resultado = false,
                        LogoutExitoso = false,
                        Mensaje = "Error procesando respuesta del servidor",
                        FechaLogout = DateTime.Now,
                        TokensInvalidados = 0
                    };
                }
            }
            else
            {
                Debug.WriteLine($"Error HTTP en logout: {response.StatusCode}");
                Debug.WriteLine($"Contenido del error: {responseContent}");

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ResLogout>(responseContent, new JsonSerializerOptions
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
                    Debug.WriteLine($"Error deserializando respuesta de error de logout: {ex.Message}");
                }

                return new ResLogout
                {
                    resultado = false,
                    LogoutExitoso = false,
                    Mensaje = $"Error del servidor: {response.StatusCode} - {responseContent}",
                    FechaLogout = DateTime.Now,
                    TokensInvalidados = 0
                };
            }
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"Error HTTP en logout: {httpEx.Message}");
            return new ResLogout
            {
                resultado = false,
                LogoutExitoso = false,
                Mensaje = "Error de conexión. Verifica que el servidor esté corriendo.",
                FechaLogout = DateTime.Now,
                TokensInvalidados = 0
            };
        }
        catch (TaskCanceledException timeoutEx)
        {
            Debug.WriteLine($"Timeout en logout: {timeoutEx.Message}");
            return new ResLogout
            {
                resultado = false,
                LogoutExitoso = false,
                Mensaje = "Tiempo de espera agotado. Intenta nuevamente.",
                FechaLogout = DateTime.Now,
                TokensInvalidados = 0
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error general en logout: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            // En caso de error de comunicación, limpiar datos locales como medida de seguridad
            try
            {
                SecureStorage.Remove("user_id");
                SecureStorage.Remove("jwt_token");
                SecureStorage.Remove("refresh_token");
                SecureStorage.Remove("user_email");
                SecureStorage.Remove("user_name");
                Debug.WriteLine("Datos locales limpiados por error de comunicación");
            }
            catch (Exception cleanupEx)
            {
                Debug.WriteLine($"Error limpiando datos locales: {cleanupEx.Message}");
            }

            return new ResLogout
            {
                resultado = false,
                LogoutExitoso = false,
                Mensaje = "Error de conexión. No se pudo comunicar con el servidor.",
                FechaLogout = DateTime.Now,
                TokensInvalidados = 0
            };
        }
    }

    #endregion

    #region TOKEN

    private async Task ConfigurarTokenAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("jwt_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Debug.WriteLine("Token cargado desde almacenamiento seguro");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cargando token: {ex.Message}");
        }
    }

    private async Task<bool> EnsureTokenAsync()
    {
        var token = await SecureStorage.GetAsync("jwt_token");
        if (string.IsNullOrEmpty(token))
        {
            Debug.WriteLine("No hay token disponible");
            return false;
        }

        if (_httpClient.DefaultRequestHeaders.Authorization == null)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            Debug.WriteLine("Token configurado en HttpClient");
        }

        return true;
    }

    #endregion
}