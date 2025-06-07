using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;
using BuildingBlocks.Enums;

namespace UserManagement.API.Infrastructure.Data.Models
{
    public class User : EntityWithDomainEvents<Guid>, IAggregateRoot, ISoftDelete
    {
        public Guid IdentityId { get; set; }
        public MasterIdentity MasterIdentity { get; set; } = default!;

        public string? Name { get; set; } = default!;
        public string? Family { get; set; } = default!;
        public string? NationalCode { get; set; } = default!;
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }

        public virtual ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

        public void Deactivate()
        {
            MasterIdentity.IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
        public void Activate()
        {
            MasterIdentity.IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
        public void UpdateProfile(string name, string family, string? nationalCode, Gender? gender, DateTime birthDate)
        {
            Name = name;
            Family = family;
            NationalCode = nationalCode;
            Gender = gender;
            BirthDate = birthDate;
        }


    }

}
