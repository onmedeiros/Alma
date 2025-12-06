using Alma.Core.Data;
using Alma.Core.Types;
using Alma.Modules.Auctions.AwardedLots.Entities;
using Alma.Modules.Auctions.AwardedLots.Models;
using Microsoft.Extensions.Logging;

namespace Alma.Modules.Auctions.AwardedLots.Services
{
    public interface IAwardedLotService
    {
        ValueTask<AwardedLot> CreateAsync(CreateAwardedLotModel model);
        ValueTask<AwardedLot> UpdateAsync(EditAwardedLotModel model);
        ValueTask<AwardedLot?> FindByIdAsync(string id);
        ValueTask<AwardedLot?> DeleteAsync(string id);
        ValueTask<PagedList<AwardedLot>> ListAsync(int page, int pageSize, AwardedLotFilters? filters = null);
    }

    public class AwardedLotService : IAwardedLotService
    {
        private readonly ILogger<AwardedLotService> _logger;
        private readonly IRepository<AwardedLot> _repository;

        public AwardedLotService(ILogger<AwardedLotService> logger, IRepository<AwardedLot> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async ValueTask<AwardedLot> CreateAsync(CreateAwardedLotModel model)
        {
            _logger.LogDebug("Creating awarded lot with name {Name}.", model.Name);

            var now = DateTime.UtcNow;

            var awardedLot = new AwardedLot
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = now,
                ModifiedAt = now,
                Deleted = false,
                Name = model.Name!,
                Description = model.Description,
                AuctionHouse = model.AuctionHouse!,
                AuctionDate = model.AuctionDate!.Value,
                AuctionLink = model.AuctionLink,
                WinningBid = model.WinningBid ?? 0,
                WinningFees = model.WinningFees ?? 0
            };

            await _repository.InsertAsync(awardedLot);

            _logger.LogDebug("Awarded lot with name {Name} created with id {Id}.", model.Name, awardedLot.Id);

            return awardedLot;
        }

        public async ValueTask<AwardedLot> UpdateAsync(EditAwardedLotModel model)
        {
            _logger.LogDebug("Updating awarded lot with id {Id}.", model.Id);

            var entity = await _repository.GetByIdAsync(model.Id);

            if (entity == null)
            {
                _logger.LogError("Awarded lot with id {Id} not found.", model.Id);
                throw new InvalidOperationException($"Lote arrematado com id {model.Id} não encontrado.");
            }

            if (!string.IsNullOrEmpty(model.Name))
                entity.Name = model.Name;

            if (model.Description != null)
                entity.Description = model.Description;

            if (!string.IsNullOrEmpty(model.AuctionHouse))
                entity.AuctionHouse = model.AuctionHouse;

            if (model.AuctionDate.HasValue)
                entity.AuctionDate = model.AuctionDate.Value;

            if (model.AuctionLink != null)
                entity.AuctionLink = model.AuctionLink;

            if (model.WinningBid.HasValue)
                entity.WinningBid = model.WinningBid.Value;

            if (model.WinningFees.HasValue)
                entity.WinningFees = model.WinningFees.Value;

            await _repository.UpdateAsync(entity);

            _logger.LogDebug("Awarded lot with id {Id} updated.", model.Id);

            return entity;
        }

        public async ValueTask<AwardedLot?> DeleteAsync(string id)
        {
            _logger.LogDebug("Deleting awarded lot with id {Id}.", id);

            var entity = await _repository.DeleteAsync(id, hard: false);

            if (entity == null)
            {
                _logger.LogError("Awarded lot with id {Id} not found.", id);
                throw new InvalidOperationException($"Lote arrematado com id {id} não encontrado.");
            }

            _logger.LogDebug("Awarded lot with id {Id} deleted.", id);

            return entity;
        }

        public async ValueTask<AwardedLot?> FindByIdAsync(string id)
        {
            _logger.LogDebug("Finding awarded lot with id {Id}.", id);
            return await _repository.GetByIdAsync(id);
        }

        public async ValueTask<PagedList<AwardedLot>> ListAsync(int page, int pageSize, AwardedLotFilters? filters = null)
        {
            _logger.LogDebug("Listing awarded lots.");

            var query = _repository.AsQueryable();

            if (filters is not null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Name))
                {
                    query = query.Where(x => x.Name.Contains(filters.Name));
                }

                if (!string.IsNullOrWhiteSpace(filters.AuctionHouse))
                {
                    query = query.Where(x => x.AuctionHouse.Contains(filters.AuctionHouse));
                }

                if (filters.AuctionDateFrom.HasValue)
                {
                    query = query.Where(x => x.AuctionDate >= filters.AuctionDateFrom.Value);
                }

                if (filters.AuctionDateTo.HasValue)
                {
                    query = query.Where(x => x.AuctionDate <= filters.AuctionDateTo.Value);
                }
            }

            // Order by auction date descending
            query = query.OrderByDescending(x => x.AuctionDate);

            return await _repository.GetPagedAsync(page, pageSize, query);
        }
    }
}
