using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ESourcing.Products.Entities
{
    public class Product
    {
        [BsonId] //Id olduğunu belirtmek için yazıldı.
        [BsonRepresentation(BsonType.ObjectId)] //1 1 artması için yazıldı.
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }
        public string Category { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string ImageFile { get; set; }
        public decimal Price { get; set; }
    }
}
