namespace GradoCerrado.Application.Interfaces;

public interface IVectorService
{
    Task<bool> InitializeCollectionAsync();
    Task<string> AddDocumentAsync(string content, Dictionary<string, object> metadata);
    Task<List<SearchResult>> SearchSimilarAsync(string query, int limit = 5);
    Task<bool> DeleteDocumentAsync(string documentId);
    Task<bool> CollectionExistsAsync();
}

public class SearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}