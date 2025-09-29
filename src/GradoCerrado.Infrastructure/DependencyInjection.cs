// src/GradoCerrado.Infrastructure/DependencyInjection.cs
using GradoCerrado.Application.Interfaces;
using GradoCerrado.Infrastructure.Configuration;
using GradoCerrado.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GradoCerrado.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar settings
        services.Configure<OpenAISettings>(configuration.GetSection(OpenAISettings.SectionName));
        services.Configure<QdrantSettings>(configuration.GetSection(QdrantSettings.SectionName));

        // Registrar servicios principales
        services.AddScoped<IAIService, OpenAIService>();
        services.AddScoped<IVectorService, QdrantService>();
        services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();

        // Registrar nuevos servicios para procesamiento de documentos y preguntas
        services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
        services.AddScoped<IQuestionGenerationService, QuestionGenerationService>();

        // Configurar HttpClient
        services.AddHttpClient();

        // ⚡ AGREGAR estas 2 líneas:
        services.Configure<AzureSpeechSettings>(configuration.GetSection(AzureSpeechSettings.SectionName));
        services.AddScoped<ISpeechService, AzureSpeechService>();

        return services;
    }
}