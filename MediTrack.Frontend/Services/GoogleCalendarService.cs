/*using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System.Text.Json;

namespace MediTrack.Frontend.Services
{
    public class GoogleCalendarService
    {
        private CalendarService? _service;
        private readonly string _credentialsJson;

        public GoogleCalendarService()
        {
            // Tu JSON de credenciales
            _credentialsJson = """
            {
                "web": {
                    "client_id": "915498988803-8dk50nif1ujunc3m7pm4v7cf9imt15bl.apps.googleusercontent.com",
                    "project_id": "planar-maxim-459619-s9",
                    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
                    "token_uri": "https://oauth2.googleapis.com/token",
                    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
                    "client_secret": "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"
                }
            }
            """;
        }

        public async Task<bool> InicializarAsync()
        {
            try
            {
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_credentialsJson));

                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None);

                _service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "MediTrack"
                });

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando Google Calendar: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Models.EventoAgenda>> ObtenerEventosAsync(DateTime fecha)
        {
            if (_service == null) return new List<Models.EventoAgenda>();

            try
            {
                var request = _service.Events.List("primary");
                request.TimeMin = fecha.Date;
                request.TimeMax = fecha.Date.AddDays(1);
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                request.SingleEvents = true;

                var events = await request.ExecuteAsync();

                return events.Items?.Select(e => new Models.EventoAgenda
                {
                    Titulo = e.Summary ?? "Sin título",
                    Descripcion = e.Description ?? "",
                    FechaHora = e.Start.DateTime ?? DateTime.Parse(e.Start.Date),
                    Tipo = "Google Calendar"
                }).ToList() ?? new List<Models.EventoAgenda>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo eventos: {ex.Message}");
                return new List<Models.EventoAgenda>();
            }
        }

        public async Task<bool> CrearEventoAsync(Models.EventoAgenda evento)
        {
            if (_service == null) return false;

            try
            {
                var googleEvent = new Event
                {
                    Summary = evento.Titulo,
                    Description = evento.Descripcion,
                    Start = new EventDateTime
                    {
                        DateTime = evento.FechaHora,
                        TimeZone = "America/Costa_Rica"
                    },
                    End = new EventDateTime
                    {
                        DateTime = evento.FechaHora.Add(evento.Duracion),
                        TimeZone = "America/Costa_Rica"
                    }
                };

                await _service.Events.Insert(googleEvent, "primary").ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando evento: {ex.Message}");
                return false;
            }
        }
    }
}*/ 