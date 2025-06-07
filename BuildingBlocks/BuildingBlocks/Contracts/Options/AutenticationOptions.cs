namespace BuildingBlocks.Contracts.Options
{
    public class AutenticationOptions
    {
        public const string Name = "Autentication";
        public required string SecretKey { get; set; }
        public required string Audience { get; set; }
        public required string Issuer { get; set; }
        public required double TokenExpirationMinutes { get; set; }
    }
}
