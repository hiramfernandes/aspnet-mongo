using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.Serialization;

namespace aspnet_mongo.Models
{
    public class Vendor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [DataMember]
        [BsonElement("name")]
        public string? Name { get; set; }

        [DataMember]
        [BsonElement("location")]
        public string? Location { get; set; }


        [DataMember]
        [BsonElement("logoUrl")]
        public string? LogoUrl { get; set; }


        [DataMember]
        [BsonElement("addedOn")]
        public DateTime? AddedOn { get; set; }

        [DataMember]
        [BsonElement("updatedOn")]
        public DateTime? UpdatedOn { get; set; }
    }
}
