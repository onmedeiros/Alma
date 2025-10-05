namespace Alma.Core.Types
{
    public class SearchModelBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string? OrderBy { get; set; }
        public string? Term { get; set; }
    }
}