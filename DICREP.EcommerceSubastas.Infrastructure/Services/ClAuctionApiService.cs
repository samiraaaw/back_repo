using DICREP.EcommerceSubastas.Application.DTOs.FichaProducto;
using DICREP.EcommerceSubastas.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace DICREP.EcommerceSubastas.Infrastructure.Services
{
    public class ClAuctionApiService : IClAuctionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public ClAuctionApiService( HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = Log.ForContext<ClAuctionApiService>();
        }


        public async Task<ClApiResponse> UpdateProductStatusAsync(string productId, int statusId)
        {
            try
            {
                var endpoint = _configuration["ClAuctionApi:Endpoint"];
                var apiKey = _configuration["ClAuctionApi:ApiKey"];
                var bearerToken = _configuration["ClAuctionApi:BearerToken"];


                _logger.Information("Iniciando actualización para producto {ProductId}", productId);
                _logger.Debug("Endpoint: {Endpoint}, API Key: {ApiKeyPartial}", endpoint,
                    string.IsNullOrEmpty(apiKey) ? "null" : apiKey.Substring(0, Math.Min(4, apiKey.Length)) + "***");

                var requestData = new
                {
                    id_publicacion_bien = productId,
                    IdEstado = statusId
                };

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("x-api-key", apiKey);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                _logger.Information("Enviando actualización de estado a CL API para producto {ProductId}", productId);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.Debug("Respuesta de CL API: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ClApiRawResponse>(responseContent);

                    if (!string.IsNullOrEmpty(result?.error))
                    {
                        _logger.Warning("CL API retornó error para producto {ProductId}: {Error}",
                            productId, result.error);

                        return new ClApiResponse
                        {
                            Success = false,
                            Error = result.error
                        };
                    }

                    _logger.Information("CL API actualizó exitosamente producto {ProductId}: {Message}",
                        productId, result?.message);

                    return new ClApiResponse
                    {
                        Success = true,
                        Message = result?.message
                    };
                }

                _logger.Error("Error HTTP {StatusCode} de CL API para producto {ProductId}: {Content}",
                    (int)response.StatusCode, productId, responseContent);

                return new ClApiResponse
                {
                    Success = false,
                    Error = $"HTTP Error: {response.StatusCode} - {responseContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Excepción al llamar CL API para producto {ProductId}", productId);
                return new ClApiResponse
                {
                    Success = false,
                    Error = $"Exception: {ex.Message}"
                };
            }
        }
    }

    // Clase para deserializar la respuesta cruda de la API
    public class ClApiRawResponse
    {
        [JsonPropertyName("error")]
        public string error { get; set; }

        [JsonPropertyName("message")]
        public string message { get; set; }
    }
}
