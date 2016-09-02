namespace TfsAdvanced.Infrastructure
{
    public class CertificateValidation
    {
        public string Subject { get; set; }
        public string IssuerCN { get; set; }
        public string Thumbprint { get; set; }
    }
}