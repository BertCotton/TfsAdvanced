using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TfsAdvanced.Models.Errors;

namespace TfsAdvanced.Updater
{
    public class JsonDeserializer
    {
        public static async Task<T> Deserialize<T>(HttpResponseMessage responseMessage)
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
