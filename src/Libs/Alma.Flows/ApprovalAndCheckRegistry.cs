using Alma.Flows.Core.Description.Describers;
using Alma.Flows.Core.Description.Descriptors;
using Alma.Flows.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alma.Flows
{
    public interface IApprovalAndCheckRegistry
    {
        IEnumerable<ApprovalAndCheckDescriptor> ApprovalAndChecks { get; }
        void RegisterApprovalAndCheck(Type type);
        ApprovalAndCheckDescriptor GetApprovalAndCheckDescriptor(Type type);
        ApprovalAndCheckDescriptor GetApprovalAndCheckDescriptor(string fullName);
        IEnumerable<ApprovalAndCheckDescriptor> List(string term);
    }

    internal class ApprovalAndCheckRegistry : IApprovalAndCheckRegistry
    {
        private readonly ILogger<ApprovalAndCheckRegistry> _logger;
        private readonly FlowOptions _options;

        private readonly ICollection<ApprovalAndCheckDescriptor> _approvalAndChecksDescriptors = [];

        public IEnumerable<ApprovalAndCheckDescriptor> ApprovalAndChecks => _approvalAndChecksDescriptors;

        public ApprovalAndCheckRegistry(ILogger<ApprovalAndCheckRegistry> logger, IOptions<FlowOptions> options)
        {
            _logger = logger;
            _options = options.Value;

            foreach (var type in _options.ApprovalAndChecksTypes)
                RegisterApprovalAndCheck(type);
        }

        public ApprovalAndCheckDescriptor GetApprovalAndCheckDescriptor(Type type)
        {
            return _approvalAndChecksDescriptors.FirstOrDefault(x => x.Type == type)
                ?? throw new Exception($"Descriptor for {type.FullName} not found.");
        }

        public ApprovalAndCheckDescriptor GetApprovalAndCheckDescriptor(string fullName)
        {
            return _approvalAndChecksDescriptors.FirstOrDefault(x => x.FullName == fullName)
                ?? throw new Exception($"Descriptor for {fullName} not found.");
        }

        public void RegisterApprovalAndCheck(Type type)
        {
            _approvalAndChecksDescriptors.Add(ApprovalAndCheckDescriber.Describe(type));
        }

        public IEnumerable<ApprovalAndCheckDescriptor> List(string term)
        {
            var query = _approvalAndChecksDescriptors.AsQueryable();

            if (!string.IsNullOrEmpty(term))
                query = query.Where(x => x.DisplayName.Contains(term) || x.FullName.Contains(term));

            return query.OrderBy(x => x.DisplayName);
        }
    }
}
