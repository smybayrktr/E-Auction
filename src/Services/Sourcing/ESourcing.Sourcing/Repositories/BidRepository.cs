using ESourcing.Sourcing.Data.Interface;
using ESourcing.Sourcing.Entities;
using ESourcing.Sourcing.Repositories.Interface;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESourcing.Sourcing.Repositories
{
    public class BidRepository : IBidRepository
    {
        private readonly ISourcingContext _context;

        public BidRepository(ISourcingContext context)
        {
            _context = context;
        }
        public async Task<List<Bid>> GetBidsByAuctionId(string id) //istenilen auction idnin tekliflerini liste halinde getirir
        {
            FilterDefinition<Bid> filter = Builders<Bid>.Filter.Eq(a => a.AuctionId, id);
            List<Bid> bids = await _context.Bids.Find(filter).ToListAsync();
            bids=bids.OrderByDescending(b => b.CreatedAt)
                                        .GroupBy(b=>b.SellerUserName)   
                                        .Select(b=>new Bid
                                        {
                                            AuctionId = b.FirstOrDefault().AuctionId,
                                            Price = b.FirstOrDefault().Price,
                                            CreatedAt = b.FirstOrDefault().CreatedAt,
                                            SellerUserName = b.FirstOrDefault().SellerUserName,
                                            ProductId = b.FirstOrDefault().ProductId,
                                            Id = b.FirstOrDefault().Id
                                        })
                                        .ToList(); //Listedeki en yeni elemanı en üste koyar. Bu teklifleri getirirkende
                                                   //yeni teklif veren en üstte olacak şekilde getirir. Satıcıların son teklifleri gelir
            return bids;
        }

        public async Task<Bid> GetWinnerBid(string id) //Verilen teklifler içinden kazanan teklifi verir.
        {
            List<Bid> bids = await GetBidsByAuctionId(id);
            return bids.OrderByDescending(b => b.Price).FirstOrDefault();
        }

        public async Task SendBid(Bid bid) //İhale süresi boyunca verilen teklifleri veritabanına kaydeder.
        {
           await _context.Bids.InsertOneAsync(bid);
        }
    }
}
