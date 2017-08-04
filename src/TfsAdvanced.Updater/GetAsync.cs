using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Errors;

namespace TfsAdvanced.Updater
{
    public class GetAsync
    {
        public static async Task<T> Fetch<T>(RequestData requestData, string url)
        {
            var response = await requestData.HttpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new BadRequestException(url, response.StatusCode);

            return await JsonDeserializer.Deserialize<T>(response);
        }

        public static async Task<List<T>> FetchResponseList<T>(RequestData requestData, string url)
        {
            var response = await requestData.HttpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new BadRequestException(url, response.StatusCode);

            var responseContext = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<Response<IEnumerable<T>>>(responseContext, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });
            return items.value.ToList();
        }
    }
}
