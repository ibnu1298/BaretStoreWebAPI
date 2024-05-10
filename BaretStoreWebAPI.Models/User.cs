using Microsoft.AspNetCore.Identity;

namespace BaretStoreWebAPI.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public DateTime? OTPcreated { get; set; }
        public Cart? Cart { get; set; }
        public string URLImage { get; set; } = string.Empty;
        public string ProviderId { get; set; } = string.Empty;
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
    public class UserRole : IdentityUserRole<string>
    {
        public virtual User User { get; set; }
        public virtual CustomRole Role { get; set; }
    }

    public class CustomRole : IdentityRole
    {
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
