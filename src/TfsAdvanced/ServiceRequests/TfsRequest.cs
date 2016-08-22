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

        public RequestData GetRequestData()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appSettings.Security.Username}:{appSettings.Security.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
            return new RequestData
            {
                HttpClient = client,
                BaseAddress = appSettings.BaseAddress
            };
        }
    }
}