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
    public static class HttpTrigger1
    {
        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
 [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
  ILogger log)
        {
            // Get the authentication code from the request payload
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string authCode = data.authCode;

            // Get the Application details from the settings
            string tenantId = Environment.GetEnvironmentVariable("TenantId", EnvironmentVariableTarget.Process);
            string clientId = Environment.GetEnvironmentVariable("ClientId", EnvironmentVariableTarget.Process);
            string clientSecret = Environment.GetEnvironmentVariable("ClientSecret", EnvironmentVariableTarget.Process);
            string storageConnectionString = Environment.GetEnvironmentVariable("TableStorage", EnvironmentVariableTarget.Process);

            // Get the access token from MS Identity
            MicrosoftIdentityClient idClient = new MicrosoftIdentityClient(clientId, clientSecret, tenantId);
            (string idToken, string accessToken, string refreshToken) tokens = await idClient.GetAccessTokenFromAuthorizationCode(authCode);

            // Save the refresh token to an Azure Storage Table
            AzureStorageClient azureStorageClient = new AzureStorageClient(storageConnectionString);
            await azureStorageClient.AddOrUpdateRefreshToken(clientId, MicrosoftIdentityClient.GetIdTokenUniqueUser(tokens.idToken), tokens.refreshToken);

            return new OkObjectResult(new ReturnValue
            {
                AccessToken = tokens.accessToken,
                IdToken = tokens.idToken
            });
        }
    }
}
