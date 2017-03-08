using System;

namespace TfsAdvanced.Models.Errors
{
    public class JsonDeserializationException : Exception
    {
        public JsonDeserializationException(Type ExpectedType, string response)
            : base($"Error converting to {ExpectedType.Name}. {response}")
        {
            
        }
    }
}
