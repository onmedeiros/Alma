using Alma.Workflows.Enums;

namespace Alma.Workflows.Models.Activities
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string? Content { get; set; }
    }
}