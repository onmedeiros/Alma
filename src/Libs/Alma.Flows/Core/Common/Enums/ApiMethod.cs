using System.ComponentModel;

namespace Alma.Flows.Core.Common.Enums
{
    /// <summary>
    /// An enumeration representing the HTTP methods used in API requests.
    /// </summary>
    public enum ApiMethod
    {
        /// <summary>
        /// Represents an HTTP GET request.
        /// </summary>
        [Description("GET")]
        Get,

        /// <summary>
        /// Represents an HTTP POST request.
        /// </summary>
        [Description("POST")]
        Post,

        /// <summary>
        /// Represents an HTTP PUT request.
        /// </summary>
        [Description("PUT")]
        Put,

        /// <summary>
        /// Represents an HTTP DELETE request.
        /// </summary>
        [Description("DELETE")]
        Delete
    }
}