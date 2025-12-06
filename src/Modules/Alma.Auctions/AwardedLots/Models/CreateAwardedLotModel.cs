namespace Alma.Modules.Auctions.AwardedLots.Models
{
    public class CreateAwardedLotModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? AuctionHouse { get; set; }
        public DateTime? AuctionDate { get; set; }
        public string? AuctionLink { get; set; }
        public decimal? WinningBid { get; set; }
        public decimal? WinningFees { get; set; }
    }
}
