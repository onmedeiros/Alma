using System.ComponentModel;

namespace Alma.Workflows.Enums
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
