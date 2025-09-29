using GradoCerrado.Application.Interfaces;
using GradoCerrado.Infrastructure.Configuration;
using GradoCerrado.Infrastructure.DTOs;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace GradoCerrado.Infrastructure.Services;

public class QdrantService : IVectorService
{
    private readonly HttpClient _httpClient;
    private readonly QdrantSettings _settings;
    private readonly IEmbeddingService _embeddingService;
    private readonly string _baseUrl;

    public QdrantService(IOptions<QdrantSettings> settings, HttpClient httpClient, IEmbeddingService embeddingService)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _embeddingService = embeddingService;
        _baseUrl = _settings.Url.TrimEnd('/');

        // Configurar headers para Qdrant Cloud
        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);
        }
    }

    public async Task<bool> CollectionExistsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/collections");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var collectionsResponse = JsonSerializer.Deserialize<QdrantCollectionsResponse>(content);
                return collectionsResponse?.result?.collections?.Any(c => c.name == _settings.CollectionName) ?? false;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verificando colección: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> InitializeCollectionAsync()
    {
        try
        {
            // Verificar si la colección ya existe
            if (await CollectionExistsAsync())
            {
                Console.WriteLine($"La colección '{_settings.CollectionName}' ya existe");
                return true;
            }

            // Crear la colección
            var createRequest = new
            {
                vectors = new
                {
                    size = _settings.VectorSize,
                    distance = _settings.Distance
                }
            };

            var json = JsonSerializer.Serialize(createRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(
                $"{_baseUrl}/collections/{_settings.CollectionName}", 
                content
            );

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Colección '{_settings.CollectionName}' creada exitosamente");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error creando colección: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando colección: {ex.Message}");
            return false;
        }
    }

    public async Task<string> AddDocumentAsync(string content, Dictionary<string, object> metadata)
    {
        try
        {
            var documentId = Guid.NewGuid().ToString();
            
            // Generar embedding real del contenido usando OpenAI
            var vector = await _embeddingService.GenerateEmbeddingAsync(content);

            // Preparar payload
            var payload = new Dictionary<string, object>(metadata)
            {
                ["content"] = content
            };

            var point = new
            {
                id = documentId,
                vector = vector,
                payload = payload
            };

            var upsertRequest = new
            {
                points = new[] { point }
            };

            var json = JsonSerializer.Serialize(upsertRequest);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(
                $"{_baseUrl}/collections/{_settings.CollectionName}/points", 
                httpContent
            );

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Documento agregado con embedding real: {documentId}");
                return documentId;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error agregando documento: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error agregando documento: {ex.Message}");
            throw;
        }
    }

    public async Task<List<SearchResult>> SearchSimilarAsync(string query, int limit = 5)
    {
        try
        {

            // 🛡️ VALIDACIÓN: Manejar query vacía
            if (string.IsNullOrWhiteSpace(query))
            {
                // Para "listar todos", usar una query genérica
                query = "documento legal";
            }

            // Generar embedding real de la consulta usando OpenAI
            var queryVector = await _embeddingService.GenerateEmbeddingAsync(query);

            var searchRequest = new
            {
                vector = queryVector,
                limit = limit,
                with_payload = true
            };

            var json = JsonSerializer.Serialize(searchRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/collections/{_settings.CollectionName}/points/search", 
                content
            );

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var searchResponse = JsonSerializer.Deserialize<QdrantSearchResponse>(responseContent);

                var results = searchResponse?.result?.Select(hit => new SearchResult
                {
                    Id = hit.id?.ToString() ?? "",
                    Content = hit.payload?.GetValueOrDefault("content")?.ToString() ?? "",
                    Score = hit.score,
                    Metadata = hit.payload ?? new Dictionary<string, object>()
                }).ToList() ?? new List<SearchResult>();

                Console.WriteLine($"Búsqueda semántica completada para: '{query}', resultados: {results.Count}");
                return results;
            }
            else
            {
                Console.WriteLine($"Error en búsqueda: {response.StatusCode}");
                return new List<SearchResult>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en búsqueda: {ex.Message}");
            return new List<SearchResult>();
        }
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        try
        {
            var deleteRequest = new
            {
                points = new[] { documentId }
            };

            var json = JsonSerializer.Serialize(deleteRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/collections/{_settings.CollectionName}/points/delete", 
                content
            );

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Documento eliminado: {documentId}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error eliminando documento: {ex.Message}");
            return false;
        }
    }
}