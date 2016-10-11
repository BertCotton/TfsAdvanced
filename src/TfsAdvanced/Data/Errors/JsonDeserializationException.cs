using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.Errors
{
    public class JsonDeserializationException : Exception
    {
        public JsonDeserializationException(Type ExpectedType, string response)
            : base($"Error converting to {ExpectedType.Name}. {response}")
        {
            
        }
    }
}
