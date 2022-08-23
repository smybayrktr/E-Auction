using AutoMapper;
using DnsClient.Internal;
using ESourcing.Sourcing.Entities;
using ESourcing.Sourcing.Repositories.Interface;
using EventBusRabbitMQ.Core;
using EventBusRabbitMQ.Events;
using EventBusRabbitMQ.Producer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly IBidRepository _bidRepository;
        private readonly IMapper _mapper;
        private readonly EventBusRabbitMQProducer _producer;
        private readonly ILogger <AuctionController> _logger;

        public AuctionController(ILogger<AuctionController> logger,
            IAuctionRepository auctionRepository,
            IBidRepository bidRepository,
            EventBusRabbitMQProducer producer,
            IMapper mapper)
        {
            _producer = producer;
            _mapper= mapper;    
            _bidRepository = bidRepository;
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

        [HttpGet("{id:length(24)}", Name ="GetAuction")]
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

        [HttpDelete("{id:length(24)}")]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Auction>> UpdateAuction(string id)
        {
            return Ok(await _auctionRepository.Delete(id));
        }


        [HttpPost("CompleteAuction")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        public async Task<ActionResult> CompleteAuction(string id)
        {
            Auction auction= await _auctionRepository.GetAuction(id);
            if (auction == null)
                return NotFound();
            
            if (auction.Status != (int)Status.Active)
            {
                _logger.LogError("Auction can not be completed");
                return BadRequest();
            }
            Bid bid= await _bidRepository.GetWinnerBid(id);
            if (bid == null)
            {
                return NotFound();
            }
            OrderCreateEvent eventMessage = _mapper.Map<OrderCreateEvent>(bid);
            eventMessage.Quantity=auction.Quantity;
            auction.Status = (int)Status.Closed;
            bool updateResponse = await _auctionRepository.Update(auction);
            if (!updateResponse)
            {
                _logger.LogError("Auction can not updated");
                return BadRequest();
            }
            try
            {
                _producer.Publish(EventBusConstants.OrderCreateQueue, eventMessage);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Error publishing integration event: {EventId} from {AppName}",eventMessage.Id, "Sourcing");
                throw;
            }
            return Accepted();
        }

    }
}
