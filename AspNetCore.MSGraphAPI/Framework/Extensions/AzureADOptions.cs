namespace AspNetCore.MSGraphAPI.Framework.Extensions
{
    public class AzureADOptions
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Authority { get; set; }

        public string TenantId { get; set; }

        public string CallbackPath { get; set; }

        public string BaseUrl { get; set; }

        public string Scopes { get; set; }

        public string GraphResourceId { get; set; }

        public string GraphScopes { get; set; }
    }
}
