using Alma.Flows.Core.Instances.Models;
using FluentValidation;

namespace Alma.Modules.Flows.Validators.Instances
{
    public class InstanceEditValidator : AbstractValidator<InstanceEditModel>
    {
        public InstanceEditValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
