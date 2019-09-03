using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using TfsAdvanced.Models.Infrastructure;

namespace TfsAdvanced.Models
{
    public class RequestData : IDisposable
    {
        private readonly AppSettings appSettings;

        public string BaseAddress { get; }

        public string BaseReleaseManagerAddress { get; }
        public HttpClient HttpClient { get; }
        
        public RequestData(IOptions<AppSettings> settings)
        {
            appSettings = settings.Value;
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            BaseAddress = appSettings.BaseAddress;

            var baseAddressParts = BaseAddress.Split('.');
            if (appSettings.BaseReleaseManagerAddress == null)
                BaseReleaseManagerAddress = $"{baseAddressParts[0]}.vsrm.{baseAddressParts[1]}.{baseAddressParts[2]}";
            else
                BaseReleaseManagerAddress = appSettings.BaseReleaseManagerAddress;

            HttpClient = new HttpClient(handler);
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TFSAdvanced");
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appSettings.Security.Username}:{appSettings.Security.Password}"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
        }

        public void Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}