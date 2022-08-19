using ESourcing.Sourcing.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESourcing.Sourcing.Repositories.Interface
{
    public interface IAuctionRepository
    {
        Task<IEnumerable<Auction>> GetAuctions();
        Task<Auction> GetAuction(string id);
        Task<Auction> GetAuctionByName(string name);
        Task Create(Auction auction);
        Task<bool> Update(Auction auction);
        Task<bool> Delete(string id);

    }
}
