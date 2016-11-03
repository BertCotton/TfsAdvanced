using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Data.Errors;

namespace TfsAdvanced.Utilities
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

            var items = await JsonDeserializer.Deserialize<Response<IEnumerable<T>>>(response);
            return items.value.ToList();
        }
    }
}
