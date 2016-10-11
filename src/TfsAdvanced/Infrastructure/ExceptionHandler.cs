using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TfsAdvanced.Data.Errors;

namespace TfsAdvanced.Infrastructure
{
    public class ExceptionHandler : IExceptionFilter, IDisposable
    {
        public void OnException(ExceptionContext context)
        {
            var response = new Dictionary<string, object>()
            {
                {"Message", context.Exception.Message},
                {"StackTrace", context.Exception.StackTrace}
            };

            var statusCode = 500;

            if (context.Exception.GetType() == typeof(BadRequestException))
                statusCode = ((BadRequestException)context.Exception).StatusCode;
            else if(context.Exception.InnerException != null && context.Exception.InnerException.GetType() == typeof(BadRequestException))
                statusCode = ((BadRequestException)context.Exception.InnerException).StatusCode;

            context.Result = new ObjectResult(response)
            {
                StatusCode = statusCode,
                DeclaredType = typeof(Dictionary<string, object>)
            };
        }

        public void Dispose()
        {
        }
    }
}
