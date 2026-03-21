using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Runtime.Serialization;

namespace aspnet_mongo.Models
{
    [BsonIgnoreExtraElements]
    public class Purchase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [DataMember]
        [BsonElement("purchaseDate")]
        public DateTime? PurchaseDate { get; set; }

        [DataMember]
        [BsonElement("purchaseUrl")]
        public string? PurchaseUrl { get; set; }

        [DataMember]
        [BsonElement("vendorName")]
        public string? VendorName { get; set; }

        [DataMember]
        [BsonElement("vendorId")]
        public string? VendorId { get; set; }

        public double? TotalAmount { get; set; }

        [DataMember]
        [BsonElement("items")]
        public PurchaseItem[]? Items { get; set; }

        [DataMember]
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class PurchaseItem
    {
        [DataMember]
        [BsonElement("description")]
        public string? Description { get; set; }

        [DataMember]
        [BsonElement("unitPrice")]
        public float? UnitPrice { get; set; }

        [DataMember]
        [BsonElement("tags")]
        public string[]? Tags { get; set; }
    }
}
