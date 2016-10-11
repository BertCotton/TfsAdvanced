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
            return
                $"https://app.vssps.visualstudio.com/oauth2/authorize?client_id={appSettings.authorization.ClientId}&response_type=Assertion&state={appSettings.authorization.State}&scope={appSettings.authorization.Scope}&redirect_uri={baseURL}{appSettings.authorization.RedirectURI}";
        }

        public async Task<string> Authorize()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var HttpClient = new HttpClient(handler);

            var response = await HttpClient.GetAsync("https://app.vssps.visualstudio.com/oauth2/authorize?client_id={appSettings.authorization.ClientId}&response_type=Assertion&state={appSettings.authorization.State}&scope={appSettings.authorization.Scope}&redirect_uri={appSettings.authorization.RedirectURI}");
            var code = response.StatusCode;
            var body = await response.Content.ReadAsStringAsync();
            return body;

        }

        public async Task<AuthenticationToken> GetAccessToken(string code, string state)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://app.vssps.visualstudio.com/oauth2/token");
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                new KeyValuePair<string, string>("client_assertion", appSettings.authorization.AppSecret),
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                new KeyValuePair<string, string>("assertion", code),
                new KeyValuePair<string, string>("redirect_uri", appSettings.authorization.RedirectURI)
            });
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var HttpClient = new HttpClient(handler);

            var saveResponse = await HttpClient.SendAsync(request);

            var responseText = await saveResponse.Content.ReadAsStringAsync();

            AuthenticationToken token = JsonConvert.DeserializeObject<AuthenticationToken>(responseText);
            return token;
        }
    }
}
