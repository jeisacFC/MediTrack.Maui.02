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


 
}