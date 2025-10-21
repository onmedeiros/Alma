using System.ComponentModel;

namespace Alma.Workflows.Enums
{
    public enum HttpContentType
    {
        [Description("json")]
        Json,
        [Description("x-www-form-urlencoded")]
        FormUrlEncoded,
        [Description("xml")]
        Xml,
        [Description("multipart/form-data")]
        MultipartFormData
    }
}
