using System.ComponentModel;

namespace Alma.Flows.Enums
{
    public enum HttpMethod
    {
        [Description("GET")]
        Get,

        [Description("POST")]
        Post,

        [Description("PUT")]
        Put,

        [Description("PATCH")]
        Patch
    }
}
