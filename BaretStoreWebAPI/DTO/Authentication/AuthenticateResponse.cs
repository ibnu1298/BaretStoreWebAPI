
using BaretStoreWebAPI.DTO;
using BaretStoreWebAPI.Models;
using System.Globalization;

namespace BaretStoreWebAPI.Authentication
{
    public class AuthenticateResponse : BaseResponse
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string ImageURL { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PasswordExist { get; set; }
        public string Token { get; set; }


        public AuthenticateResponse(User user, string token)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            UserId = user.Id;
            Name = ti.ToTitleCase(user.Name.ToLower());
            PhoneNumber = user.PhoneNumber;
            Username = user.UserName;
            ImageURL = user.URLImage != null? user.URLImage:"";
            EmailConfirmed = user.EmailConfirmed;
            PasswordExist = user.PasswordHash != null ? true : false;
            if (token != null) { Token = token; }

        }

    }
    public class TokenDTO:BaseResponse
    {
        public string Token { get; set;}
    }
   
}
