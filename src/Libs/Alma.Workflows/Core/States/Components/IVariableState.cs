using Alma.Workflows.Models;

using Alma.Workflows.Core.ApprovalsAndChecks.Models;

namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Manages variable state during flow execution.
    /// </summary>
    public interface IVariableState
    {
        /// <summary>
        /// Gets all variables.
        /// </summary>
        IReadOnlyDictionary<string, ValueObject> GetAll();

        /// <summary>
        /// Gets a variable value by name.
        /// </summary>
        ValueObject? Get(string name);

        /// <summary>
        /// Sets a variable value.
        /// </summary>
        void Set(string name, object? value);

        /// <summary>
        /// Checks if a variable exists.
        /// </summary>
        bool Contains(string name);

        /// <summary>
        /// Removes a variable.
        /// </summary>
        bool Remove(string name);

        /// <summary>
        /// Clears all variables.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the count of variables.
        /// </summary>
        int Count { get; }
    }
}
