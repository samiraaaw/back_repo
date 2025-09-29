using GradoCerrado.Application.Interfaces;
using GradoCerrado.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;

namespace GradoCerrado.Infrastructure.Services;

public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _embeddingClient;
    private readonly OpenAISettings _settings;

    public OpenAIEmbeddingService(IOptions<OpenAISettings> settings)
    {
        _settings = settings.Value;
        var client = new OpenAIClient(_settings.ApiKey);
        _embeddingClient = client.GetEmbeddingClient("text-embedding-ada-002");
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var embedding = await _embeddingClient.GenerateEmbeddingAsync(text);
            return embedding.Value.ToFloats().ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generando embedding: {ex.Message}");
            throw;
        }
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
    {
        try
        {
            var embeddings = new List<float[]>();
            
            // Procesar en lotes para evitar límites de rate
            const int batchSize = 5;
            for (int i = 0; i < texts.Count; i += batchSize)
            {
                var batch = texts.Skip(i).Take(batchSize).ToList();
                var batchTasks = batch.Select(GenerateEmbeddingAsync);
                var batchResults = await Task.WhenAll(batchTasks);
                embeddings.AddRange(batchResults);
                
                // Pequeña pausa entre lotes
                if (i + batchSize < texts.Count)
                {
                    await Task.Delay(100);
                }
            }
            
            return embeddings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generando embeddings en lote: {ex.Message}");
            throw;
        }
    }
}