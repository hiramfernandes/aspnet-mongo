namespace Purchases.Domain.Models.Settings
{
    public class JwtSettings
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string Key { get; set; }
    }
}
