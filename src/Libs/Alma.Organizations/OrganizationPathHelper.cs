namespace Alma.Organizations
{
    public static class OrganizationPathHelper
    {
        private static readonly string[] invalidPaths = new string[]
        {
            "_content", "_framework"
        };

        public static bool IsValidSubdomain(string subdomain)
        {
            return !invalidPaths.Contains(subdomain);
        }
    }
}