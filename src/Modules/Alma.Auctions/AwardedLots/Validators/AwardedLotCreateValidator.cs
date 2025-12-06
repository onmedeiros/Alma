using Alma.Modules.Auctions.AwardedLots.Models;
using FluentValidation;

namespace Alma.Modules.Auctions.AwardedLots.Validators
{
    public class AwardedLotCreateValidator : AbstractValidator<CreateAwardedLotModel>
    {
        public AwardedLotCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome do lote é obrigatório.");

            RuleFor(x => x.AuctionHouse)
                .NotEmpty()
                .WithMessage("O nome da casa de leilão é obrigatório.");

            RuleFor(x => x.AuctionDate)
                .NotEmpty()
                .WithMessage("A data do leilão é obrigatória.");

            RuleFor(x => x.WinningBid)
                .GreaterThanOrEqualTo(0)
                .WithMessage("O valor do lance vencedor deve ser maior ou igual a zero.");

            RuleFor(x => x.WinningFees)
                .GreaterThanOrEqualTo(0)
                .WithMessage("O valor das taxas deve ser maior ou igual a zero.");
        }
    }
}
