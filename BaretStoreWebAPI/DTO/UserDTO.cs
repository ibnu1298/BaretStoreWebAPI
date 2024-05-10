using System.ComponentModel.DataAnnotations;

namespace BaretStoreWebAPI.DTO
{
    public class UserDTO : BaseResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public bool PasswordExist { get; set; }
    }
    public class RoleNameDTO
    {
        public string RoleName { get; set; } = string.Empty;
    }

    public class UsernameOrEmailDTO
    {
        [Required]
        public string UsernameOrEmail { get; set; } = String.Empty;
    }
    public class UpdateImageDTO
    {
        [Required]
        public string Id { get; set; } = String.Empty;
        [Required]
        public string ImageURL { get; set; } = String.Empty;
    }
    public class UpdateUserDTO
    {
        [Required]
        public string Id { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
    }
    public class UpdateCredentialDTO
    {
        [Required]
        public string Id { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string UserName { get; set; } = String.Empty;
        public string OldPassword { get; set; } = String.Empty;
        public string NewPassword { get; set; } = String.Empty;
        public bool ChangePassword { get; set; }
    }
    public class CreateUserDTO
    {
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    public class UpdateUsernameDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class UpdateEmailDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class CreateRoleDTO
    {
        public string RoleName { get; set; }
    }
}
