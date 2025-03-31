using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Interfaces.Graph;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace LSC.SmartCertify.Application.Services.Graph
{
    public class GraphService : IGraphService
    {
        private readonly IGraphAuthService graphAuthService;
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;
        private readonly ILogger<GraphService> logger;

        public GraphService(IGraphAuthService  graphAuthService, IConfiguration configuration, HttpClient httpClient,
            ILogger<GraphService> logger)
        {
            this.graphAuthService = graphAuthService;
            this.configuration = configuration;
            this.httpClient = httpClient;
            this.logger = logger;
        }
        public async Task<List<AdB2CUserModel>> GetADB2CUsersAsync()
        {
            var accessToken = await graphAuthService.GetAccessTokenAsync();
            var graphEndpoint = configuration["AzureAdB2CGraph:GraphEndpoint"] + "users?$select=id,givenName,surname,displayName,userPrincipalName,mail,otherMails";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync(graphEndpoint);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"IsSuccessStatusCode : {(int)response.StatusCode}, {content}");
                return new List<AdB2CUserModel>();
            }

            // Deserialize using GraphApiResponse wrapper class
            var graphResponse = JsonConvert.DeserializeObject<GraphApiResponse>(content);

            // Ensure we have users before filtering
            if (graphResponse?.Users == null)
            {
                logger.LogInformation("graphResponse?.Users received as null");
                return new List<AdB2CUserModel>();
            }

            // Filter users that have an email
            List<AdB2CUserModel> usersWithEmail = graphResponse.Users
              .Where(u => !string.IsNullOrEmpty(u.Mail) || (u.OtherMails != null && u.OtherMails.Any()))
              .Select(u => new AdB2CUserModel
              {
                  Id = u.Id,
                  GivenName = u.GivenName,
                  Surname = u.Surname,
                  DisplayName = u.DisplayName,
                  UserPrincipalName = u.UserPrincipalName,
                  Mail = u.Mail,
                  OtherMails = u.OtherMails
              })
            .ToList();

            return usersWithEmail;
        }
    }
}
