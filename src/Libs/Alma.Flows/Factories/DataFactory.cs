using Alma.Flows.Core.Activities.Base;

namespace Alma.Flows.Factories
{
    public static class DataFactory
    {
        public static object CreateData(Type valueType)
        {
            var dataType = typeof(Data<>).MakeGenericType(valueType);

            var dataInstance = Activator.CreateInstance(dataType)
                ?? throw new InvalidOperationException($"Failed to create an instance of {dataType}.");

            return dataInstance;
        }
    }
}
