using Alma.Core.Entities;

namespace Alma.Modules.Auctions.AwardedLots.Entities
{
    public class AwardedLot : Entity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string AuctionHouse { get; set; }
        public DateTime AuctionDate { get; set; }
        public string? AuctionLink { get; set; }
        public decimal WinningBid { get; set; }
        public decimal WinningFees { get; set; }
    }
}