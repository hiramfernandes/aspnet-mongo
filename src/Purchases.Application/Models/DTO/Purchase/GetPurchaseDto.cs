namespace Purchases.Application.Models.DTO.Purchase
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
        public PurchaseItemDto[]? Items { get; set; }
    }

    public class PurchaseItemDto
    {
        public string? Description { get; set; }
        public float? UnitPrice { get; set; }
        public string[]? Tags { get; set; }
    }
}
