using FluentValidation.Results;

namespace Alma.Core.Types
{
    public class ServiceResult
    {
        /// <summary>
        /// Indicates if the operation was successful.
        /// </summary>
        public bool Succeeded => Status == ServiceResultStatus.Success;

        /// <summary>
        /// Status of the operation.
        /// </summary>
        public ServiceResultStatus Status { get; init; }

        /// <summary>
        /// Additional message related to the status.
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Error result when applicable.
        /// </summary>
        public ICollection<ServiceError>? Errors { get; init; }

        #region Success

        /// <summary>
        /// Creates a success result.
        /// </summary>
        public static ServiceResult Success()
            => new ServiceResult { Status = ServiceResultStatus.Success };

        #endregion

        #region Warning Error

        public static ServiceResult WarningError(string message)
        {
            return new ServiceResult
            {
                Status = ServiceResultStatus.WarningError,
                Message = message
            };
        }

        #endregion

        #region Validation Error

        public static ServiceResult ValidationError(ValidationResult validationResult)
        {
            return new ServiceResult
            {
                Status = ServiceResultStatus.ValidationError,
                Message = "Validation failed.",
                Errors = validationResult.Errors.Select(s => new ServiceError
                {
                    Code = s.ErrorCode,
                    Message = s.ErrorMessage,
                    Data = new Dictionary<string, object>
                    {
                        { "PropertyName", s.PropertyName },
                        { "AttemptedValue", s.AttemptedValue ?? string.Empty }
                    }
                }).ToList()
            };
        }

        #endregion

        #region Operation errors

        public static ServiceResult OperationError(string code, string message)
        {
            return new ServiceResult
            {
                Status = ServiceResultStatus.OperationError,
                Message = message,
                Errors = new List<ServiceError>
                {
                    new ServiceError
                    {
                        Code = code,
                        Message = message
                    }
                }
            };
        }

        #endregion

        #region Server errors

        public static ServiceResult ServerError(string message, Exception? ex = null)
        {
            return new ServiceResult
            {
                Status = ServiceResultStatus.ServerError,
                Message = "Internal server error.",
                Errors = ex != null ? new List<ServiceError>
                {
                    new ServiceError
                    {
                        Message = ex.Message,
                        Data = new Dictionary<string, object>
                        {
                            { "StackTrace", ex.StackTrace ?? string.Empty }
                        }
                    }
                } : null
            };
        }

        #endregion
    }

    public class ServiceResult<T> : ServiceResult
    {
        /// <summary>
        /// The result of the operation.
        /// </summary>
        public T? Data { get; init; }

        /// <summary>
        /// Creates a success result with a value.
        /// </summary>
        public static ServiceResult<T> Success(T? value)
            => new ServiceResult<T> { Status = ServiceResultStatus.Success, Data = value };

        #region WarningErrors

        public static new ServiceResult<T> WarningError(string message)
        {
            return new ServiceResult<T>
            {
                Status = ServiceResultStatus.WarningError,
                Message = message
            };
        }

        #endregion

        #region Validation Errors

        public static new ServiceResult<T> ValidationError(ValidationResult validationResult)
        {
            return new ServiceResult<T>
            {
                Status = ServiceResultStatus.ValidationError,
                Message = "Validation failed.",
                Errors = validationResult.Errors.Select(s => new ServiceError
                {
                    Code = s.ErrorCode,
                    Message = s.ErrorMessage,
                    Data = new Dictionary<string, object>
                    {
                        { "PropertyName", s.PropertyName },
                        { "AttemptedValue", s.AttemptedValue ?? string.Empty }
                    }
                }).ToList()
            };
        }

        public static ServiceResult<T> ValidationError(string code, string message, IDictionary<string, object>? data = null)
        {
            return new ServiceResult<T>
            {
                Status = ServiceResultStatus.ValidationError,
                Message = message,
                Errors = new List<ServiceError>
                {
                    new ServiceError
                    {
                        Code = code,
                        Message = message,
                        Data = data ?? new Dictionary<string, object>()
                    }
                }
            };
        }

        public static ServiceResult<T> ValidationError(IEnumerable<ServiceError> errors)
        {
            return new ServiceResult<T>
            {
                Status = ServiceResultStatus.ValidationError,
                Message = "Validation failed.",
                Errors = errors.ToList()
            };
        }

        #endregion

        #region Operation errors

        public static new ServiceResult<T> OperationError(string code, string message)
        {
            return new ServiceResult<T>
            {
                Status = ServiceResultStatus.OperationError,
                Message = message,
                Errors = new List<ServiceError>
                {
                    new ServiceError
                    {
                        Code = code,
                        Message = message
                    }
                }
            };
        }

        #endregion

        #region Server errors

        public static new ServiceResult<T> ServerError(string message, Exception? ex = null)
        {
            return new ServiceResult<T>
            {
                Status = ServiceResultStatus.ServerError,
                Message = "Internal server error.",
                Errors = ex != null ? new List<ServiceError>
                {
                    new ServiceError
                    {
                        Message = ex.Message,
                        Data = new Dictionary<string, object>
                        {
                            { "StackTrace", ex.StackTrace ?? string.Empty }
                        }
                    }
                } : null
            };
        }

        #endregion
    }
}