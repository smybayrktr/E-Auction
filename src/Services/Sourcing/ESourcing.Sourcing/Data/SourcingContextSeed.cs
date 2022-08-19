using ESourcing.Sourcing.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace ESourcing.Sourcing.Data
{
    public class SourcingContextSeed
    {
        public static void SeedData(IMongoCollection<Auction> auctionCollection)
        {
            bool existProduct = auctionCollection.Find(p => true).Any();
            if (!existProduct) //Data varsa çalışmaz yoksa dataseed işlemini yapar
            {
                auctionCollection.InsertManyAsync(GetConfigureAuctions());
            }
        }

        private static IEnumerable<Auction> GetConfigureAuctions()
        {
            return new List<Auction>() {

                new Auction {
                            Name= "Auction1", 
                            Description= "Description1", 
                            StartedAt=DateTime.Now,
                            CreatedAt= DateTime.Now, 
                            FinishedAt= DateTime.Now.AddDays(10),
                            IncludedSellers=new List<string>()
                            {
                                "seller1@gmail.com",
                                "seller2@gmail.com",
                                "seller3@gmail.com",
                                "seller4@gmail.com",
                            },
                            Quantity=3, Status=(int)Status.Active,
                            ProductId="111111111111111111111111",
                            },

                new Auction {
                            Name= "Auction2",
                            Description= "Description2",
                            StartedAt=DateTime.Now,
                            CreatedAt= DateTime.Now,
                            FinishedAt= DateTime.Now.AddDays(10),
                            IncludedSellers=new List<string>()
                            {
                                "seller1@gmail.com",
                                "seller2@gmail.com",
                                "seller3@gmail.com",
                                "seller4@gmail.com",
                            },
                            Quantity=3, Status=(int)Status.Active,
                            ProductId="111111111111101111111111",
                            }
            };
        }
    }
}
