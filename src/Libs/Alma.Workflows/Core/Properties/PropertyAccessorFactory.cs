using Alma.Workflows.Core.Abstractions;

namespace Alma.Workflows.Core.Properties
{
    /// <summary>
    /// Factory for creating property accessors.
    /// Provides a centralized way to obtain accessor instances.
    /// </summary>
    public class PropertyAccessorFactory
    {
        private readonly Lazy<ParameterAccessor> _parameterAccessor;
        private readonly Lazy<DataAccessor> _dataAccessor;
        private readonly Lazy<PortAccessor> _portAccessor;

        public PropertyAccessorFactory()
        {
            _parameterAccessor = new Lazy<ParameterAccessor>(() => new ParameterAccessor());
            _dataAccessor = new Lazy<DataAccessor>(() => new DataAccessor());
            _portAccessor = new Lazy<PortAccessor>(() => new PortAccessor());
        }

        /// <summary>
        /// Gets the accessor for Parameter properties.
        /// </summary>
        public ParameterAccessor Parameters => _parameterAccessor.Value;

        /// <summary>
        /// Gets the accessor for Data properties.
        /// </summary>
        public DataAccessor Data => _dataAccessor.Value;

        /// <summary>
        /// Gets the accessor for Port properties.
        /// </summary>
        public PortAccessor Ports => _portAccessor.Value;

        /// <summary>
        /// Creates a new instance of PropertyAccessorFactory.
        /// </summary>
        public static PropertyAccessorFactory Create()
        {
            return new PropertyAccessorFactory();
        }
    }
}
