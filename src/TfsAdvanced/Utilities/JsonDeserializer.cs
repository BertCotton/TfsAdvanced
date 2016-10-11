using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TfsAdvanced.Data.Errors;

namespace TfsAdvanced.Utilities
{
    public class JsonDeserializer
    {
        public static async Task<T> Deserialie<T>(HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception)
            {
                throw new JsonDeserializationException(typeof(T), content);
            }
        }
    }
}
