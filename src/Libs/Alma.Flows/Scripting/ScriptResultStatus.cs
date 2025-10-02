namespace Alma.Flows.Scripting
{
    public enum ScriptResultStatus
    {
        /// <summary>
        /// The script executed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The script execution failed.
        /// </summary>
        Failure = 1,

        /// <summary>
        /// The script execution was canceled.
        /// </summary>
        Canceled = 2,

        /// <summary>
        /// The script execution timed out.
        /// </summary>
        Timeout = 3
    }
}