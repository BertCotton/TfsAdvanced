using System;
using System.Net;

namespace TfsAdvanced.Models.Errors
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
