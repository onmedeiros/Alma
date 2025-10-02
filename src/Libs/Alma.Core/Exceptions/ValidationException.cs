using System.ComponentModel.DataAnnotations;

namespace Alma.Core.Exceptions
{
    public class ValidationException : Exception
    {
        public Dictionary<string, string>? Properties { get; set; }

        public ICollection<ValidationResult>? ValidationResults { get; set; }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {

        }

        public ValidationException(string message, ICollection<ValidationResult> results) : base(message)
        {
            ValidationResults = results;
        }

        public ValidationException(ICollection<ValidationResult> results) : base("Validation failed.")
        {
            ValidationResults = results;
        }

        public ValidationException(string message, params KeyValuePair<string, string>[] properties) : base(message)
        {
            Properties = properties.ToDictionary();
        }

        public ValidationException(string message, Exception innerException, params KeyValuePair<string, string>[] properties) : base(message, innerException)
        {
            Properties = properties.ToDictionary();
        }

    }
}
