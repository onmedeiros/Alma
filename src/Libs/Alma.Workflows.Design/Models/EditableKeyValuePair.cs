namespace Alma.Workflows.Design.Models
{
    public class EditableKeyValuePair<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}
