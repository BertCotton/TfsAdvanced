﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace TfsAdvanced.Infrastructure
{
    // You may need to install the Microsoft.AspNet.Http.Abstractions package into your project
    public class ClientCertificateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CertificateValidation _config;
        private readonly ILogger _logger;

        public ClientCertificateMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<AppSettings> options)
        {
            _next = next;
            _config = options.Value.CertificateValidation;
            _logger = loggerFactory.CreateLogger<ClientCertificateMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            //Validate the cert here

            bool isValidCert = false;
            X509Certificate2 certificate = null;

            string certHeader = context.Request.Headers["X-ARR-ClientCert"];

            if (!String.IsNullOrEmpty(certHeader))
            {
                try
                {
                    byte[] clientCertBytes = Convert.FromBase64String(certHeader);
                    certificate = new X509Certificate2(clientCertBytes);

                    isValidCert = IsValidClientCertificate(certificate);
                    if (isValidCert)
                    {
                        //Invoke the next middleware in the pipeline
                        await _next.Invoke(context);
                    }
                    else
                    {
                        //Stop the pipeline here.
                        _logger.LogError("Certificate is not valid");
                        context.Response.StatusCode = 403;
                    }
                }
                catch (Exception ex)
                {
                    //What to do with exceptions in middleware?
                    _logger.LogError(ex.Message, ex);
                    await context.Response.WriteAsync(ex.Message);
                    context.Response.StatusCode = 403;
                }
            }
            else
            {
                _logger.LogError("X-ARR-ClientCert header is missing");
                context.Response.StatusCode = 403;
            }
        }


        private bool IsValidClientCertificate(X509Certificate2 certificate)
        {
            // In this example we will only accept the certificate as a valid certificate if all the conditions below are met:
            // 1. The certificate is not expired and is active for the current time on server.
            // 2. The subject name of the certificate has the common name nildevecc
            // 3. The issuer name of the certificate has the common name nildevecc and organization name Microsoft Corp
            // 4. The thumbprint of the certificate is 30757A2E831977D8BD9C8496E4C99AB26CB9622B
            //
            // This example does NOT test that this certificate is chained to a Trusted Root Authority (or revoked) on the server
            // and it allows for self signed certificates
            //

            if (null == certificate) return false;

            // 1. Check time validity of certificate
            if (DateTime.Compare(DateTime.Now, certificate.NotBefore) < 0 || DateTime.Compare(DateTime.Now, certificate.NotAfter) > 0) return false;

            // 2. Check subject name of certificate
            bool foundSubject = false;
            string[] certSubjectData = certificate.Subject.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in certSubjectData)
            {
                if (String.Compare(s.Trim(), _config.Subject) == 0)
                {
                    foundSubject = true;
                    break;
                }
            }
            if (!foundSubject) return false;

            // 3. Check issuer name of certificate
            bool foundIssuerCN = false;
            string[] certIssuerData = certificate.Issuer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in certIssuerData)
            {
                if (String.Compare(s.Trim(), _config.IssuerCN) == 0)
                {
                    foundIssuerCN = true;
                    break;
                }

            }

            if (!foundIssuerCN) return false;

            // 4. Check thumprint of certificate
            if (String.Compare(certificate.Thumbprint.Trim().ToUpper(), _config.Thumbprint) != 0) return false;

            // If you also want to test if the certificate chains to a Trusted Root Authority you can uncomment the code below
            //
            //X509Chain certChain = new X509Chain();
            //certChain.Build(certificate);
            //bool isValidCertChain = true;
            //foreach (X509ChainElement chElement in certChain.ChainElements)
            //{
            //    if (!chElement.Certificate.Verify())
            //    {
            //        isValidCertChain = false;
            //        break;
            //    }
            //}
            //if (!isValidCertChain) return false;

            return true;
        }

    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ClientCertMiddlewareExtensions
    {
        public static IApplicationBuilder UseClientCertMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClientCertificateMiddleware>();
        }

        public static IApplicationBuilder UseClientCertMiddleware(this IApplicationBuilder builder, IOptions<CertificateValidation> options)
        {

            return builder.UseMiddleware<ClientCertificateMiddleware>(options);
        }
    }
}