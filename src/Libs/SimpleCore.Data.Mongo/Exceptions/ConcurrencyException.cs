namespace SimpleCore.Data.Mongo.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public string? EntityName { get; set; }
        public string? EntityIdentifier { get; set; }

        public ConcurrencyException() : base()
        {
        }

        public ConcurrencyException(string? message) : base(message)
        {
        }

        public ConcurrencyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
