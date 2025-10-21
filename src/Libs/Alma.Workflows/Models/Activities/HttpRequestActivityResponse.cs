using DotLiquid;

namespace Alma.Workflows.Models.Activities
{
    public class HttpRequestActivityResponse : ILiquidizable
    {
        public int StatusCode { get; set; }
        public string Content { get; set; } = string.Empty;
        public object? Body { get; set; } = string.Empty;

        public object ToLiquid()
        {
            var dictionary = new Dictionary<string, object>
            {
                { "StatusCode", StatusCode },
                { "Content", Content },
                { "Body", Body }
            };

            return dictionary;
        }
    }
}
