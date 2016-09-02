using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace TfsAdvanced.Infrastructure
{
    // You may need to install the Microsoft.AspNet.Http.Abstractions package into your project
    public class ClientCertificateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings appSettings;

        public ClientCertificateMiddleware(RequestDelegate next, IOptions<AppSettings> options)
        {
            _next = next;
            appSettings = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            //Validate the cert here
            try
            {
                bool isValidCert = false;
                X509Certificate2 certificate = null;

                string certHeader = context.Request.Headers["X-ARR-ClientCert"];
                if (String.IsNullOrEmpty(certHeader))
                    certHeader = context.Request.Headers["MS-ASPNETCORE-CLIENTCERT"];

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
                            Debug.WriteLine("Certificate is not valid");
                            context.Response.StatusCode = 403;
                        }
                    }
                    catch (Exception ex)
                    {
                        //What to do with exceptions in middleware?
                        Debug.WriteLine(ex.Message, ex);
                        await context.Response.WriteAsync(ex.Message);
                        context.Response.StatusCode = 403;
                    }
                }
                else
                {
                    Debug.WriteLine("X-ARR-ClientCert header is missing");
                    context.Response.StatusCode = 403;
                }
            }
            catch (Exception ex)
            {
                context.Response.Headers["Error"] = ex.Message;
                context.Response.StatusCode = 400;
            }
        }

        private bool IsValidClientCertificate(X509Certificate2 certificate)
        {
            var _config = appSettings.CertificateValidation;
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
}