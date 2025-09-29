using DICREP.EcommerceSubastas.API.Filters;
using DICREP.EcommerceSubastas.API.Middlewares;
using DICREP.EcommerceSubastas.API.Security;
using DICREP.EcommerceSubastas.Application.DTOs;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Application.Mappers;
using DICREP.EcommerceSubastas.Application.UseCases.Acceso;
using DICREP.EcommerceSubastas.Application.UseCases.CLPrenda;
using DICREP.EcommerceSubastas.Application.UseCases.CuentaBancaria;
using DICREP.EcommerceSubastas.Application.UseCases.FichaProducto;
using DICREP.EcommerceSubastas.Application.UseCases.Subasta;
using DICREP.EcommerceSubastas.Infrastructure.Authorization;
using DICREP.EcommerceSubastas.Infrastructure.Configurations;
using DICREP.EcommerceSubastas.Infrastructure.Data.Repositories;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using DICREP.EcommerceSubastas.Infrastructure.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using File = System.IO.File;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.Sources.Clear();

/*
 Considerar agregar: 
.Enrich.WithMachineName()
.Enrich.WithEnvironmentName()
.Enrich.WithAssemblyName()
.Enrich.WithMemoryUsage()
 */


/*
// Program.cs o Startup.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://adminrematestest.tiarica.cl")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Usar el middleware
app.UseCors("AllowAngularApp");
 */

