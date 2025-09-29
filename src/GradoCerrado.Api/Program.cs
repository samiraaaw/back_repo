using GradoCerrado.Infrastructure;
using GradoCerrado.Domain.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ AGREGAR: Configuración de PostgreSQL
builder.Services.AddDbContext<GradocerradoContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// ✅ AGREGAR: Servicios de Infrastructure (OpenAI + Qdrant)  
builder.Services.AddInfrastructure(builder.Configuration);

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowIonic", policy =>
    {
        policy.WithOrigins("http://localhost:8100")  // Frontend Ionic
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ IMPORTANTE: CORS antes de Authorization
app.UseCors("AllowIonic");

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/api/status", () => new { 
    status = "OK", 
    timestamp = DateTime.UtcNow,
    message = "Backend funcionando correctamente"
});

app.Run();