namespace aspnet_mongo.Models.DTO
{
    public class PurchaseDto
    {
        public string? Description { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? Url { get; set; }
        public string? VendorName { get; set; }
        public double? TotalAmount { get; set; }
        public string[]? Items { get; set; }
    }
}