// Configuración Serilog para producción
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Reduce logs de Microsoft
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "EcoCircular.API")
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
    .WriteTo.File(
        path: "Logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {MachineName} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        new CompactJsonFormatter(),
        "Logs/app-json-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

// Integrar Serilog con la aplicación
builder.Host.UseSerilog();

try
{
    Log.Information("🚀 Iniciando aplicación EcoCircular API...");
    Log.Information("Entorno: {Environment}", builder.Environment.EnvironmentName);
    Log.Debug("Directorio de trabajo: {CurrentDirectory}", Environment.CurrentDirectory);

    var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    Log.Debug("Ejecutando en contenedor: {IsContainer}", runningInContainer);


    // Valida existencia de archivo .env en entorno Development.
    if (builder.Environment.IsDevelopment() && !runningInContainer)
    {
        var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
            Log.Information("📁 Archivo .env cargado: {Path}", envPath);
        }
        else
        {
            Log.Warning("⚠️ Archivo .env no encontrado en: {Path}", envPath);
        }
    }

    builder.Configuration.AddEnvironmentVariables();

    string? conn = builder.Configuration.GetConnectionString("DbContext");
    string? issuer = builder.Configuration["JwtSettings:Issuer"];
    string? audience = builder.Configuration["JwtSettings:Audience"];
    string? secret = builder.Configuration["JwtSettings:Secret"];
    var apiKey = builder.Configuration["ApiKeyCL"];
    var expiryMinutes = builder.Configuration["ExpiryMinutes"];
    var baseUrl = builder.Configuration["BaseUrl"];
    var apiKeyGoogleCalendar = builder.Configuration["ApiKeyGoogleCalendar"];
    var regionalCalendarId = builder.Configuration["RegionalCalendarId"];
    var Endpoint = builder.Configuration["ClAuctionApi:Endpoint"];
    var ClApiKey = builder.Configuration["ClAuctionApi:ApiKey"];
    var ClBearerToken = builder.Configuration["ClAuctionApi:BearerToken"];

    // Logging SEGURO de configuración
    Log.Information("🔧 Configuración cargada:");
    Log.Debug("ConnectionString: {Conn}", MaskPassword(builder.Configuration.GetConnectionString("DbContext")));
    Log.Debug("Jwt Issuer: {Issuer}", builder.Configuration["JwtSettings:Issuer"]);
    Log.Debug("Jwt Audience: {Audience}", builder.Configuration["JwtSettings:Audience"]);
    Log.Debug("Jwt Secret: {Secret}", MaskSecret(builder.Configuration["JwtSettings:Secret"]));
    Log.Debug("API Key: {ApiKey}", MaskApiKey(builder.Configuration["ApiKeyCL"]));
    Log.Debug("Expiry Minutes: {ExpiryMinutes}", expiryMinutes);
    Log.Debug("Base URL: {BaseUrl}", baseUrl);
    Log.Debug("Google Calendar API Key: {GoogleKey}", MaskApiKey(apiKeyGoogleCalendar));
    Log.Debug("Regional Calendar ID: {CalendarId}", regionalCalendarId);
    Log.Debug("CL Auction Endpoint: {Endpoint}", Endpoint);
    Log.Debug("CL Auction API Key: {ClApiKey}", MaskApiKey(ClApiKey));
    Log.Debug("CL Bearer Token: {BearerToken}", MaskBearerToken(ClBearerToken));




    // Validación de variables requeridas
    var requiredSettings = new Dictionary<string, string>
    {
        ["DbContext"] = builder.Configuration.GetConnectionString("DbContext"),
        ["JwtSettings:Issuer"] = builder.Configuration["JwtSettings:Issuer"],
        ["JwtSettings:Audience"] = builder.Configuration["JwtSettings:Audience"],
        ["JwtSettings:Secret"] = builder.Configuration["JwtSettings:Secret"],
        ["ApiKeyCL"] = builder.Configuration["ApiKeyCL"],
        ["ExpiryMinutes"] = builder.Configuration["ExpiryMinutes"],
        ["BaseUrl"] = builder.Configuration["BaseUrl"],
        ["ApiKeyGoogleCalendar"] = builder.Configuration["ApiKeyGoogleCalendar"],
        ["RegionalCalendarId"] = builder.Configuration["RegionalCalendarId"],
        ["ClAuctionApi:Endpoint"] = builder.Configuration["ClAuctionApi:Endpoint"],
        ["ClAuctionApi:ApiKey"] = builder.Configuration["ClAuctionApi:ApiKey"],
        ["ClAuctionApi:BearerToken"] = builder.Configuration["ClAuctionApi:BearerToken"]
    };

    var missingVars = requiredSettings
        .Where(x => string.IsNullOrWhiteSpace(x.Value))
        .Select(x => x.Key)
        .ToList();

    if (missingVars.Any())
    {
        Log.Fatal("❌ Faltan variables requeridas: {MissingVariables}", string.Join(", ", missingVars));
        throw new InvalidOperationException($"Faltan variables requeridas: {string.Join(", ", missingVars)}");
    }

    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.AddServerHeader = false;
        Log.Debug("Kestrel configurado - Server header deshabilitado");
    });





    #region Configurating Services - Start

    // MANTÉN SOLO esta configuración original:
    builder.Services.AddDbContext<EcoCircularContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DbContext");
        Log.Information("🗄️ Configurando DbContext con conexión a BD");
        Log.Debug("Connection string: {ConnectionString}", MaskPassword(connectionString));

        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);

            Log.Debug("SQL Server retry policy configurado: 5 intentos, 10 segundos delay");
        });
    });


    //Add services to the container
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = true;
        });


    //Configuraciones
    builder.Services.Configure<GoogleCalendarConfig>(options =>
    {
        options.ApiKeyGoogleCalendar = builder.Configuration["ApiKeyGoogleCalendar"];
        options.RegionalCalendarId = builder.Configuration["RegionalCalendarId"];
    });

    // Repositorios
    builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
    builder.Services.AddScoped<IFuncionalidadRepository, FuncionalidadRepository>();
    builder.Services.AddScoped<IPerfilRepository, PerfilRepository>();
    builder.Services.AddScoped<IFichaProductoRepository, FichaProductoRepository>();
    builder.Services.AddScoped<IFeriadosRepository, FeriadosRepository>();



    //Use Cases
    builder.Services.AddScoped<GetAllFuncionalidadesUseCase>();
    builder.Services.AddScoped<ReceiveFichaUseCase>();



    //Services/Mappers/Providers
    builder.Services.AddScoped<IPermisoService, PermisoService>();
    builder.Services.AddScoped<IAuthorizationHandler, PermisoHandler>();
    builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
    builder.Services.AddScoped<GoogleCalendarService>();
    builder.Services.AddTransient<IFuncionalidadMapper, FuncionalidadMapper>();
    builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermisoPolicyProvider>();
    builder.Services.AddHostedService<HolidaysCronService>();
    builder.Services.AddScoped<IClAuctionApiService, ClAuctionApiService>();
    builder.Services.AddScoped<NotifyAuctionResultUseCase>();

    // Repositorios adicionales
    builder.Services.AddScoped<ICuentaBancariaRepository, CuentaBancariaRepository>();
    builder.Services.AddScoped<ICLPrendaRepository, CLPrendaRepository>();

    // Use Cases adicionales
    builder.Services.AddScoped<CuentaBancariaUseCase>();
    builder.Services.AddScoped<CLPrendaUpdateUseCase>();
    builder.Services.AddScoped<ExtraccionSubastaUseCase>();
    builder.Services.AddScoped<CargaResultadoSubastaUseCase>();

    // Repositorio de subasta
    builder.Services.AddScoped<ISubastaRepository, SubastaRepository>();

    // Use Case de subasta
    builder.Services.AddScoped<ExtraccionSubastaUseCase>();


    //Configurations
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });


    //Autenticación
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.AddSingleton(sp =>
        sp.GetRequiredService<IOptions<JwtSettings>>().Value);
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<ValidateModelAttribute>();

    // Custom Services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddHttpClient(); // Para llamadas a Clave Única


    // Authentication & Authorization
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
            };
        });

    builder.Services.AddAuthorization();


    builder.Services.AddHttpClient<IClAuctionApiService, ClAuctionApiService>(client =>
    {
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        // Agregar headers de autenticación
        var apiKey = builder.Configuration["ClAuctionApi:ApiKey"];
        var bearerToken = builder.Configuration["ClAuctionApi:BearerToken"];

        if (!string.IsNullOrEmpty(apiKey))
        {
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        if (!string.IsNullOrEmpty(bearerToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        // NO establecer BaseAddress - usarás URL absoluta en el servicio
    });




    builder.Services.AddSwaggerGen(c =>
    {
        c.DocumentFilter<DevelopmentDocumentFilter>();
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mi API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Ingrese el token JWT",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new string[] {}
        }
    });
    });

    builder.Services.AddEndpointsApiExplorer();
}

