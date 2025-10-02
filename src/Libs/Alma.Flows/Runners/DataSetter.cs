using Alma.Flows.Core.Abstractions;
using Alma.Flows.Extensions;
using Alma.Flows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Flows.Runners
{
    public interface IDataSetter
    {
        void LoadData(ExecutionState state, IActivity activity);

        void UpdateData(ExecutionState state, IActivity activity);
    }

    public class DataSetter : IDataSetter
    {
        private readonly ILogger<DataSetter> _logger;

        public DataSetter(ILogger<DataSetter> logger)
        {
            _logger = logger;
        }

        public void LoadData(ExecutionState state, IActivity activity)
        {
            foreach (var dataDescriptor in activity.Descriptor.DataProperties)
            {
                if (!state.ActivityData.ContainsKey(activity.Id) || !state.ActivityData[activity.Id].ContainsKey(dataDescriptor.Name))
                    continue;

                var dataValue = state.ActivityData[activity.Id][dataDescriptor.Name];

                if (dataValue is null)
                    continue;

                activity.SetDataValue(dataDescriptor.Name, dataValue);
            }
        }

        public void UpdateData(ExecutionState state, IActivity activity)
        {
            foreach (var dataDescriptor in activity.Descriptor.DataProperties)
            {
                var dataValue = activity.GetDataValue(dataDescriptor.Name);

                if (dataValue is null)
                    continue;

                if (!state.ActivityData.ContainsKey(activity.Id))
                    state.ActivityData.Add(activity.Id, []);

                state.ActivityData[activity.Id].Remove(dataDescriptor.Name);
                state.ActivityData[activity.Id].Add(dataDescriptor.Name, dataValue);
            }
        }
    }
}