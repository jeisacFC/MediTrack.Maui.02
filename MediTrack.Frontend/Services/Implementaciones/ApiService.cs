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

    // Recibe un IHttpClientFactory para crear un cliente HttpClient de forma segura y eficiente.
    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient"); // Pide el cliente que configuraremos

        // Determina la URL base correcta dependiendo de la plataforma
        string baseUrl = DeviceInfo.Platform == DevicePlatform.Android ? "https://10.0.2.2:44382" : "https://localhost:44382";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    #region MEDICAMENTOS

    // Implementación del método definido en la interfaz.
    public async Task<ResEscanearMedicamento> EscanearMedicamentoAsync(ReqEscanearMedicamento request)
    {
        // La ruta específica del endpoint, que se añade a la URL base del HttpClient.
        var endpoint = "api/medicamentos/escanear";

        try
        {
            // Convierte el objeto C# de la solicitud a un string en formato JSON.
            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Debug.WriteLine($"Llamando a POST: {_httpClient.BaseAddress}{endpoint}");

            // Realiza la llamada POST al backend y espera la respuesta.
            var response = await _httpClient.PostAsync(endpoint, content);

            // Lee la respuesta como un stream para deserializar.
            var responseStream = await response.Content.ReadAsStreamAsync();

            if (response.IsSuccessStatusCode)
            {
                // Si la llamada fue exitosa (código 200 OK), convierte la respuesta JSON a un objeto ResEscanearMedicamento.
                return await JsonSerializer.DeserializeAsync<ResEscanearMedicamento>(responseStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Permite que "nombreComercial" en JSON mapee a "NombreComercial" en C#.
                });
            }
            else
            {
                // Si el backend devolvió un error (ej. 404 No Encontrado, 500 Error de Servidor), se registra y devuelve null.
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Error de API: {response.StatusCode}. Contenido: {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            // Captura errores de conexión (ej. el backend no está corriendo).
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
            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Debug.WriteLine($"Llamando a POST: {_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"Respuesta del servidor: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                // El backend devuelve un objeto con AccessToken, TokenExpiration, TokenType
                // Necesitamos mapear a Token, Expiracion, TipoToken
                var backendResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                var resLogin = new ResLogin();

                // Mapear campos básicos
                if (backendResponse.TryGetProperty("resultado", out var resultado))
                    resLogin.resultado = resultado.GetBoolean();

                if (backendResponse.TryGetProperty("Mensaje", out var mensaje))
                    resLogin.Mensaje = mensaje.GetString();

                // Mapear datos del usuario
                if (backendResponse.TryGetProperty("IdUsuario", out var idUsuario))
                    resLogin.IdUsuario = idUsuario.GetInt32();

                if (backendResponse.TryGetProperty("Email", out var email))
                    resLogin.Email = email.GetString();

                if (backendResponse.TryGetProperty("Nombre", out var nombre))
                    resLogin.Nombre = nombre.GetString();

                if (backendResponse.TryGetProperty("Apellido1", out var apellido1))
                    resLogin.Apellido1 = apellido1.GetString();

                if (backendResponse.TryGetProperty("Apellido2", out var apellido2))
                    resLogin.Apellido2 = apellido2.GetString();

                if (backendResponse.TryGetProperty("UltimoAcceso", out var ultimoAcceso))
                    resLogin.UltimoAcceso = ultimoAcceso.GetDateTime();

                // Mapear tokens - CLAVE: mapear desde nombres del backend a nombres del frontend
                if (backendResponse.TryGetProperty("AccessToken", out var accessToken))
                    resLogin.Token = accessToken.GetString();

                if (backendResponse.TryGetProperty("RefreshToken", out var refreshToken))
                    resLogin.RefreshToken = refreshToken.GetString();

                if (backendResponse.TryGetProperty("TokenExpiration", out var tokenExpiration))
                    resLogin.Expiracion = tokenExpiration.GetDateTime();

                if (backendResponse.TryGetProperty("TokenType", out var tokenType))
                    resLogin.TipoToken = tokenType.GetString();

                return resLogin;
            }
            else
            {
                Debug.WriteLine($"Error de API en login: {response.StatusCode}. Contenido: {responseContent}");

                // Para errores, intentar deserializar directamente
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ResLogin>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return errorResponse;
                }
                catch
                {
                    return new ResLogin
                    {
                        resultado = false,
                        Mensaje = $"Error del servidor: {response.StatusCode}"
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error de conexión en login: {ex.Message}");
            return new ResLogin
            {
                resultado = false,
                Mensaje = "Error de conexión. No se pudo comunicar con el servidor."
            };
        }
    }

    #endregion
}