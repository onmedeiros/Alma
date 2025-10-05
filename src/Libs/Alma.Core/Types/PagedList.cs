namespace Alma.Core.Types
{
    /// <summary>
    /// A generic paged List.
    /// </summary>
    /// <typeparam name="T">Type of list element.</typeparam>
    public class PagedList<T> : List<T>
    {
        /// <summary>
        /// Actual page Index.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total count of elements.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Items in the list.
        /// </summary>
        public IEnumerable<T> Items => this;

        /// <summary>
        /// Creates a new instance of <see cref="PagedListViewModel{T}"/>.
        /// </summary>
        /// <returns>A Paged List View Model object for serialization.</returns>
        public PagedListViewModel<T> ToViewModel()
        {
            return new PagedListViewModel<T>(this);
        }
    }
}