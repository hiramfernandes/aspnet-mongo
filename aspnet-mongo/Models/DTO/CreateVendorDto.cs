using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Runtime.Serialization;

namespace aspnet_mongo.Models.DTO
{
    public class CreateVendorDto
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? LogoUrl { get; set; }
    }
}
