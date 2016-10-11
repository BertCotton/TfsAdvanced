using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.Errors
{
    public class BadRequestException : Exception
    {
        public int StatusCode { get; set; }

        public BadRequestException(string url, HttpStatusCode responseCode)
            : base($"Error making request to {url}.  Response Code {responseCode}")
        {
            this.StatusCode = (int) responseCode;
        }
    }
}
