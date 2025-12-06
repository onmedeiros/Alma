namespace Alma.Modules.Auctions.AwardedLots.Models
{
    public class AwardedLotFilters
    {
        public string? Name { get; set; }
        public string? AuctionHouse { get; set; }
        public DateTime? AuctionDateFrom { get; set; }
        public DateTime? AuctionDateTo { get; set; }
    }
}
