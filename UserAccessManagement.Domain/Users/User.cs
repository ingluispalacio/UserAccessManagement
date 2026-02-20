using UserAccessManagement.Domain.Common;
using UserAccessManagement.Domain.Users.ValueObjects;

namespace UserAccessManagement.Domain.Users
{
    public class User : BaseEntity
    {
        public string Name { get; private set; } = null!;
        public string Lastname { get; private set; } = null!;
        public Email Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public string? Address { get; private set; }
        public bool IsActive { get; private set; }
        private User() { }
        public User(string name, string lastname, string address, Email email, string passwordHash) 
        { 
            Name = name;
            Lastname = lastname;
            Address = address;
            Email = email;
            PasswordHash = passwordHash;
            IsActive = true;
        }
        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
            MarkAsUpdated();
        }
        public void SoftDelete()
        {
            MarkAsDeleted();
            IsActive = false;
            MarkAsUpdated();
        }

        public void Update(
            string name,
            string lastname,
            string address,
            Email email)
        {
            Name = name;
            Lastname = lastname;
            Address = address;
            Email = email;

            MarkAsUpdated();
        }
        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            MarkAsUpdated();
        }


    }
}
