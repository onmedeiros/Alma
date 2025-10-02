using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace SimpleCore.Services
{
    public interface IValidationService
    {
        Task<bool> Validate<T>(T model, out ICollection<ValidationResult> results)
            where T : class;
    }

    public class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger;
        }

        public Task<bool> Validate<T>(T model, out ICollection<ValidationResult> results)
            where T : class
        {
            var context = new ValidationContext(model);

            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(model, context, validationResults, true);

            results = validationResults;

            return Task.FromResult(isValid);
        }
    }
}
