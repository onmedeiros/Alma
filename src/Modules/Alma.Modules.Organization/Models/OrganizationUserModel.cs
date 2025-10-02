using Alma.Blazor.Entities;
using Alma.Organizations.Entities;

namespace Alma.Modules.Organizations.Models
{
    public class OrganizationUserModel
    {
        public DateTime AssociationCreatedAt { get; set; }
        public DateTime? AssociationModifiedAt { get; set; }
        public string AssociationId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }

        public OrganizationUserModel(OrganizationUser association, AlmaUser user)
        {
            AssociationCreatedAt = association.CreatedAt;
            AssociationModifiedAt = association.ModifiedAt;
            AssociationId = association.Id;
            UserId = association.UserId;
            UserName = user.UserName ?? string.Empty;
            EmailConfirmed = user.EmailConfirmed;
            PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            TwoFactorEnabled = user.TwoFactorEnabled;
        }
    }
}