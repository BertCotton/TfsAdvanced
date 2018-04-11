using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Errors;

namespace TfsAdvanced.Updater
{
    public class GetAsync
    {
        public static async Task<T> Fetch<T>(RequestData requestData, string url)
        {
            try
            {
                var response = await requestData.HttpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new BadRequestException(url, response.StatusCode);

                return await JsonDeserializer.Deserialize<T>(response);
            }
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static async Task<List<T>> FetchResponseList<T>(RequestData requestData, string url, ILogger logger = null)
        {
            try
            {
                var log = $"[{url}]  ";
                DateTime start = DateTime.Now;
                var response = await requestData.HttpClient.GetAsync(url);
                log += "Connetion Time: " + (DateTime.Now - start).TotalMilliseconds;
                if (!response.IsSuccessStatusCode)
                    throw new BadRequestException(url, response.StatusCode);

                start = DateTime.Now;
                var responseContext = await response.Content.ReadAsStringAsync();
                log += "  Read Time: " + (DateTime.Now - start).TotalMilliseconds;
                start = DateTime.Now;
                var items = JsonConvert.DeserializeObject<Response<IEnumerable<T>>>(responseContext, new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                });
                log += " Deserialization Time: " + (DateTime.Now - start).TotalMilliseconds;
                start = DateTime.Now;
                var list = items.value.ToList();
                log += " List Time: " + (DateTime.Now - start).TotalMilliseconds;

                if (logger != null)
                    logger.LogWarning(log);
                return list;
            }
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }
    }
}