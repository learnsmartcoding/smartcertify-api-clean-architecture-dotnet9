using LSC.SmartCertify.Application.Interfaces.Graph;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace LSC.SmartCertify.Application.Services.Graph
{

    public class GraphAuthService: IGraphAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GraphAuthService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var tenantId = _configuration["AzureAdB2CGraph:TenantId"];
            var clientId = _configuration["AzureAdB2CGraph:ClientId"];
            var clientSecret = _configuration["AzureAdB2CGraph:ClientSecret"];
            var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var body = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default")
        });

            var response = await _httpClient.PostAsync(tokenUrl, body);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error getting token: {responseContent}");

            var token = JObject.Parse(responseContent)["access_token"]?.ToString();
            return token ?? throw new Exception("Token not found in response");
        }
    }

}
