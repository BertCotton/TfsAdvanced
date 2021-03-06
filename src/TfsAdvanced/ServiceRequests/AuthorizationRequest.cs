﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TfsAdvanced.Models.Infrastructure;

namespace TfsAdvanced.ServiceRequests
{
    public class AuthorizationRequest
    {
        private readonly AppSettings appSettings;

        public AuthorizationRequest(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        //public string GetVSOChallengeUrl(string baseURL)
        //{
        //    return
        //        $"https://app.vssps.visualstudio.com/oauth2/authorize?" +
        //        $"client_id={appSettings.authorization.AppId}&response_type=Assertion" + 
        //        $"&state={appSettings.authorization.State}&scope={appSettings.authorization.Scope}" +
        //        $"&redirect_uri={baseURL}{appSettings.authorization.RedirectURI}";
        //}


        //public async Task<string> GetVSOAccessToken(string baseURL, string code, string state)
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Post, "https://app.vssps.visualstudio.com/oauth2/token");
        //    request.Content = new FormUrlEncodedContent(new[]
        //    {
        //        new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
        //        new KeyValuePair<string, string>("client_assertion", appSettings.authorization.AppSecret),
        //        new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
        //        new KeyValuePair<string, string>("assertion", code),
        //        new KeyValuePair<string, string>("redirect_uri", baseURL + appSettings.authorization.RedirectURI)
        //    });
        //    HttpClientHandler handler = new HttpClientHandler()
        //    {
        //        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        //    };
            
        //    var saveResponse = await new HttpClient(handler).SendAsync(request);

        //    var responseText = await saveResponse.Content.ReadAsStringAsync();
        //    return responseText;

        //    //AuthenticationToken token = JsonConvert.DeserializeObject<AuthenticationToken>(responseText);
        //    //return token;
        //}

        public string GetADChallengeUrl(string baseURL)
        {
            return $"https://login.microsoftonline.com/{appSettings.authorization.TenantId}/oauth2/authorize?" +
                $"client_id={appSettings.authorization.ClientId}" +
                $"&response_type=code&redirect_uri={baseURL}{appSettings.authorization.RedirectURI}" +
                $"&response_mode=query&resource=https://graph.windows.net&state=User" +
                $"&scope=profile%20openid";
        }

        public async Task<AuthenticationToken> GetADAccessToken(string baseURL, string code, string state)
        {

            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", appSettings.authorization.ClientId),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_secret", appSettings.authorization.ClientSecret),
                new KeyValuePair<string, string>("redirect_uri", $"{baseURL}{appSettings.authorization.RedirectURI}" )
            };


            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var client = new HttpClient(handler);
            
            var saveResponse = await client.PostAsync($"https://login.microsoftonline.com/{appSettings.authorization.TenantId}/oauth2/token", new FormUrlEncodedContent(content));

            var responseText = await saveResponse.Content.ReadAsStringAsync();

            
            var token = JsonConvert.DeserializeObject<AuthenticationToken>(responseText);
            token.expiredTime = DateTime.Now.AddSeconds(token.expires_in);
            if(token.access_token == null)
                throw new Exception("Unable to deserialize AuthenticationToken from response: " + responseText);

            return token;

        }
    }
}
