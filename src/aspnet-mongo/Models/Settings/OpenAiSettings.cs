namespace aspnet_mongo.Models.Settings
{
    public class OpenAiSettings
    {
        public required string? ApiKey { get; set; }
        public required string? Model { get; set; }
        public required string? Endpoint { get; set; }
        public bool TestMode { get; set; }
    }
}
