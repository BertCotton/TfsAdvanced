namespace TfsAdvanced.Models.Infrastructure
{
    public class AuthorizationSettings
    {
        public string TenantId { get; set; }

        public string AppId { get; set; }
        public string AppSecret { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string State { get; set; }

        public string Scope { get; set; }

        public string RedirectURI { get; set; }

        
    }
}
