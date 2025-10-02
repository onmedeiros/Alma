namespace Alma.Core.Mongo.Exceptions
{
    public class MongoRepositoryException : Exception
    {
        public string Name { get; }
        public string Operation { get; }

        public MongoRepositoryException() : base()
        {
            Name = string.Empty;
            Operation = string.Empty;
        }

        public MongoRepositoryException(string? message) : base(message)
        {
            Name = string.Empty;
            Operation = string.Empty;
        }

        public MongoRepositoryException(string? message, Exception? innerException) : base(message, innerException)
        {
            Name = string.Empty;
            Operation = string.Empty;
        }

        public MongoRepositoryException(string? message, string name, string operation) : base(message)
        {
            Name = name;
            Operation = operation;
        }

        public MongoRepositoryException(string? message, string name, string operation, Exception? innerException) : base(message, innerException)
        {
            Name = name;
            Operation = operation;
        }
    }
}