catch (Exception ex)
{
    Log.Fatal(ex, "💥 Error fatal durante el startup de la aplicación");
    throw;
}





// Métodos auxiliares para logging seguro
static string MaskPassword(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString)) return "null";
    var regex = new Regex(@"Password=([^;]+)", RegexOptions.IgnoreCase);
    return regex.Replace(connectionString, "Password=***");
}

static string MaskSecret(string secret)
{
    if (string.IsNullOrEmpty(secret)) return "null";
    return secret.Length > 4 ? $"{secret[..4]}..." : "***";
}

static string MaskApiKey(string apiKey)
{
    if (string.IsNullOrEmpty(apiKey)) return "null";
    return apiKey.Length > 8 ? $"{apiKey[..4]}...{apiKey[^4..]}" : "***";
}

static string MaskBearerToken(string token)
{
    if (string.IsNullOrEmpty(token)) return "null";
    return token.Length > 10 ? $"{token[..6]}...{token[^4..]}" : "***";
}


#endregion Configurating Services - End




#region Configurating Middleware - Start




var app = builder.Build();

// Fail Fast al inicio
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<EcoCircularContext>();

    Log.Information("🔄 Probando conexión a la base de datos...");
    var canConnect = dbContext.Database.CanConnect();

    if (canConnect)
    {
        Log.Information("✅ BD conectada - La aplicación puede iniciar");
        Log.Debug("Base de datos: {Database}", dbContext.Database.GetDbConnection().Database);
    }
    else
    {
        Log.Fatal("❌ La aplicación NO puede iniciar - Base de datos no disponible");
        throw new InvalidOperationException("Error de conexión a BD - La aplicación se detendrá");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 ERROR CRÍTICO: No se puede conectar a la base de datos");
    Log.Fatal(ex, "🛑 DETENIENDO LA EJECUCIÓN - Verifique la conexión a la base de datos y reinicie la aplicación");
    throw;
}


// Configuración de política de reintentos con Serilog
var retryPolicy = Policy
    .Handle<Exception>() // Captura cualquier excepción
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2, 4, 8 segundos
        onRetry: (exception, delay, attempt, context) =>
        {
            Log.Warning(exception, "⚠️  Reintento #{Attempt} en {DelaySeconds}s | Tipo: {ExceptionType} | Mensaje: {Message}",
                attempt,
                delay.TotalSeconds,
                exception.GetType().Name,
                exception.Message);
        }
    );

// Hacer la política disponible para inyección
//app.Services.AddSingleton(retryPolicy);
Log.Debug("🔄 Política de reintentos configurada: 3 intentos con backoff exponencial");

// Configuración de seguridad
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
Log.Debug("🌐 Forwarded headers configurado para X-Forwarded-For y X-Forwarded-Proto");

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Remove("ETag");
    }
});
Log.Debug("📁 Static files configurado - ETag headers removidos");

// Configuración de CORS
app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyMethod();
    policy.AllowAnyHeader();
});
Log.Information("🌍 CORS configurado: AllowAnyOrigin, AllowAnyMethod, AllowAnyHeader");

// Configuración de Swagger por entornos
if (app.Environment.IsDevelopment() ||
    app.Environment.IsProduction() ||
    app.Environment.IsStaging() ||
    app.Environment.EnvironmentName == "Sandbox")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoCircular API v1");
        c.RoutePrefix = string.Empty;
    });
    Log.Information("📚 Swagger habilitado para el entorno: {Environment}", app.Environment.EnvironmentName);
}

// Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();
Log.Information("🔐 Autenticación y autorización configuradas");

// Middlewares personalizados
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<DevelopmentMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
Log.Information("🛡️  Middlewares personalizados configurados: ExceptionHandling, Development, SecurityHeaders");

// Middleware condicional para API Key
app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/api/ReceiveFichaProducto") && ctx.Request.Method == "POST",
    pr => pr.UseMiddleware<ApiKeyMiddleware>()
);
Log.Information("🔑 Middleware de API Key configurado para: POST /api/ReceiveFichaProducto");

// Mapeo de controladores
app.MapControllers();
Log.Information("🗺️  Controladores mapeados - API lista para recibir requests");

// Inicio de la aplicación
Log.Information("""
    🚀 APLICACIÓN INICIADA CORRECTAMENTE
    Entorno: {Environment}
    Tiempo de inicio: {StartupTime}
    URLs: {Urls}
    """,
    app.Environment.EnvironmentName,
    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    string.Join(", ", app.Urls));

Log.Information("🎯 La aplicación está lista para procesar requests");

app.Run();

#endregion Configurating Middleware - End