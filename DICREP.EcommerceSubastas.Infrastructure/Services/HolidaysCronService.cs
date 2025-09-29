using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
namespace DICREP.EcommerceSubastas.Infrastructure.Services
{
    public class HolidaysCronService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CronExpression _expression;
        private readonly TimeZoneInfo _timeZone;
        private readonly ILogger _logger;


        /*

        // Despues ocupar este
        // En el constructor, mejor usar IConfiguration:

        private readonly CronExpression _expression;

        public HolidaysCronService(IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _logger = Log.ForContext<HolidaysCronService>();
    
            // Leer de configuración en lugar de hardcodear
            var cronExpression = config["Cron:HolidaysSchedule"] ?? "0 0 0 1 1 *";
            _expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
            _timeZone = TimeZoneInfo.Local;
    
            _logger.Information("Cron configurado: {CronExpression}", cronExpression);
        }

        */

        public HolidaysCronService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _logger = Log.ForContext<HolidaysCronService>();

            //segundo, minuto, hora, dia del mes, mes, dia de la semana
            //_expression = CronExpression.Parse("0 0 0 1 1 *", CronFormat.IncludeSeconds); //todos los 1 de Enero a las 00:00 hrs
            _expression = CronExpression.Parse("0 52 17 27 8 *", CronFormat.IncludeSeconds); //todos los 1 de Enero a las 00:00 hrs
            _timeZone = TimeZoneInfo.Local;
        }



        public static async Task LongDelayAsync(TimeSpan delay, CancellationToken ct)
        {
            const long MaxMs = int.MaxValue; // ~ 24.8 días, seguro dentro de límite
            long remaining = (long)delay.TotalMilliseconds;

            while (remaining > 0)
            {
                int current = remaining > MaxMs ? int.MaxValue : (int)remaining;
                await Task.Delay(current, ct);
                remaining -= current;
            }
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("HolidaysCronService iniciado. Esperando próxima ejecución según cron.");
            var gracePeriod = TimeSpan.FromDays(1); // margen para "catch-up"
            var now = DateTimeOffset.Now;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.Information("🚀 HolidaysCronService iniciado. Programación: {CronExpression}", "0 52 17 27 8 *");
                    var next = _expression.GetNextOccurrence(now, _timeZone);

                    // 1. Catch-up
                    if (next.HasValue &&
                        next.Value <= DateTimeOffset.Now &&
                        next.Value >= DateTimeOffset.Now.Subtract(gracePeriod))
                    {
                        _logger.Warning("⚠️ Ejecución atrasada detectada. Se saltó la programación normal. Ejecutando feriados para {Year}", next.Value.Year);

                        using var scope = _scopeFactory.CreateScope();
                        var holidayService = scope.ServiceProvider.GetRequiredService<GoogleCalendarService>();
                        await holidayService.GetChileHolidaysAsync(next.Value.Year);
                        now = DateTimeOffset.Now;
                        continue;
                    }

                    // 2. Espera hasta la próxima ejecución
                    if (next.HasValue && next.Value > DateTimeOffset.Now)
                    {
                        var delay = next.Value - DateTimeOffset.Now;
                        _logger.Information("Próxima ejecución programada para: {NextExecution} (espera de {Delay})", next.Value, delay);

                        await LongDelayAsync(delay, stoppingToken);

                        _logger.Information("Ejecutando actualización de feriados para el año {Year}", next.Value.Year);
                        using var scope = _scopeFactory.CreateScope();
                        var holidayService = scope.ServiceProvider.GetRequiredService<GoogleCalendarService>();
                        await holidayService.GetChileHolidaysAsync(next.Value.Year);

                        // Después de ejecutar el servicio:
                        await holidayService.GetChileHolidaysAsync(next.Value.Year);
                        _logger.Information("✅ Actualización de feriados para {Year} completada exitosamente", next.Value.Year);

                        now = next.Value;
                    }
                    else
                    {
                        _logger.Warning("No se encontró una próxima ejecución válida. Esperando 1 hora.");
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        now = DateTimeOffset.Now;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error inesperado en HolidaysCronService. Reintentando en 5 minutos.");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }



    }
}
