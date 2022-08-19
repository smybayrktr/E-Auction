using DnsClient.Internal;
using ESourcing.Sourcing.Entities;
using ESourcing.Sourcing.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ESourcing.Sourcing.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly ILogger <AuctionController> _logger;

        public AuctionController(ILogger<AuctionController> logger, IAuctionRepository auctionRepository)
        {
            _logger = logger;
            _auctionRepository = auctionRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Auction>),(int)HttpStatusCode.OK)]
        //StatusKodu 200 olunca IEnumerable<Auction> türünde nesne bekler.
        public async Task<ActionResult<IEnumerable<Auction>>> GetAuctions()
        {
            var auctions= await _auctionRepository.GetAuctions();
            return Ok(auctions);
        }

        [HttpGet("{id: length(24)}", Name ="GetAuction")]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<Auction>> GetAuction(string id)
        {
            var auction = await _auctionRepository.GetAuction(id);
            if (auction == null)
            {
                _logger.LogError($"Auction with id: { id } hasn't been found in database.");
                return NotFound();
            }
            return Ok(auction);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.Created)]
        public async Task<ActionResult<Auction>> CreateAuction([FromBody]Auction auction)
        {
             await _auctionRepository.Create(auction);
            return CreatedAtRoute("GetAuction", new { id= auction.Id});
            //Gelen Auctionu oluşturup, GetAuction metotuna yönlendirdik. Oluşturulan nesneyi geri döndürdük
        }

        [HttpPut]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Auction>> UpdateAuction([FromBody] Auction auction)
        {
            return Ok(await _auctionRepository.Update(auction));
        }

        [HttpDelete("{id: length(24)}")]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Auction>> UpdateAuction(string id)
        {
            return Ok(await _auctionRepository.Delete(id));
        }


    }
}
