using System.ComponentModel.DataAnnotations;

namespace BaretStoreWebAPI.Authentication
{
    public class AuthenticateRequest
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string URLImage { get; set; } = string.Empty;
        public string ProviderId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
