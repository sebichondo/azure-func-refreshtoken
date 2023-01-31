using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;


public class MicrosoftIdentityClient
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly string hostUrl = "https://login.microsoftonline.com";

    private readonly string tenantId;
    private readonly string clientId;
    private readonly string clientSecret;

    public MicrosoftIdentityClient(string clientId, string clientSecret, string tenantId)
    {
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        this.tenantId = tenantId;
    }
    public static string GetIdTokenUniqueUser(string idToken)
    {
        JwtSecurityToken securityToken = new JwtSecurityToken(idToken);
        string tid = securityToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "tid").Value;
        string sub = securityToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "sub").Value;

        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{tid}{sub}"));
    }

    public async Task<(string idToken, string accessToken, string refreshToken)> GetAccessTokenFromAuthorizationCode(string authCode)
    {
        string redirectUrl = "http://localhost:7071";
        string scopes = "openid offline_access https://graph.microsoft.com/user.read";

        Uri requestUri = new Uri($"{hostUrl}/{this.tenantId}/oauth2/v2.0/token");

        List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>()
    {
        new KeyValuePair<string, string>("client_id", this.clientId),
        new KeyValuePair<string, string>("scope", scopes),
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("code", authCode),
        new KeyValuePair<string, string>("redirect_uri", redirectUrl),
        new KeyValuePair<string, string>("client_secret", this.clientSecret)
    };

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new FormUrlEncodedContent(content),
        };

        HttpResponseMessage response = await httpClient.SendAsync(request);

        string responseContent = await response.Content.ReadAsStringAsync();
        dynamic responseObject = JsonConvert.DeserializeObject(responseContent);

        if (response.IsSuccessStatusCode)
        {
            return (responseObject.id_token, responseObject.access_token, responseObject.refresh_token);
        }
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // Something failed along the way, and there will be an error in there if the error code is 400
            // Handle it however you want.
            throw new Exception((string)responseObject.error_message);
        }
        else
        {
            // ¯\_(ツ)_/¯
            throw new Exception("Something bad happened");
        }
    }

    public async Task<(string idToken, string accessToken, string refreshToken)> GetAccessTokenFromRefreshToken(string refreshToken)
    {
        string scopes = "openid offline_access https://graph.microsoft.com/user.read";

        Uri requestUri = new Uri($"{hostUrl}/{this.tenantId}/oauth2/v2.0/token");

        List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>()
    {
        new KeyValuePair<string, string>("client_id", this.clientId),
        new KeyValuePair<string, string>("scope", scopes),
        new KeyValuePair<string, string>("grant_type", "refresh_token"),
        new KeyValuePair<string, string>("refresh_token", refreshToken),
        new KeyValuePair<string, string>("client_secret", this.clientSecret)
    };

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new FormUrlEncodedContent(content),
        };

        HttpResponseMessage response = await httpClient.SendAsync(request);

        string responseContent = await response.Content.ReadAsStringAsync();
        dynamic responseObject = JsonConvert.DeserializeObject(responseContent);

        if (response.IsSuccessStatusCode)
        {
            return (responseObject.id_token, responseObject.access_token, responseObject.refresh_token);
        }
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // Something failed along the way, and there will be an error in there if the error code is 400
            // Handle it however you want.
            throw new Exception((string)responseObject.error_message);
        }
        else
        {
            // ¯\_(ツ)_/¯
            throw new Exception("Something bad happened");
        }
    }
}