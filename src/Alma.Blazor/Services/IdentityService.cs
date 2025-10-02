using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenIddict.Abstractions;
using OpenIddict.MongoDb;
using Alma.Core.Types;
using Alma.Core.Validations;
using System.Security.Cryptography;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Alma.Blazor.Models;
using Alma.Blazor.Entities;

namespace Alma.Blazor.Services
{
    public interface IIdentityService
    {
        ValueTask<ServiceResult<CreateApplicationResult>> Create(CreateApplicationModel model);

        ValueTask<ICollection<AlmaApplication>> List(string organizationId);

        ValueTask<ServiceResult> Delete(AlmaApplication application);
    }

    public class IdentityService : IIdentityService
    {
        private readonly ILogger<IdentityService> _logger;
        private readonly OpenIddictMongoDbOptions _options;
        private readonly IMongoDatabase _context;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IValidator _validator;

        public IdentityService(ILogger<IdentityService> logger, IOptions<OpenIddictMongoDbOptions> options, IMongoDatabase context, IOpenIddictApplicationManager applicationManager, IValidator validator)
        {
            _logger = logger;
            _options = options.Value;
            _context = context;
            _applicationManager = applicationManager;
            _validator = validator;
        }

        public async ValueTask<ServiceResult<CreateApplicationResult>> Create(CreateApplicationModel model)
        {
            #region Validação

            // Realiza as demais validações
            var validationResult = await _validator.Validate(model);

            if (!validationResult.IsValid)
                return ServiceResult<CreateApplicationResult>.ValidationError(validationResult);

            #endregion

            // Cria a aplicação
            var secret = GenerateRandomClientSecret().Replace("+", "");
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientSecret = secret,
                DisplayName = model.Name,
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.ClientCredentials
                }
            };

            var applicationObject = await _applicationManager.CreateAsync(descriptor);
            var application = applicationObject as AlmaApplication;

            if (application is null)
            {
                _logger.LogError("Error on creating application.");
                return ServiceResult<CreateApplicationResult>.OperationError("", "Erro ao criar a aplicação.");
            }

            application.OrganizationId = model.OrganizationId;
            await _applicationManager.UpdateAsync(application);

            var result = new CreateApplicationResult
            {
                ClientSecret = secret,
                Application = application
            };

            return ServiceResult<CreateApplicationResult>.Success(result);
        }

        public async ValueTask<ServiceResult> Delete(AlmaApplication application)
        {
            await _applicationManager.DeleteAsync(application);
            return ServiceResult.Success();
        }

        public async ValueTask<ICollection<AlmaApplication>> List(string organizationId)
        {
            var collection = _context.GetCollection<AlmaApplication>(_options.ApplicationsCollectionName);
            return await collection.Find(x => x.OrganizationId == organizationId).ToListAsync();
        }

        private string GenerateRandomClientSecret()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}