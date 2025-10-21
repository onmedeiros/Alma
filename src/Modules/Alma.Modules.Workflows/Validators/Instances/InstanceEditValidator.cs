using Alma.Workflows.Core.Instances.Models;
using FluentValidation;

namespace Alma.Modules.Workflows.Validators.Instances
{
    public class InstanceEditValidator : AbstractValidator<InstanceEditModel>
    {
        public InstanceEditValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
