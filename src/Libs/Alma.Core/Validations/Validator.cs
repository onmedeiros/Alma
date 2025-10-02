using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Core.Validations
{
    public interface IValidator
    {
        Task<ValidationResult> Validate<T>(T model) where T : class;
    }

    public class Validator : IValidator
    {
        private readonly ILogger<Validator> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Validator(ILogger<Validator> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<ValidationResult> Validate<T>(T model)
            where T : class
        {
            var validator = _serviceProvider.GetService<IValidator<T>>();

            if (validator == null)
            {
                _logger.LogError("Error on trying to validate a Model. No validator found for {Model}.", typeof(T).Name);
                throw new InvalidOperationException($"No validator found for {typeof(T).Name}");
            }

            return await validator.ValidateAsync(model);
        }
    }
}
