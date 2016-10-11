using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.ServiceRequests
{
    public class AuthorizationRequest
    {
        private readonly AppSettings appSettings;

        public AuthorizationRequest(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public string GetChallengeUrl(string baseURL)
        {
            var auth = appSettings.authorization;
            return
                $"https://login.microsoftonline.com/{auth.TenantId}/oauth2/authorize?client_id={appSettings.authorization.ClientId}&response_type=code&state={appSettings.authorization.State}&scope={appSettings.authorization.Scope}&redirect_uri={baseURL}{appSettings.authorization.RedirectURI}&prompt=consent";
        }


        public async Task<string> GetAccessToken(string baseURL, string code, string state)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://login.microsoftonline.com/{appSettings.authorization.TenantId}/oauth2/token")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_secret", appSettings.authorization.AppSecret),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", baseURL + appSettings.authorization.RedirectURI),
                    new KeyValuePair<string, string>("resource", baseURL),
                    new KeyValuePair<string, string>("client_id", appSettings.authorization.ClientId)
                })
            };
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            
            var saveResponse = await new HttpClient(handler).SendAsync(request);

            var responseText = await saveResponse.Content.ReadAsStringAsync();
            return responseText;

            //AuthenticationToken token = JsonConvert.DeserializeObject<AuthenticationToken>(responseText);
            //return token;
        }
    }
}
