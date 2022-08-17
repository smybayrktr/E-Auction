using ESourcing.Products.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace ESourcing.Products.Data
{
    public class ProductContextSeed
    {
        public static void SeedData(IMongoCollection<Product> productCollection)
        {
            bool existProduct = productCollection.Find(p => true).Any();
            if (!existProduct)
            {
                productCollection.InsertManyAsync(GetConfigureProducts());
            }
        }

        private static IEnumerable<Product> GetConfigureProducts()
        {
            return new List<Product>() {

                new Product{Name= "Telefon", Category="Elektronik", Description= "Temiz 2.el", Price= 22, Summary="daha ne diyem"},
                new Product{Name= "Bilgisayar", Category="Elektronik", Description= "Temiz 2.el", Price= 24, Summary="daha ne diyem"},
                new Product{Name= "TV", Category="Elektronik", Description= "Temiz 2.el", Price= 29, Summary="daha ne diyem"},
                new Product{Name= "Soba", Category="Ev yaşam", Description= "Temiz 2.el", Price= 45, Summary="daha ne diyem" }
            };
        }
    }
}
