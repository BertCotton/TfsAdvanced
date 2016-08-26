using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Infrastructure
{
    public class CertificateValidation
    {
        public string Subject { get; set; }
        public string IssuerCN { get; set; }
        public string Thumbprint { get; set; }
    }
}