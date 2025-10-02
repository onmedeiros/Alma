using FluentValidation.Results;

namespace SimpleCore.Types
{
    public class ServiceResult
    {
        /// <summary>
        /// Indica se a operação foi bem sucedida.
        /// </summary>
        public bool Succeeded => Status == ServiceResultStatus.Success;

        /// <summary>
        /// Status da operação.
        /// </summary>
        public ServiceResultStatus Status { get; init; }

        /// <summary>
        /// Mensagem complementar referente ao status.
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Resultado da validação quando aplicável.
        /// </summary>
        public ValidationResult? ValidationResult { get; init; }

        /// <summary>
        /// Erros de validação.
        /// </summary>
        public HashSet<KeyValuePair<string, string>>? ValidationErrors { get; init; }

        #region Success

        /// <summary>
        /// Cria um resultado de sucesso.
        /// </summary>
        public static ServiceResult Success()
            => new ServiceResult { Status = ServiceResultStatus.Success };

        #endregion

        #region Invalid

        /// <summary>
        /// Cria um resultado que representa uma requisição inválida.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        public static ServiceResult Invalid(string message)
            => new ServiceResult { Status = ServiceResultStatus.Invalid, Message = message };

        /// <summary>
        /// Cria um resultado que representa uma requisição inválida.
        /// </summary>
        /// <param name="field">Campo inválido.</param>
        /// <param name="message">Mensagem de erro.</param>
        /// <returns></returns>
        public static ServiceResult Invalid(string field, string message)
        {
            var errors = new HashSet<KeyValuePair<string, string>> { new(field, message) };
            return new ServiceResult { Status = ServiceResultStatus.Invalid, Message = message };
        }

        /// <summary>
        /// Cria um resultado que representa uma requisição inválida.
        /// </summary>
        /// <param name="validationResult">Objeto ValidationResult da biblioteca FluentValidation.</param>
        /// <returns></returns>
        public static ServiceResult Invalid(ValidationResult validationResult) =>
            new ServiceResult
            {
                Status = ServiceResultStatus.Invalid,
                ValidationResult = validationResult,
                ValidationErrors = new(validationResult.Errors.Select(e => new KeyValuePair<string, string>(e.PropertyName, e.ErrorMessage)))
            };

        /// <summary>
        /// Cria um resultado que representa uma requisição inválida.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        /// <param name="validationResult">Objeto ValidationResult da biblioteca FluentValidation.</param>
        /// <returns></returns>
        public static ServiceResult Invalid(string message, ValidationResult validationResult) =>
            new ServiceResult
            {
                Status = ServiceResultStatus.Invalid,
                Message = message,
                ValidationResult = validationResult,
                ValidationErrors = new(validationResult.Errors.Select(e => new KeyValuePair<string, string>(e.PropertyName, e.ErrorMessage)))
            };

        #endregion

        #region Fail

        /// <summary>
        /// Cria um resultado que representa uma falha na operação.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        /// <returns></returns>
        public static ServiceResult Fail(string message) => new ServiceResult { Status = ServiceResultStatus.Fail, Message = message };

        #endregion

        #region Not found

        public static ServiceResult NotFound(string message) => new ServiceResult { Status = ServiceResultStatus.NotFound, Message = message };

        #endregion
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; init; }

        #region Success

        /// <summary>
        /// Cria um resultado de sucesso.
        /// </summary>
        /// <typeparam name="T">Tipo de dado retornado com o sucesso.</typeparam>
        /// <param name="data">Dado retornado com o sucesso.</param>
        /// <returns></returns>
        public static ServiceResult<T> Success(T? data)
            => new ServiceResult<T> { Status = ServiceResultStatus.Success, Data = data };

        #endregion

        #region Invalid

        /// <summary>
        /// Cria um resultado que representa uma requisição inválida.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        public new static ServiceResult<T> Invalid(string message)
            => new ServiceResult<T> { Status = ServiceResultStatus.Invalid, Message = message };

        /// <summary>
        /// Cria um resultado que representa uma requisição inválida.
        /// </summary>
        /// <param name="field">Campo inválido.</param>
        /// <param name="message">Mensagem de erro.</param>
        /// <returns></returns>
        public new static ServiceResult<T> Invalid(string field, string message)
        {
            var errors = new HashSet<KeyValuePair<string, string>> { new(field, message) };
            return new ServiceResult<T> { Status = ServiceResultStatus.Invalid, Message = message, ValidationErrors = errors };
        }

        /// <summary>
        /// Cria um resultado que representa uma requisição inválida.
        /// </summary>
        /// <param name="validationResult">Objeto ValidationResult da biblioteca FluentValidation.</param>
        /// <returns></returns>
        public new static ServiceResult<T> Invalid(ValidationResult validationResult) =>
            new ServiceResult<T>
            {
                Status = ServiceResultStatus.Invalid,
                ValidationResult = validationResult,
                ValidationErrors = new(validationResult.Errors.Select(e => new KeyValuePair<string, string>(e.PropertyName, e.ErrorMessage)))
            };

        /// <summary>
        /// Cria um resultado que representa uma requisição inválida.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        /// <param name="validationResult">Objeto ValidationResult da biblioteca FluentValidation.</param>
        /// <returns></returns>
        public new static ServiceResult<T> Invalid(string message, ValidationResult validationResult) =>
            new ServiceResult<T>
            {
                Status = ServiceResultStatus.Invalid,
                Message = message,
                ValidationResult = validationResult,
                ValidationErrors = new(validationResult.Errors.Select(e => new KeyValuePair<string, string>(e.PropertyName, e.ErrorMessage)))
            };

        #endregion

        #region Fail

        /// <summary>
        /// Cria um resultado que representa uma falha na operação.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        /// <returns></returns>
        public new static ServiceResult<T> Fail(string message) =>
            new ServiceResult<T> { Status = ServiceResultStatus.Fail, Message = message };

        #endregion

        #region Not found

        public static new ServiceResult<T> NotFound(string message) =>
            new ServiceResult<T> { Status = ServiceResultStatus.NotFound, Message = message };

        #endregion
    }
}
