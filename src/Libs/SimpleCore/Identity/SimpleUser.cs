using Microsoft.AspNetCore.Identity;

namespace SimpleCore.Identity
{
    public class SimpleUser: IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
