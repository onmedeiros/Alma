using Alma.Workflows.Core.Abstractions;

namespace Alma.Workflows.Core.Activities.Visitors
{
    /// <summary>
    /// Visitor pattern interface for performing operations on activities.
    /// Allows adding new operations without modifying activity classes.
    /// </summary>
    public interface IActivityVisitor
    {
        /// <summary>
        /// Visits an activity and performs an operation.
        /// </summary>
        /// <param name="activity">The activity to visit.</param>
        void Visit(IActivity activity);
    }

    /// <summary>
    /// Generic visitor pattern interface with return type.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the visit operation.</typeparam>
    public interface IActivityVisitor<TResult>
    {
        /// <summary>
        /// Visits an activity and returns a result.
        /// </summary>
        /// <param name="activity">The activity to visit.</param>
        /// <returns>The result of the visit operation.</returns>
        TResult Visit(IActivity activity);
    }

    /// <summary>
    /// Async visitor pattern interface.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the visit operation.</typeparam>
    public interface IActivityVisitorAsync<TResult>
    {
        /// <summary>
        /// Visits an activity asynchronously and returns a result.
        /// </summary>
        /// <param name="activity">The activity to visit.</param>
        /// <returns>A task representing the async operation with the result.</returns>
        Task<TResult> VisitAsync(IActivity activity);
    }
}
