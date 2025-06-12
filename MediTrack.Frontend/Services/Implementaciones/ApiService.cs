using MediTrack.Frontend.Models;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Services.Interfaces;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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

    public async Task<ResGuardarMedicamento> GuardarMedicamentoAsync(ReqGuardarMedicamento request)
    {
        var endpoint = "api/medicamentos/guardar";

        try
        {
            // Serializamos el objeto de solicitud a JSON
            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Debug.WriteLine($"Llamando a POST: {_httpClient.BaseAddress}{endpoint}");
            Debug.WriteLine($"JSON Enviado: {jsonRequest}");

            // Realizamos la llamada POST
            var response = await _httpClient.PostAsync(endpoint, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Respuesta de Guardar Medicamento: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                // Deserializamos la respuesta del backend
                return JsonSerializer.Deserialize<ResGuardarMedicamento>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                // Si hay un error de servidor (ej. 400, 500), lo manejamos
                return new ResGuardarMedicamento
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error de conexión en GuardarMedicamentoAsync: {ex.Message}");
            return new ResGuardarMedicamento
            {
                resultado = false,
                Mensaje = "No se pudo conectar con el servidor."
            };
        }
    }


    public async Task<ResListarMedicamentosUsuario> ListarMedicamentosUsuarioAsync(ReqObtenerUsuario request)
    {
        var endpoint = "api/medicamentos/listar/usuario";
        try
        {
            // Usamos PostAsJsonAsync para simplificar la serialización
            var response = await _httpClient.PostAsJsonAsync(endpoint, request);

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Respuesta de {endpoint}: {json}");

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<ResListarMedicamentosUsuario>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return new ResListarMedicamentosUsuario { resultado = false, Mensaje = $"Error del servidor: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error de conexión en ListarMedicamentosUsuarioAsync: {ex.Message}");
            return new ResListarMedicamentosUsuario { resultado = false, Mensaje = "No se pudo conectar con el servidor." };
        }
    }

    public async Task<ResDetalleMedicamentoUsuario> ObtenerDetalleMedicamentoUsuarioAsync(ReqMedicamento request)
    {
        var endpoint = "api/medicamentos/detalle";
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, request);

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Respuesta de {endpoint}: {json}");

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<ResDetalleMedicamentoUsuario>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return new ResDetalleMedicamentoUsuario { resultado = false, Mensaje = $"Error del servidor: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error de conexión en ObtenerDetalleMedicamentoUsuarioAsync: {ex.Message}");
            return new ResDetalleMedicamentoUsuario { resultado = false, Mensaje = "No se pudo conectar con el servidor." };
        }
    }

    public async Task<ResEliminarMedicamentoUsuario> EliminarMedicamentoUsuarioAsync(ReqMedicamento request)
    {
        var endpoint = "api/medicamentos/eliminar";
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, request);

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Respuesta de {endpoint}: {json}");

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<ResEliminarMedicamentoUsuario>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return new ResEliminarMedicamentoUsuario { resultado = false, Mensaje = $"Error del servidor: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error de conexión en EliminarMedicamentoUsuarioAsync: {ex.Message}");
            return new ResEliminarMedicamentoUsuario { resultado = false, Mensaje = "No se pudo conectar con el servidor." };
        }
    }

    #endregion

    #region IA
    public async Task<ResHabitosSaludables?> ObtenerHabitosAsync(ReqObtenerUsuario request)
    {
        const string endpoint = "api/ia/habitos";
        try
        {
            var json = JsonSerializer.Serialize(request);
            Debug.WriteLine($"[ApiService] Request JSON: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[ApiService] Llamando a POST: {_httpClient.BaseAddress}{endpoint}");
            var response = await _httpClient.PostAsync(endpoint, content);

            var payload = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[ApiService] HTTP {(int)response.StatusCode} – Payload: {payload}");

            if (response.IsSuccessStatusCode)
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
                var res = await JsonSerializer.DeserializeAsync<ResHabitosSaludables>(
                    stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Debug.WriteLine($"[ApiService] Deserializados {res?.Habitos?.Count ?? 0} hábitos");
                return res;
            }
            else
            {
                Debug.WriteLine($"[ApiService] Error servidor: {(int)response.StatusCode}");
                return new ResHabitosSaludables
                {
                    resultado = false,
                    Habitos = new List<string>(),
                    Mensaje = $"Error {(int)response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService] Excepción: {ex}");
            throw;
        }
    }

    public async Task<ResRecomendacionesIA?> ObtenerRecomendacionesAsync(ReqObtenerUsuario request)
    {
        const string endpoint = "api/ia/recomendaciones";
        try
        {
            var json = JsonSerializer.Serialize(request);
            Debug.WriteLine($"[ApiService] Req Recomendaciones JSON: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[ApiService] Llamando a POST: {_httpClient.BaseAddress}{endpoint}");
            var response = await _httpClient.PostAsync(endpoint, content);

            var payload = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[ApiService] HTTP {(int)response.StatusCode} – Payload: {payload}");

            if (response.IsSuccessStatusCode)
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
                var res = await JsonSerializer.DeserializeAsync<ResRecomendacionesIA>(
                    stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                Debug.WriteLine($"[ApiService] Deserializadas {res?.Recomendaciones?.Count ?? 0} recomendaciones");
                return res;
            }
            else
            {
                Debug.WriteLine($"[ApiService] Error servidor: {(int)response.StatusCode}");
                return new ResRecomendacionesIA
                {
                    resultado = false,
                    Recomendaciones = new List<string>(),
                    Mensaje = $"Error {(int)response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService] Excepción recomendaciones: {ex}");
            throw;
        }
    }

    public async Task<ResInteraccionesMedicamentos?> ObtenerInteraccionesAsync(ReqObtenerUsuario request)
    {
        const string endpoint = "api/ia/interacciones";
        try
        {
            var json = JsonSerializer.Serialize(request);
            Debug.WriteLine($"[ApiService] Req Interacciones JSON: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[ApiService] POST {_httpClient.BaseAddress}{endpoint}");
            var response = await _httpClient.PostAsync(endpoint, content);

            var payload = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[ApiService] HTTP {(int)response.StatusCode} – Payload: {payload}");

            if (response.IsSuccessStatusCode)
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
                var res = await JsonSerializer.DeserializeAsync<ResInteraccionesMedicamentos>(
                    stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                Debug.WriteLine($"[ApiService] Deserializadas {res?.Interacciones?.Count ?? 0} interacciones");
                return res;
            }
            else
            {
                Debug.WriteLine($"[ApiService] Error servidor: {(int)response.StatusCode}");
                return new ResInteraccionesMedicamentos
                {
                    resultado = false,
                    Interacciones = new List<string>(),
                    Mensaje = $"Error {(int)response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService] Excepción interacciones: {ex}");
            throw;
        }
    }

    public async Task<ResAlertaSalud?> ObtenerAlertasSaludAsync(ReqObtenerUsuario request)
    {
        const string endpoint = "api/ia/alertas";
        try
        {
            var json = JsonSerializer.Serialize(request);
            Debug.WriteLine($"[ApiService] Req Alertas JSON: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[ApiService] POST {_httpClient.BaseAddress}{endpoint}");
            var response = await _httpClient.PostAsync(endpoint, content);

            var payload = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[ApiService] HTTP {(int)response.StatusCode} – Payload: {payload}");

            if (response.IsSuccessStatusCode)
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
                var res = await JsonSerializer.DeserializeAsync<ResAlertaSalud>(
                    stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                Debug.WriteLine($"[ApiService] Deserializadas {res?.Alertas?.Count ?? 0} alertas");
                return res;
            }
            else
            {
                Debug.WriteLine($"[ApiService] Error servidor alertas: {(int)response.StatusCode}");
                return new ResAlertaSalud
                {
                    resultado = false,
                    Alertas = new List<AlertaSalud>(),
                    Mensaje = $"Error {(int)response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService] Excepción alertas: {ex}");
            throw;
        }
    }

    #endregion

    #region AUTENTICACIÓN USUARIOS
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
    public async Task<ResActualizarUsuario> ActualizarUsuarioAsync(ReqActualizarUsuario request)
    {
        var endpoint = "api/usuarios/perfil";
        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var resActualizarUsuario = new ResActualizarUsuario();
                MapProperty(backendResponse, "resultado", v => resActualizarUsuario.resultado = v.GetBoolean());
                MapProperty(backendResponse, "mensaje", v => resActualizarUsuario.Mensaje = v.GetString());
                // Mapear usuario actualizado
                if (backendResponse.TryGetProperty("usuarioActualizado", out var usuarioActualizado) && usuarioActualizado.ValueKind == JsonValueKind.Object)
                {
                    resActualizarUsuario.FilasAfectadas = 1;
                }
                resActualizarUsuario.errores = MapErrors(backendResponse);
                return resActualizarUsuario;
            }
            else
            {
                return await HandleErrorResponse<ResActualizarUsuario>(responseContent, () => new ResActualizarUsuario
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}",
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en ActualizarUsuarioAsync: {ex.Message}");
            return new ResActualizarUsuario { resultado = false, Mensaje = "No se pudo conectar con el servidor.", errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }
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

    #region CondicionesMedicas y Alergias
    public async Task<ResInsertarCondicion> InsertarCondicionAsync(ReqInsertarCondicion request)
    {
        var endpoint = "api/condiciones-medicas/crear";
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
                return JsonSerializer.Deserialize<ResInsertarCondicion>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                return new ResInsertarCondicion
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en InsertarCondicionAsync: {ex.Message}");
            return new ResInsertarCondicion { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }
    public async Task<ResObtenerCondicionesUsuario> ObtenerCondicionesMedicasAsync(ReqObtenerCondicionesUsuario request)
    {
        var endpoint = "api/condiciones-medicas/obtener-usuario";
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
                return JsonSerializer.Deserialize<ResObtenerCondicionesUsuario>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                return new ResObtenerCondicionesUsuario
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en ObtenerCondicionesMedicasAsync: {ex.Message}");
            return new ResObtenerCondicionesUsuario { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }
    public async Task<ResListarCondiciones> ListarCondicionesMedicasAsync(ReqListarCondiciones request)
    {
        var endpoint = "api/condiciones-medicas/listar";
        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.GetAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<ResListarCondiciones>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                return new ResListarCondiciones
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en ListarCondicionesMedicasAsync: {ex.Message}");
            return new ResListarCondiciones { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }
    public async Task<ResAsignarCondicionUsuario> AsignarCondicionUsuarioAsync(ReqAsignarCondicionUsuario request)
    {
        var endpoint = "api/condiciones-medicas/asignar-usuario";
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
                return JsonSerializer.Deserialize<ResAsignarCondicionUsuario>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                return new ResAsignarCondicionUsuario
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en AsignarCondicionUsuarioAsync: {ex.Message}");
            return new ResAsignarCondicionUsuario { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }
    public async Task<ResDesasignarCondicionUsuario> DesasignarCondicionUsuarioAsync(ReqDesasignarCondicionUsuario request)
    {
        var endpoint = "api/condiciones-medicas/desasignar-usuario";
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
                return JsonSerializer.Deserialize<ResDesasignarCondicionUsuario>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                return new ResDesasignarCondicionUsuario
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en DesasignarCondicionUsuarioAsync: {ex.Message}");
            return new ResDesasignarCondicionUsuario { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }
    public async Task<ResInsertarAlergia> InsertarAlergiaAsync(ReqInsertarAlergia request)
    {
        var endpoint = "api/alergias/crear";
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
                return JsonSerializer.Deserialize<ResInsertarAlergia>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                return new ResInsertarAlergia
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en InsertarAlergiaAsync: {ex.Message}");
            return new ResInsertarAlergia { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }
    public async Task<ResObtenerAlergiasUsuario> ObtenerAlergiasUsuarioAsync(ReqObtenerAlergiasUsuario request)
    {
        var endpoint = "api/alergias/obtener-usuario";
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
                return JsonSerializer.Deserialize<ResObtenerAlergiasUsuario>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                return new ResObtenerAlergiasUsuario
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en ObtenerAlergiasUsuarioAsync: {ex.Message}");
            return new ResObtenerAlergiasUsuario { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }
    public async Task<ResListarAlergias> ListarAlergiasAsync(ReqListarAlergias request)
    {
        var endpoint = "api/alergias/listar";
        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.GetAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<ResListarAlergias>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                return new ResListarAlergias
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en ListarAlergiasAsync: {ex.Message}");
            return new ResListarAlergias { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }
    public async Task<ResAsignarAlergiaUsuario> AsignarAlergiaUsuarioAsync(ReqAsignarAlergiaUsuario request)
    {
        var endpoint = "api/alergias/asignar-usuario";
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
                return JsonSerializer.Deserialize<ResAsignarAlergiaUsuario>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                return new ResAsignarAlergiaUsuario
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en AsignarAlergiaUsuarioAsync: {ex.Message}");
            return new ResAsignarAlergiaUsuario { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
        }
    }
    public async Task<ResDesasignarAlergiaUsuario> DesasignarAlergiaUsuarioAsync(ReqDesasignarAlergiaUsuario request)
    {
        var endpoint = "api/alergias/desasignar-usuario";
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
                return JsonSerializer.Deserialize<ResDesasignarAlergiaUsuario>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                return new ResDesasignarAlergiaUsuario
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Errores de conexión en DesasignarAlergiaUsuarioAsync: {ex.Message}");
            return new ResDesasignarAlergiaUsuario { resultado = false, errores = new List<Errores> { new Errores { mensaje = "No se pudo conectar con el servidor." } } };
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

    #region RECUPERAR CONTRASEÑA

    public async Task<ResSolicitarResetPassword> SolicitarResetPasswordAsync(ReqSolicitarResetPassword request)
    {
        var endpoint = "api/usuarios/solicitar-reset-password";

        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"SolicitarResetPassword Response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var resSolicitar = new ResSolicitarResetPassword();

                MapProperty(backendResponse, "resultado", v => resSolicitar.resultado = v.GetBoolean());
                MapProperty(backendResponse, "mensaje", v => resSolicitar.Mensaje = v.GetString());
                MapProperty(backendResponse, "emailEnviado", v => resSolicitar.EmailEnviado = v.GetBoolean());
                MapProperty(backendResponse, "fechaEnvio", v => resSolicitar.FechaEnvio = v.GetDateTime());

                resSolicitar.errores = MapErrors(backendResponse);

                return resSolicitar;
            }
            else
            {
                return await HandleErrorResponse<ResSolicitarResetPassword>(responseContent, () => new ResSolicitarResetPassword
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}",
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                });
            }
        }
        catch (HttpRequestException)
        {
            return CreateErrorSolicitarResetResponse("Error de conexión HTTPS");
        }
        catch (TaskCanceledException)
        {
            return CreateErrorSolicitarResetResponse("Tiempo de espera agotado");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Excepción en SolicitarResetPassword: {ex.Message}");
            return CreateErrorSolicitarResetResponse("Error de conexión general");
        }
    }

    public async Task<ResRestablecerContrasena> RestablecerContrasenaAsync(ReqRestablecerContrasena request)
    {
        var endpoint = "api/usuarios/restablecer-contrasena";

        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"RestablecerContrasena Response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var resRestablecer = new ResRestablecerContrasena();

                MapProperty(backendResponse, "resultado", v => resRestablecer.resultado = v.GetBoolean());
                MapProperty(backendResponse, "mensaje", v => resRestablecer.Mensaje = v.GetString());
                MapProperty(backendResponse, "fechaActualizacion", v => resRestablecer.FechaActualizacion = v.GetDateTime());

                resRestablecer.errores = MapErrors(backendResponse);

                return resRestablecer;
            }
            else
            {
                return await HandleErrorResponse<ResRestablecerContrasena>(responseContent, () => new ResRestablecerContrasena
                {
                    resultado = false,
                    Mensaje = $"Error del servidor: {response.StatusCode}",
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                });
            }
        }
        catch (HttpRequestException)
        {
            return CreateErrorRestablecerResponse("Error de conexión HTTPS");
        }
        catch (TaskCanceledException)
        {
            return CreateErrorRestablecerResponse("Tiempo de espera agotado");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Excepción en RestablecerContrasena: {ex.Message}");
            return CreateErrorRestablecerResponse("Error de conexión general");
        }
    }

    private ResSolicitarResetPassword CreateErrorSolicitarResetResponse(string mensaje)
    {
        return new ResSolicitarResetPassword
        {
            resultado = false,
            Mensaje = mensaje,
            EmailEnviado = false,
            errores = new List<Errores> { new Errores { mensaje = mensaje } }
        };
    }

    private ResRestablecerContrasena CreateErrorRestablecerResponse(string mensaje)
    {
        return new ResRestablecerContrasena
        {
            resultado = false,
            Mensaje = mensaje,
            errores = new List<Errores> { new Errores { mensaje = mensaje } }
        };
    }

    #endregion

    #region SÍNTOMAS

    public async Task<List<ResBuscarSintoma>> BuscarSintomasAsync(ReqBuscarSintoma request)
    {
        var endpoint = "api/sintomas/buscar";
        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[ApiService] Buscando síntomas: {_httpClient.BaseAddress}{endpoint}");
            Debug.WriteLine($"[ApiService] Request: {jsonRequest}");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[ApiService] Response síntomas: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<List<ResBuscarSintoma>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ResBuscarSintoma>();
            }
            else
            {
                Debug.WriteLine($"[ApiService] Error buscando síntomas: {response.StatusCode}");
                return new List<ResBuscarSintoma>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService] Excepción buscando síntomas: {ex.Message}");
            return new List<ResBuscarSintoma>();
        }
    }

    public async Task<ResObtenerSintomasEnum> ObtenerTodosLosSintomasAsync()
    {
        var endpoint = "api/sintomas/ListaSintomasEnum";
        try
        {
            Debug.WriteLine($"[ApiService TODOS] URL: {_httpClient.BaseAddress}{endpoint}");
            Debug.WriteLine($"[ApiService TODOS] Request: Sin parámetros (solo JWT)");

            // Enviar POST sin contenido pero con JWT automático
            var content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[ApiService TODOS] Status: {response.StatusCode}");
            Debug.WriteLine($"[ApiService TODOS] Response Length: {responseContent.Length}");
            Debug.WriteLine($"[ApiService TODOS] Response Preview: {responseContent.Substring(0, Math.Min(300, responseContent.Length))}...");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ResObtenerSintomasEnum>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new ResObtenerSintomasEnum { resultado = false };

                Debug.WriteLine($"[ApiService TODOS] ✅ Síntomas deserializados: {result.Sintomas?.Count ?? 0}");

                if (result.Sintomas?.Any() == true)
                {
                    foreach (var sintoma in result.Sintomas.Take(3))
                    {
                        Debug.WriteLine($"[ApiService TODOS] - ID: {sintoma.IdSintoma}, Nombre: {sintoma.Nombre}");
                    }
                    if (result.Sintomas.Count > 3)
                    {
                        Debug.WriteLine($"[ApiService TODOS] ... y {result.Sintomas.Count - 3} más");
                    }
                }

                return result;
            }
            else
            {
                Debug.WriteLine($"[ApiService TODOS] ❌ Error: {response.StatusCode}");
                return new ResObtenerSintomasEnum
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService TODOS] ❌ Excepción: {ex.Message}");
            return new ResObtenerSintomasEnum
            {
                resultado = false,
                errores = new List<Errores> { new Errores { mensaje = "Error de conexión" } }
            };
        }
    }

    public async Task<ResInsertarSintomaManual> InsertarSintomaManualAsync(ReqInsertarSintomaManual request)
    {
        var endpoint = "api/sintomas/insertarSintomaManual";
        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[ApiService] Insertando síntoma manual: {_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<ResInsertarSintomaManual>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new ResInsertarSintomaManual { resultado = false };
            }
            else
            {
                return new ResInsertarSintomaManual
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService] Error insertando síntoma manual: {ex.Message}");
            return new ResInsertarSintomaManual
            {
                resultado = false,
                errores = new List<Errores> { new Errores { mensaje = "Error de conexión" } }
            };
        }
    }

    public async Task<ResAgregarSintomasSeleccionados> AgregarSintomasSeleccionadosAsync(ReqAgregarSintomasSeleccionados request)
    {
        var endpoint = "api/sintomas/agregarSintomasSeleccionados";
        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[ApiService] Agregando síntomas seleccionados: {_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<ResAgregarSintomasSeleccionados>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new ResAgregarSintomasSeleccionados { resultado = false };
            }
            else
            {
                return new ResAgregarSintomasSeleccionados
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService] Error agregando síntomas seleccionados: {ex.Message}");
            return new ResAgregarSintomasSeleccionados
            {
                resultado = false,
                errores = new List<Errores> { new Errores { mensaje = "Error de conexión" } }
            };
        }
    }

    public async Task<ResObtenerSintomasUsuario> ObtenerSintomasUsuarioAsync(ReqObtenerUsuario request)
    {
        var endpoint = "api/sintomas/ListaUsuario";
        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[ApiService] Obteniendo síntomas usuario: {_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<ResObtenerSintomasUsuario>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new ResObtenerSintomasUsuario { resultado = false };
            }
            else
            {
                return new ResObtenerSintomasUsuario
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService] Error obteniendo síntomas usuario: {ex.Message}");
            return new ResObtenerSintomasUsuario
            {
                resultado = false,
                errores = new List<Errores> { new Errores { mensaje = "Error de conexión" } }
            };
        }
    }

    public async Task<ResEliminarSintoma> EliminarSintomaUsuarioAsync(ReqEliminarSintoma request)
    {
        var endpoint = "api/sintomas/eliminar";
        try
        {
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[ApiService] Eliminando síntoma usuario: {_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<ResEliminarSintoma>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new ResEliminarSintoma { resultado = false };
            }
            else
            {
                return new ResEliminarSintoma
                {
                    resultado = false,
                    errores = new List<Errores> { new Errores { mensaje = $"Error del servidor: {response.StatusCode}" } }
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService] Error eliminando síntoma: {ex.Message}");
            return new ResEliminarSintoma
            {
                resultado = false,
                errores = new List<Errores> { new Errores { mensaje = "Error de conexión" } }
            };
        }
    }

    #endregion
}