namespace DICREP.EcommerceSubastas.API.Security
{
    public static class Constants
    {
        public const string ApiKeyHeaderName = "X-API-Key";
        public const string ApiKeyName = "ApiKeyCL";
    }

    public interface IApiKeyValidation
    {
        bool IsValid(string apiKey);
    }

    public class ApiKeyValidation : IApiKeyValidation
    {
        private readonly IConfiguration _cfg;
        public ApiKeyValidation(IConfiguration cfg) => _cfg = cfg;

        public bool IsValid(string apiKey) =>
            !string.IsNullOrEmpty(apiKey) &&
            apiKey == _cfg.GetValue<string>(Constants.ApiKeyName);
    }

}
