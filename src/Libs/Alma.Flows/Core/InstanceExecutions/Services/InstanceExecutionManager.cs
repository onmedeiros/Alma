using Alma.Flows.Core.InstanceExecutions.Entities;
using Alma.Flows.Core.InstanceExecutions.Enums;
using Alma.Flows.Core.InstanceExecutions.Stores;
using Alma.Flows.Core.Instances.Entities;
using Alma.Flows.Enums;
using Alma.Flows.Options;
using Alma.Flows.States;
using Microsoft.Extensions.Logging;
using Alma.Core.Types;

namespace Alma.Flows.Core.InstanceExecutions.Services
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