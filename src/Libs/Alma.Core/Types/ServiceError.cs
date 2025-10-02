namespace Alma.Core.Types
{
    public class ServiceError
    {
        /// <summary>
        /// Error message.
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Error code.
        /// </summary>
        public string? Code { get; init; }

        /// <summary>
        /// Additional data related to the error.
        /// </summary>
        public IDictionary<string, object>? Data { get; init; }
    }
}