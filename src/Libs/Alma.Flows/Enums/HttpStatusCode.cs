using System.ComponentModel;

namespace Alma.Flows.Enums
{
    public enum HttpStatusCode
    {
        [Description("OK (200)")]
        Ok = 200,

        [Description("Created (201)")]
        Created = 201,

        [Description("No Content (204)")]
        NoContent = 204,

        [Description("Bad Request (400)")]
        BadRequest = 400,

        [Description("Unauthorized (401)")]
        Unauthorized = 401,

        [Description("Forbidden (403)")]
        Forbidden = 403,

        [Description("Not Found (404)")]
        NotFound = 404
    }
}