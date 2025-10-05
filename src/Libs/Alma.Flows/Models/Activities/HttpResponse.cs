using Alma.Flows.Enums;

namespace Alma.Flows.Models.Activities
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string? Content { get; set; }
    }
}