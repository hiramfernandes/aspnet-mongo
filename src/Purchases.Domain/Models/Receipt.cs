using System.Text.Json.Serialization;

namespace Purchases.Domain.Models;

public class Receipt
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("processed")]
    public bool Processed { get; set; }

    [JsonPropertyName("received-date")]
    public DateTime? ReceivedDate { get; set; }

    [JsonPropertyName("processed-date")]
    public DateTime? ProcessedDate { get; set; }
}
