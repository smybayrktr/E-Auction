using ESourcing.Sourcing.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESourcing.Sourcing.Repositories.Interface
{
    public interface IBidRepository
    {
        Task<List<Bid>> GetBidsByAuctionId(string id); //AuctionId verildiğinde o auctiona gelen teklifleri verir
        Task<Bid> GetWinnerBid(string id);//auction idyi  alıp kazanan ihaleyi verir.
        Task SendBid(Bid bid); //Teklifleri alır
    }
}
