namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Manages parameter state during flow execution.
    /// </summary>
    public interface IParameterState
    {
        /// <summary>
        /// Gets all parameters.
        /// </summary>
        IReadOnlyDictionary<string, object?> GetAll();

        /// <summary>
        /// Gets a parameter value by name.
        /// </summary>
        object? Get(string name);

        /// <summary>
        /// Sets a parameter value.
        /// </summary>
        void Set(string name, object? value);

        /// <summary>
        /// Checks if a parameter exists.
        /// </summary>
        bool Contains(string name);

        /// <summary>
        /// Removes a parameter.
        /// </summary>
        bool Remove(string name);

        /// <summary>
        /// Clears all parameters.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the count of parameters.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets parameters formatted for template rendering.
        /// </summary>
        Dictionary<string, object?> GetTemplateParameters();
    }
}
