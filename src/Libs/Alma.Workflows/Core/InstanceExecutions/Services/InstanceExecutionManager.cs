using Alma.Workflows.Core.InstanceExecutions.Entities;
using Alma.Workflows.Core.InstanceExecutions.Enums;
using Alma.Workflows.Core.InstanceExecutions.Stores;
using Alma.Workflows.Core.Instances.Entities;
using Alma.Workflows.Enums;
using Alma.Workflows.Options;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;
using Alma.Core.Types;

namespace Alma.Workflows.Core.InstanceExecutions.Services
{
    public interface IInstanceExecutionManager
    {
        ValueTask<InstanceExecution> Begin(FlowInstance instance, ExecutionOptions? options = null);

        ValueTask<InstanceExecution> Update(InstanceExecution instance, ExecutionState state);

        ValueTask<InstanceExecution?> FindById(string id, string discriminator);

        ValueTask<PagedList<InstanceExecution>> List(int page, int pageSize, InstanceExecutionFilters? filters = null);
    }

    public class InstanceExecutionManager : IInstanceExecutionManager
    {
        private readonly ILogger<InstanceExecutionManager> _logger;
        private readonly IInstanceExecutionStore _instanceExecutionStore;

        public InstanceExecutionManager(ILogger<InstanceExecutionManager> logger, IInstanceExecutionStore instanceExecutionStore)
        {
            _logger = logger;
            _instanceExecutionStore = instanceExecutionStore;
        }

        public ValueTask<InstanceExecution> Begin(FlowInstance instance, ExecutionOptions? options = null)
        {
            if (options is null)
            {
                options = new ExecutionOptions
                {
                    ExecutionMode = instance.ExecutionMode
                };
            }

            var execution = new InstanceExecution
            {
                Id = Guid.NewGuid().ToString(),
                Discriminator = instance.Discriminator,
                InstanceId = instance.Id,
                DefinitionVersionId = instance.FlowDefinitionVersionId,
                Options = options,
                Status = InstanceExecutionStatus.Pending
            };

            if (options.Parameters.Count > 0)
            {
                execution.State = new ExecutionState
                {
                    Parameters = options.Parameters
                };
            }

            return _instanceExecutionStore.InsertAsync(execution);
        }

        public ValueTask<InstanceExecution> Update(InstanceExecution instance, ExecutionState state)
        {
            instance.State = state;
            instance.UpdatedAt = DateTime.Now;

            switch (state.GetExecutionStatus())
            {
                case ExecutionStatus.Completed:
                    instance.Status = InstanceExecutionStatus.Completed;
                    break;

                case ExecutionStatus.Failed:
                    instance.Status = InstanceExecutionStatus.Failed;
                    break;

                case ExecutionStatus.Waiting:
                    instance.Status = InstanceExecutionStatus.Waiting;
                    break;
            }

            return _instanceExecutionStore.UpdateAsync(instance);
        }

        public ValueTask<InstanceExecution?> FindById(string id, string discriminator)
        {
            return _instanceExecutionStore.FindByIdAsync(id, discriminator);
        }

        public ValueTask<PagedList<InstanceExecution>> List(int page, int pageSize, InstanceExecutionFilters? filters = null)
        {
            return _instanceExecutionStore.ListAsync(page, pageSize, filters);
        }
    }
}