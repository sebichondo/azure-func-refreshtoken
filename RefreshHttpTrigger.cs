using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace bundilabs.auth
{
    public static class RefreshHttpTrigger
    {
        [FunctionName("RefreshHttpTrigger")]
        public static async Task<IActionResult> Run(
              [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            // Get the authentication code from the request payload
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string idToken = data.idToken;

            // Get the Application details from the settings
            string tenantId = Environment.GetEnvironmentVariable("TenantId", EnvironmentVariableTarget.Process);
            string clientId = Environment.GetEnvironmentVariable("ClientId", EnvironmentVariableTarget.Process);
            string clientSecret = Environment.GetEnvironmentVariable("ClientSecret", EnvironmentVariableTarget.Process);
            string storageConnectionString = Environment.GetEnvironmentVariable("TableStorage", EnvironmentVariableTarget.Process);

            // Get the Object ID
            MicrosoftIdentityClient idClient = new MicrosoftIdentityClient(clientId, clientSecret, tenantId);
            string userId = MicrosoftIdentityClient.GetIdTokenUniqueUser(idToken);

            // Get the refresh token 
            AzureStorageClient azureStorageClient = new AzureStorageClient(storageConnectionString);
            string refreshToken = await azureStorageClient.GetRefreshToken(clientId, userId);

            // Get a new access token from the refresh token
            (string idToken, string accessToken, string refreshToken) tokens = await idClient.GetAccessTokenFromRefreshToken(refreshToken);

            // Save the refresh token to an Azure Storage Table
            await azureStorageClient.AddOrUpdateRefreshToken(clientId, userId, tokens.refreshToken);

            return new OkObjectResult(new ReturnValue
            {
                AccessToken = tokens.accessToken,
                IdToken = tokens.idToken
            });
        }
    }
}