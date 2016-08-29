using System.Collections.Generic;

namespace TfsAdvanced.Infrastructure
{
    public class AppSettings
    {
        public string BaseAddress { get; set; }
        public Security Security { get; set; }
        public string DatabaseConnection { get; set; }
        public List<string> Projects { get; set; }
        public CertificateValidation CertificateValidation { get; set; }
    }
}