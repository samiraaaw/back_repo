using DICREP.EcommerceSubastas.Application.DTOs;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Domain.Entities;
using DICREP.EcommerceSubastas.Infrastructure.Data.Repositories;
using Google;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using Serilog;

namespace DICREP.EcommerceSubastas.Infrastructure.Services
{
    public class GoogleCalendarService
    {
        private readonly GoogleCalendarConfig _config;
        private readonly CalendarService _service;
        private readonly IFeriadosRepository _iferiadosRepository;
        private readonly ILogger _logger;

        public GoogleCalendarService(IFeriadosRepository iferiadosRepository,
            IOptions<GoogleCalendarConfig> config)
        {
            _config = config.Value;
            _iferiadosRepository = iferiadosRepository;
            _logger = Log.ForContext<GoogleCalendarService>();

            if (string.IsNullOrEmpty(_config.ApiKeyGoogleCalendar))
                throw new ArgumentException("API Key de Google Calendar no configurada");

            if (string.IsNullOrEmpty(_config.RegionalCalendarId))
                throw new ArgumentException("Calendar ID no configurado");

            _service = new CalendarService(new BaseClientService.Initializer
            {
                ApiKey = _config.ApiKeyGoogleCalendar,
                ApplicationName = "ApiRemates"
            });
        }


        public class HolidayEvent
        {
            public string Name { get; set; }
            public DateTime Date { get; set; }
        }



        public async Task<List<HolidayEvent>> GetChileHolidaysAsync(int year)
        {
            _logger.Information("Iniciando obtención de feriados de Google Calendar para el año {Year}", year);

            var request = _service.Events.List(_config.RegionalCalendarId);
            request.TimeMin = new DateTime(year, 1, 1);
            request.TimeMax = new DateTime(year, 12, 31);
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            request.Fields = "items(summary,start)";

            try
            {
                var response = await request.ExecuteAsync();
                _logger.Information("Respuesta recibida de Google Calendar. Total de eventos: {Count}", response.Items?.Count);

                var list = response.Items
                    .Where(i => i.Start?.Date != null) // Más específico
                    .Select(i => {
                        try
                        {
                            return new HolidayEvent
                            {
                                Name = i.Summary,
                                Date = DateTime.Parse(i.Start.Date)
                            };
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning("Error parsing date for event '{Summary}': {Date} - {Error}",
                                i.Summary, i.Start.Date, ex.Message);
                            return null;
                        }
                    })
                    .Where(h => h != null) // Filtrar nulos
                    .ToList();


                // Filtrar feriados específicos que no queremos insertar
                var feriadosExcluidos = new List<DateTime>
                {
                    new DateTime(year, 4, 17),  // 17 de abril
                    new DateTime(year, 6, 19),  // 19 de junio
                    new DateTime(year, 12, 31)  // 31 de diciembre
                };

                var filteredList = list
                    .Where(h => !feriadosExcluidos.Contains(h.Date))
                    .ToList();

                _logger.Information("Feriados excluidos: {Count}. Feriados restantes: {Remaining}",
                    list.Count - filteredList.Count, filteredList.Count);


                // 1) Filtrar duplicados - eligiendo el nombre más apropiado
                var distinctEvents = filteredList
                    .GroupBy(h => h.Date)
                    .Select(g => {
                        if (g.Count() == 1)
                            return g.First();

                        // Si hay duplicados, elegir el más apropiado
                        _logger.Warning("Múltiples eventos para la misma fecha {Fecha}: {Cantidad} eventos",
                            g.Key.ToString("yyyy-MM-dd"), g.Count());

                        foreach (var evento in g)
                        {
                            _logger.Information("  - {Nombre}", evento.Name);
                        }

                        // Estrategia: preferir nombres más específicos o evitar ciertos textos
                        var eventos = g.ToList();
                        var eventoElegido = eventos
                            .FirstOrDefault(e => !e.Name.Contains("Observado", StringComparison.OrdinalIgnoreCase))
                            ?? eventos.First();

                        _logger.Information("  Elegido: {Nombre}", eventoElegido.Name);
                        return eventoElegido;
                    })
                    .ToList();


                // 2) Mapear a Feriado
                var feriados = distinctEvents.Select(h => new Feriado
                {
                    Fecha = DateOnly.FromDateTime(h.Date),
                    Descripcion = h.Name,
                    EsRegional = false,
                    Activo = true
                }).ToList();


                // 3) Insertar solo los que no existen
                var feriadosParaInsertar = new List<Feriado>();

                foreach (var f in feriados)
                {
                    if (!await _iferiadosRepository.ExistsByFechaAsync(f.Fecha))
                    {
                        feriadosParaInsertar.Add(f);
                    }
                }

                // Insertar todos los feriados nuevos de una sola vez
                if (feriadosParaInsertar.Any())
                {
                    _logger.Information("Insertando {Count} feriados nuevos en base de datos", feriadosParaInsertar.Count);
                    await _iferiadosRepository.populateHolidaysTableBulk(feriadosParaInsertar);
                }
                _logger.Information("✅ Proceso completado. Año: {Year}, Total eventos: {Total}, Filtrados: {Filtrados}, Insertados: {Insertados}",
                 year, list.Count, filteredList.Count, feriadosParaInsertar.Count);

                return filteredList; // Devolver la lista filtrada 
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.Error(ex, "No se encontró el calendario con ID {CalendarId}", _config.RegionalCalendarId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado al obtener feriados desde Google Calendar para el año {Year}", year);
                throw;
            }
        }
    }

}
