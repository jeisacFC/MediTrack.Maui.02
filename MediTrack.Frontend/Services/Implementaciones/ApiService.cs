using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Services.Interfaces;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
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
            Debug.WriteLine($"JSON Enviado: {jsonRequest}");

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
                Debug.WriteLine($"Errores de API: {response.StatusCode}. Contenido: {errorContent}");
                return null;
            }
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"Errores HTTPS en escanear medicamento: {httpEx.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en ApiService: {ex.Message}");
            return null;
        }
    }

    public async Task<ResBuscarMedicamento> BuscarMedicamentoManualAsync(ReqBuscarMedicamento request)
    {
        var endpoint = "api/medicamentos/buscar"; // Endpoint específico para la búsqueda manual

        try
        {
            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Debug.WriteLine($"Llamando a POST: {_httpClient.BaseAddress}{endpoint}");
            Debug.WriteLine($"JSON Enviado: {jsonRequest}");

            var response = await _httpClient.PostAsync(endpoint, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Respuesta de Búsqueda Manual: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                // Deserializa la respuesta JSON al tipo ResBuscarMedicamento
                return JsonSerializer.Deserialize<ResBuscarMedicamento>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                // Manejo de error si la llamada no fue exitosa
                Debug.WriteLine($"Errores de API en Búsqueda Manual: {response.StatusCode}");
                return new ResBuscarMedicamento { resultado = false, errores = new List<Errores> { new Errores { mensaje = $"Errores del servidor: {response.StatusCode}" } } };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en BuscarMedicamentoManualAsync: {ex.Message}");
            return new ResBuscarMedicamento { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }

    #endregion

    #region AUTENTICACIÓN

    public async Task<ResLogin> LoginAsync(ReqLogin request)
    {
        var endpoint = "api/usuarios/login";

        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var resLogin = new ResLogin();

                // Mapear propiedades básicas
                MapProperty(backendResponse, "resultado", v => resLogin.resultado = v.GetBoolean());
                MapProperty(backendResponse, "mensaje", v => resLogin.Mensaje = v.GetString());

                // Mapear datos del usuario
                MapProperty(backendResponse, "idUsuario", v => resLogin.IdUsuario = v.GetInt32());
                MapProperty(backendResponse, "email", v => resLogin.Email = v.GetString());
                MapProperty(backendResponse, "nombre", v => resLogin.Nombre = v.GetString());
                MapProperty(backendResponse, "apellido1", v => resLogin.Apellido1 = v.GetString());
                MapProperty(backendResponse, "apellido2", v => resLogin.Apellido2 = v.GetString());

                // Mapear tokens
                MapProperty(backendResponse, new[] { "accessToken", "token" }, v => resLogin.Token = v.GetString());
                MapProperty(backendResponse, "refreshToken", v => resLogin.RefreshToken = v.GetString());

                // Guardar datos en SecureStorage si el token existe
                if (!string.IsNullOrEmpty(resLogin.Token))
                {
                    await SaveUserDataAsync(resLogin);
                }

                return resLogin;
            }
            else
            {
                return await HandleErrorResponse<ResLogin>(responseContent, () => new ResLogin
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}"
                });
            }
        }
        catch (HttpRequestException)
        {
            return new ResLogin { resultado = false, Mensaje = "Error de conexión HTTPS" };
        }
        catch (TaskCanceledException)
        {
            return new ResLogin { resultado = false, Mensaje = "Tiempo de espera agotado" };
        }
        catch (Exception)
        {
            return new ResLogin { resultado = false, Mensaje = "Error de conexión general" };
        }
    }

    public async Task<ResRegister> RegisterAsync(ReqRegister request)
    {
        var endpoint = "api/usuarios/registrar";

        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var resRegister = new ResRegister();

                MapProperty(backendResponse, "resultado", v => resRegister.resultado = v.GetBoolean());
                MapProperty(backendResponse, "mensaje", v => resRegister.Mensaje = v.GetString());
                MapProperty(backendResponse, "idUsuario", v => resRegister.IdUsuario = v.GetInt32());
                MapProperty(backendResponse, "fechaRegistro", v => resRegister.FechaRegistro = v.GetDateTime());

                // Mapear errores
                resRegister.errores = MapErrors(backendResponse);

                return resRegister;
            }
            else
            {
                return await HandleErrorResponse<ResRegister>(responseContent, () => new ResRegister
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}",
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                });
            }
        }
        catch (HttpRequestException)
        {
            return CreateErrorRegisterResponse("Error de conexión HTTPS");
        }
        catch (TaskCanceledException)
        {
            return CreateErrorRegisterResponse("Tiempo de espera agotado");
        }
        catch (Exception)
        {
            return CreateErrorRegisterResponse("Error de conexión general");
        }
    }

    public async Task<ResObtenerUsuario> GetUserAsync(ReqObtenerUsuario request)
    {
        var endpoint = "api/usuarios/perfil";

        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var resObtenerUsuario = new ResObtenerUsuario();

                MapProperty(backendResponse, "resultado", v => resObtenerUsuario.resultado = v.GetBoolean());
                MapProperty(backendResponse, "mensaje", v => resObtenerUsuario.Mensaje = v.GetString());

                // Mapear usuario
                if (backendResponse.TryGetProperty("Usuario", out var usuario) && usuario.ValueKind == JsonValueKind.Object)
                {
                    resObtenerUsuario.Usuario = MapUsuario(usuario);
                }

                resObtenerUsuario.errores = MapErrors(backendResponse);

                return resObtenerUsuario;
            }
            else
            {
                string mensajeError = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Usuario no autenticado",
                    HttpStatusCode.NotFound => "Usuario no encontrado",
                    HttpStatusCode.Forbidden => "No tienes permisos para acceder",
                    _ => $"Error del servidor: {response.StatusCode}"
                };

                return await HandleErrorResponse<ResObtenerUsuario>(responseContent, () => new ResObtenerUsuario
                {
                    resultado = false,
                    Mensaje = mensajeError,
                    errores = new List<Errores> { new Errores { mensaje = mensajeError } }
                });
            }
        }
        catch (HttpRequestException)
        {
            return CreateErrorUsuarioResponse("Error de conexión HTTPS");
        }
        catch (TaskCanceledException)
        {
            return CreateErrorUsuarioResponse("Tiempo de espera agotado");
        }
        catch (Exception)
        {
            return CreateErrorUsuarioResponse("Error de conexión general");
        }
    }

    public async Task<ResLogout> LogoutAsync(ReqLogout request)
    {
        var endpoint = "api/usuarios/logout";

        try
        {
            request = await PrepareLogoutRequest(request);

            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var resLogout = new ResLogout();

                MapProperty(backendResponse, "resultado", v => resLogout.resultado = v.GetBoolean());
                MapProperty(backendResponse, "mensaje", v => resLogout.Mensaje = v.GetString());
                MapProperty(backendResponse, "logoutExitoso", v => resLogout.LogoutExitoso = v.GetBoolean());
                MapProperty(backendResponse, "fechaLogout", v => resLogout.FechaLogout = v.GetDateTime());
                MapProperty(backendResponse, "tokensInvalidados", v => resLogout.TokensInvalidados = v.GetInt32());

                if (resLogout.resultado && resLogout.LogoutExitoso)
                {
                    await ClearUserDataAsync();
                }

                return resLogout;
            }
            else
            {
                return await HandleErrorResponse<ResLogout>(responseContent, () => CreateErrorLogoutResponse($"Error del servidor: {response.StatusCode}"));
            }
        }
        catch (Exception)
        {
            // Limpiar datos locales por seguridad en caso de error
            await ClearUserDataAsync();
            return CreateErrorLogoutResponse("Error de conexión");
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
            Debug.WriteLine($"Errores cargando token: {ex.Message}");
        }
    }

    #endregion

    #region AuxAutenticacion
    private void MapProperty(JsonElement element, string propertyName, Action<JsonElement> setValue)
    {
        MapProperty(element, new[] { propertyName }, setValue);
    }

    private void MapProperty(JsonElement element, string[] propertyNames, Action<JsonElement> setValue)
    {
        foreach (var name in propertyNames)
        {
            if (element.TryGetProperty(name, out var value) ||
                element.TryGetProperty(char.ToUpper(name[0]) + name.Substring(1), out value))
            {
                setValue(value);
                break;
            }
        }
    }

    private async Task<T> HandleErrorResponse<T>(string responseContent, Func<T> createDefaultError)
    {
        try
        {
            var errorResponse = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return errorResponse ?? createDefaultError();
        }
        catch
        {
            return createDefaultError();
        }
    }

    private List<Errores> MapErrors(JsonElement backendResponse)
    {
        var erroresList = new List<Errores>();
        if (backendResponse.TryGetProperty("errores", out var errores) && errores.ValueKind == JsonValueKind.Array)
        {
            foreach (var error in errores.EnumerateArray())
            {
                var errorDetail = new Errores();
                MapProperty(error, "mensaje", v => errorDetail.mensaje = v.GetString());
                erroresList.Add(errorDetail);
            }
        }
        return erroresList;
    }

    private Usuarios MapUsuario(JsonElement usuario)
    {
        var usuarioObj = new Usuarios();

        MapProperty(usuario, "id_usuario", v => usuarioObj.id_usuario = v.GetInt32());
        MapProperty(usuario, "email", v => usuarioObj.email = v.GetString());
        MapProperty(usuario, "contraseña", v => usuarioObj.contrasena = v.GetString());
        MapProperty(usuario, "fecha_registro", v => usuarioObj.fecha_registro = v.GetDateTime());
        MapProperty(usuario, "notificaciones_push", v => usuarioObj.notificaciones_push = v.GetBoolean());
        MapProperty(usuario, "ultimo_acceso", v => usuarioObj.ultimo_acceso = v.GetDateTime());
        MapProperty(usuario, "intentos_fallidos", v => usuarioObj.intentos_fallidos = v.GetInt32());
        MapProperty(usuario, "cuenta_bloqueada", v => usuarioObj.cuenta_bloqueada = v.GetBoolean());
        MapProperty(usuario, "nombre", v => usuarioObj.nombre = v.GetString());
        MapProperty(usuario, "apellido1", v => usuarioObj.apellido1 = v.GetString());
        MapProperty(usuario, "apellido2", v => usuarioObj.apellido2 = v.GetString());
        MapProperty(usuario, "fecha_nacimiento", v => usuarioObj.fecha_nacimiento = v.GetDateTime());
        MapProperty(usuario, "id_genero", v => usuarioObj.id_genero = v.GetString());

        return usuarioObj;
    }

    private async Task SaveUserDataAsync(ResLogin resLogin)
    {
        await SecureStorage.SetAsync("jwt_token", resLogin.Token);
        if (!string.IsNullOrEmpty(resLogin.RefreshToken))
            await SecureStorage.SetAsync("refresh_token", resLogin.RefreshToken);
        if (resLogin.IdUsuario > 0)
            await SecureStorage.SetAsync("user_id", resLogin.IdUsuario.ToString());
        if (!string.IsNullOrEmpty(resLogin.Email))
            await SecureStorage.SetAsync("user_email", resLogin.Email);
        if (!string.IsNullOrEmpty(resLogin.Nombre))
            await SecureStorage.SetAsync("user_name", resLogin.Nombre);
    }

    private async Task ClearUserDataAsync()
    {
        SecureStorage.Remove("user_id");
        SecureStorage.Remove("jwt_token");
        SecureStorage.Remove("refresh_token");
        SecureStorage.Remove("user_email");
        SecureStorage.Remove("user_name");
    }

    private async Task<ReqLogout> PrepareLogoutRequest(ReqLogout request)
    {
        request ??= new ReqLogout { InvalidarTodos = false };

        var userIdStr = await SecureStorage.GetAsync("user_id");
        if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var userId))
            request.IdUsuario = userId;

        var accessToken = await SecureStorage.GetAsync("jwt_token");
        if (!string.IsNullOrEmpty(accessToken))
            request.AccessToken = accessToken;

        var refreshToken = await SecureStorage.GetAsync("refresh_token");
        if (!string.IsNullOrEmpty(refreshToken))
            request.RefreshToken = refreshToken;

        return request;
    }

    private ResRegister CreateErrorRegisterResponse(string mensaje)
    {
        return new ResRegister
        {
            resultado = false,
            Mensaje = mensaje,
            errores = new List<Errores> { new Errores { mensaje = mensaje } }
        };
    }

    private ResObtenerUsuario CreateErrorUsuarioResponse(string mensaje)
    {
        return new ResObtenerUsuario
        {
            resultado = false,
            Mensaje = mensaje,
            errores = new List<Errores> { new Errores { mensaje = mensaje } }
        };
    }

    private ResLogout CreateErrorLogoutResponse(string mensaje)
    {
        return new ResLogout
        {
            resultado = false,
            LogoutExitoso = false,
            Mensaje = mensaje,
            FechaLogout = DateTime.Now,
            TokensInvalidados = 0
        };
    }

    #endregion

    // Agregar estos métodos a ApiService.cs

    #region EVENTOS MÉDICOS

    public async Task<ResListarEventosCalendario> ObtenerEventosAsync(ReqObtenerUsuario request)
    {
        var endpoint = "api/evento/listar";

        try
        {
            Debug.WriteLine($"=== OBTENIENDO EVENTOS PARA USUARIO: {request.IdUsuario} ===");

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
            Debug.WriteLine($"Respuesta completa del servidor: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var resEventos = new ResListarEventosCalendario();

                    // Mapear campos básicos
                    if (backendResponse.TryGetProperty("resultado", out var resultado))
                        resEventos.resultado = resultado.GetBoolean();

                    if (backendResponse.TryGetProperty("mensaje", out var mensaje))
                        resEventos.Mensaje = mensaje.GetString();
                    else if (backendResponse.TryGetProperty("Mensaje", out var mensajeMayus))
                        resEventos.Mensaje = mensajeMayus.GetString();

                    // Mapear eventos
                    if (backendResponse.TryGetProperty("eventos", out var eventos) && eventos.ValueKind == JsonValueKind.Array)
                    {
                        var eventosList = new List<ResEventoCalendario>();
                        foreach (var evento in eventos.EnumerateArray())
                        {
                            var eventoObj = new ResEventoCalendario();

                            if (evento.TryGetProperty("tipo", out var tipo))
                                eventoObj.Tipo = tipo.GetString();
                            else if (evento.TryGetProperty("Tipo", out var tipoMayus))
                                eventoObj.Tipo = tipoMayus.GetString();

                            if (evento.TryGetProperty("fechaHora", out var fechaHora))
                                eventoObj.FechaHora = fechaHora.GetDateTime();
                            else if (evento.TryGetProperty("FechaHora", out var fechaHoraMayus))
                                eventoObj.FechaHora = fechaHoraMayus.GetDateTime();

                            if (evento.TryGetProperty("titulo", out var titulo))
                                eventoObj.Titulo = titulo.GetString();
                            else if (evento.TryGetProperty("Titulo", out var tituloMayus))
                                eventoObj.Titulo = tituloMayus.GetString();

                            if (evento.TryGetProperty("descripcion", out var descripcion))
                                eventoObj.Descripcion = descripcion.GetString();
                            else if (evento.TryGetProperty("Descripcion", out var descripcionMayus))
                                eventoObj.Descripcion = descripcionMayus.GetString();

                            eventosList.Add(eventoObj);
                        }
                        resEventos.Eventos = eventosList;
                    }

                    Debug.WriteLine($"Eventos obtenidos: {resEventos.Eventos?.Count ?? 0}");
                    return resEventos;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deserializando respuesta de eventos: {ex.Message}");
                    return new ResListarEventosCalendario
                    {
                        resultado = false,
                        Mensaje = "Error procesando respuesta del servidor",
                        errores = new List<Errores> { new Errores { mensaje = "Error procesando respuesta" } }
                    };
                }
            }
            else
            {
                Debug.WriteLine($"Error HTTP obteniendo eventos: {response.StatusCode}");
                return new ResListarEventosCalendario
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}",
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"Error HTTP en obtener eventos: {httpEx.Message}");
            return new ResListarEventosCalendario
            {
                resultado = false,
                Mensaje = "Error de conexión HTTP",
                errores = new List<Errores> { new Errores { mensaje = "Error de conexión HTTP" } }
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error general obteniendo eventos: {ex.Message}");
            return new ResListarEventosCalendario
            {
                resultado = false,
                Mensaje = "Error de conexión",
                errores = new List<Errores> { new Errores { mensaje = "Error de conexión general" } }
            };
        }
    }

    public async Task<ResInsertarEventoMedico> InsertarEventoAsync(ReqInsertarEventoMedico request)
    {
        var endpoint = "api/evento/insertar";

        try
        {
            Debug.WriteLine("=== INSERTANDO NUEVO EVENTO ===");
            Debug.WriteLine($"IdUsuario: {request.IdUsuario}");
            Debug.WriteLine($"Titulo: {request.Titulo}");
            Debug.WriteLine($"FechaEvento: {request.FechaEvento:yyyy-MM-dd HH:mm}");

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
            Debug.WriteLine($"Respuesta: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var resEvento = JsonSerializer.Deserialize<ResInsertarEventoMedico>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Debug.WriteLine($"Evento insertado - resultado: {resEvento.resultado}");
                    return resEvento;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deserializando respuesta de insertar evento: {ex.Message}");
                    return new ResInsertarEventoMedico
                    {
                        resultado = false,
                        Mensaje = "Error procesando respuesta del servidor"
                    };
                }
            }
            else
            {
                Debug.WriteLine($"Error HTTP insertando evento: {response.StatusCode}");
                return new ResInsertarEventoMedico
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error general insertando evento: {ex.Message}");
            return new ResInsertarEventoMedico
            {
                resultado = false,
                Mensaje = "Error de conexión"
            };
        }
    }

    public async Task<ResActualizarEventoMedico> ActualizarEventoAsync(ReqActualizarEventoMedico request)
    {
        var endpoint = "api/evento/actualizar";

        try
        {
            Debug.WriteLine("=== ACTUALIZANDO EVENTO ===");
            Debug.WriteLine($"IdEvento: {request.IdEvento}");
            Debug.WriteLine($"Titulo: {request.Titulo}");

            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"Status Code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var resEvento = JsonSerializer.Deserialize<ResActualizarEventoMedico>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return resEvento;
            }
            else
            {
                return new ResActualizarEventoMedico
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error actualizando evento: {ex.Message}");
            return new ResActualizarEventoMedico
            {
                resultado = false,
                Mensaje = "Error de conexión"
            };
        }
    }

    public async Task<ResCompletarEvento> CompletarEventoAsync(ReqEvento request)
    {
        var endpoint = "api/evento/completar";

        try
        {
            Debug.WriteLine("=== MARCANDO EVENTO COMO COMPLETADO ===");
            Debug.WriteLine($"IdEvento: {request.IdEvento}");

            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"Status Code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var resEvento = JsonSerializer.Deserialize<ResCompletarEvento>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return resEvento;
            }
            else
            {
                return new ResCompletarEvento
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error completando evento: {ex.Message}");
            return new ResCompletarEvento
            {
                resultado = false,
                Mensaje = "Error de conexión"
            };
        }
    }

    public async Task<ResEliminarEventoUsuario> EliminarEventoAsync(ReqEvento request)
    {
        var endpoint = "api/evento/eliminar";

        try
        {
            Debug.WriteLine("=== ELIMINANDO EVENTO ===");
            Debug.WriteLine($"IdEvento: {request.IdEvento}");

            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"Status Code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var resEvento = JsonSerializer.Deserialize<ResEliminarEventoUsuario>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return resEvento;
            }
            else
            {
                return new ResEliminarEventoUsuario
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error eliminando evento: {ex.Message}");
            return new ResEliminarEventoUsuario
            {
                resultado = false,
                Mensaje = "Error de conexión"
            };
        }
    }

    #endregion
}