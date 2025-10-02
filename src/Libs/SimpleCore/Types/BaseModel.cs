using FluentValidation;
using FluentValidation.Results;

namespace SimpleCore.Types
{
    public abstract class BaseModel<T>
        where T: BaseModel<T>
    {
        public bool IsValid { get; protected set; }
        public ValidationResult? ValidationResult { get; protected set; }

        public async Task<ValidationResult> ValidateWith(AbstractValidator<T> validator)
        {
            ValidationResult = await validator.ValidateAsync((T)this);

            IsValid = ValidationResult.IsValid;

            return ValidationResult;
        }
    }
}
