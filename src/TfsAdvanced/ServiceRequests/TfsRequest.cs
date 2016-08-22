using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace TfsAdvanced.ServiceRequests
{
    public class TfsRequest
    {
        private readonly AppSettings appSettings;

        public TfsRequest(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public RequestData GetCloudHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appSettings.CloudSecurity.Username}:{appSettings.CloudSecurity.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
            return new RequestData
            {
                HttpClient = client,
                BaseAddress = appSettings.CloudBaseAddress
            };
        }

        public RequestData GetHttpClient()
        {
            var authHandler = new HttpClientHandler() { Credentials = CredentialCache.DefaultNetworkCredentials, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            authHandler.Credentials = new NetworkCredential { Domain = appSettings.Security.Domain, UserName = appSettings.Security.Username, Password = appSettings.Security.Password };
            var httpClient = new HttpClient(authHandler);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            return new RequestData
            {
                HttpClient = httpClient,
                BaseAddress = appSettings.OnSiteBaseAddress
            };
        }
    }
}