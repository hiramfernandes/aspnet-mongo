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

        public double? TotalAmount { get; set; }

        [DataMember]
        [BsonElement("items")] 
        public string[]? Items { get; set; }
    }
}
