namespace aspnet_mongo.Models.DTO
{
    public class GetPurchaseDto
    {
        public string? Id { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public string? PurchaseUrl { get; set; }

        public string? VendorName { get; set; }

        public string? VendorId { get; set; }

        public double? TotalAmount { get; set; }

        public string? VendorLogoUrl { get; set; }
    }
}
