using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.Data
{
    public class RequestData
    {
        private readonly AppSettings appSettings;

        public string BaseAddress { get; }
        public HttpClient HttpClient { get; }

        public RequestData(IOptions<AppSettings> settings)
        {
            appSettings = settings.Value;
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            BaseAddress = appSettings.BaseAddress;
            HttpClient = new HttpClient(handler);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationToken.base64_token);
            var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appSettings.Security.Username}:{appSettings.Security.Password}"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
        }
    }
}