using DotLiquid;

namespace Alma.Workflows.Models.Activities
{
    public class HttpRequestActivityResponse : ILiquidizable
    {
        public int StatusCode { get; set; }
        public int EllapsedMilliseconds { get; set; }
        public string Content { get; set; } = string.Empty;
        public object? Body { get; set; }

        public object ToLiquid()
        {
            return new Dictionary<string, object?>
            {
                { "StatusCode", StatusCode },
                { "Content", Content },
                { "Body", Body }
            };
        }
    }
}