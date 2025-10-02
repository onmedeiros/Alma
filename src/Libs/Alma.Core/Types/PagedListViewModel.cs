namespace Alma.Core.Types
{
    public class PagedListViewModel<T>
    {
        private readonly PagedList<T> _pagedList;

        /// <summary>
        /// Actual page Index.
        /// </summary>
        public int PageIndex => _pagedList.PageIndex;

        /// <summary>
        /// Page size.
        /// </summary>
        public int PageSize => _pagedList.PageSize;

        /// <summary>
        /// Total count of elements.
        /// </summary>
        public int TotalCount => _pagedList.TotalCount;

        /// <summary>
        /// Items in the list.
        /// </summary>
        public IEnumerable<T> Items => _pagedList.Items;

        public PagedListViewModel(PagedList<T> pagedList)
        {
            _pagedList = pagedList;
        }
    }
}