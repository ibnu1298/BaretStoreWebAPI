
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using BaretStoreWebAPI.Helpers;
using BaretStoreWebAPI.DTO;
using BaretStoreWebAPI.Models;
using BaretStoreWebAPI;
using BaretStoreWebAPI.Authentication;

namespace EbookWebAPI.DAL
{
    public interface IUser
    {
        Task<IEnumerable<User>> GetAll();
        Task<User> GetById(string id);
        Task<User> Update(UpdateUserDTO obj);
        Task<User> UpdateCredential(UpdateCredentialDTO obj);
        Task<AuthenticateResponse> Authenticate(User obj);
        Task<User> Registration(CreateUserDTO user);
        Task<User> GetByEmailOrUser(string emailOrUsername);
        Task<BaseResponse> CheckUsername(string emailOrUsername);
        Task<BaseResponse> CheckEmail(string emailOrUsername);
        Task<BaseResponse> VerifyOTP(verifikasiOTPDTO obj, int expireMinutes, string type);
        Task<string> GenerateCustomerNo();
        Task<string> GenerateOTP(string usernameOrEmail, bool typeNumberOTP);
        Task<BaseResponse> SendEmailAsync(SendEmailDTO obj);
        Task<User> GetByEmailProvider(string email, string providerId);
    }
    public class UserDAL : IUser
    {
        private UserManager<User> _userManager;
        private readonly DataContext _context;
        private readonly RoleManager<CustomRole> _roleManager;
        private AppSettings _appSettings;
        private string generateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("Id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(1),//hanya berlaku 1 Hari
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public UserDAL(DataContext context, IOptions<AppSettings> appSettings, UserManager<User> userManager, RoleManager<CustomRole> roleManager)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<User> GetById(string id)
        {
            try
            {
                var result = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<User> Registration(CreateUserDTO user)
        {
            try
            {
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                var generateCustNo = await GenerateCustomerNo();
                var results = await _context.Users.OrderBy(c => c.Name).ToListAsync();
               

                var newUser = new User
                {
                    UserName = user.Email,
                    Email = user.Email.ToLower(),
                    Name = ti.ToTitleCase(user.Name.ToLower()),
                   
                    PhoneNumber = user.PhoneNumber
                };
                #region Validate Username & Email
                var username = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == user.UserName.ToUpper());
                if (username != null) throw new Exception($"Username {user.UserName} sudah digunakan");
                if (!string.IsNullOrEmpty(user.Email))
                {
                    var email = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == user.Email.ToUpper());
                    if (email != null) throw new Exception($"Email {user.Email} sudah digunakan");
                }
                #endregion

                var result = await _userManager.CreateAsync(newUser, user.Password);
                if (!result.Succeeded)
                {
                    StringBuilder sb = new StringBuilder();
                    var errors = result.Errors;
                    foreach (var error in errors)
                    {
                        sb.Append($"{error.Code} - {error.Description} \n");
                    }
                    throw new Exception($"Error: {sb.ToString()}");
                }

                return newUser;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }

       
        public async Task<User> Update(UpdateUserDTO obj)
        {
            try
            {
                var update = await _context.Users.FirstOrDefaultAsync(s => s.Id == obj.Id);
                if (update == null) throw new Exception($"Data dengan ID = {obj.Id} Tidak ditemukan");
                if (!string.IsNullOrEmpty(obj.Name)) update.Name = obj.Name;
                if (!string.IsNullOrEmpty(obj.PhoneNumber))
                {
                    update.PhoneNumber = obj.PhoneNumber;
                    update.PhoneNumberConfirmed = false;
                }
                await _context.SaveChangesAsync();
                update = await _context.Users.FirstOrDefaultAsync(s => s.Id == obj.Id);
                if (update == null) throw new Exception($"Data dengan ID = {obj.Id} Tidak ditemukan");
                return update;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task<User> UpdateCredential(UpdateCredentialDTO obj)
        {
            try
            {
                var update = await _context.Users.FirstOrDefaultAsync(x => x.Id == obj.Id);
                if (update == null) throw new Exception($"Data dengan ID = {obj.Id} Tidak ditemukan");
                var checkEmail = await CheckEmail(obj.Email);
                var checkUsername = await CheckUsername(obj.UserName);
                if (!string.IsNullOrEmpty(obj.Email) && checkEmail.IsSucceeded)
                {
                    update.EmailConfirmed = false;
                    update.ProviderId = null;
                    update.Email = obj.Email;
                    update.NormalizedEmail = obj.Email.ToUpper();
                }
                if (!string.IsNullOrEmpty(obj.UserName) && checkUsername.IsSucceeded)
                {
                    update.UserName = obj.UserName;
                    update.NormalizedUserName = obj.UserName.ToUpper();
                }
                if (string.IsNullOrEmpty(update.PasswordHash) && !string.IsNullOrEmpty(obj.OldPassword))
                {
                    var changePass = await _userManager.AddPasswordAsync(update, obj.OldPassword);

                    if (!changePass.Succeeded)
                    {
                        StringBuilder sb = new StringBuilder();
                        var errors = changePass.Errors;
                        foreach (var error in errors)
                        {
                            sb.Append($"{error.Code} - {error.Description} \n");
                        }
                        throw new Exception($"Error: {sb.ToString()}");
                    }
                }
                await _context.SaveChangesAsync();
                update = await _context.Users.FirstOrDefaultAsync(s => s.Id == obj.Id);
                if (update == null) throw new Exception($"Data dengan ID = {obj.Id} Tidak ditemukan");
                return update;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            var results = await _context.Users.OrderBy(c => c.Name).ToListAsync();
            return results;
        }
        public async Task<BaseResponse> CheckUsername(string emailOrUsername)
        {
            try
            {
                BaseResponse baseResponse = new BaseResponse();
                var result = await GetByEmailOrUser(emailOrUsername);
                if (result != null)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "Username Already Taken";
                    return baseResponse;
                }
                if (emailOrUsername.Contains(" "))
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "Tidak Boleh pakai Spasi";
                    return baseResponse;
                }
                baseResponse.IsSucceeded = true;
                baseResponse.Message = "Username Avaiable";
                return baseResponse;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<BaseResponse> CheckEmail(string emailOrUsername)
        {
            try
            {
                BaseResponse baseResponse = new BaseResponse();
                var result = await GetByEmailOrUser(emailOrUsername);
                if (result != null)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "Email Already Taken";
                    return baseResponse;
                }
                if (!emailOrUsername.Contains("@") || !emailOrUsername.Contains("."))
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "Incorrect Email Format";
                    return baseResponse;
                }
                baseResponse.IsSucceeded = true;
                baseResponse.Message = "Email Avaiable";
                return baseResponse;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<User> GetByEmailProvider(string email, string providerId)
        {
            try
            {
                var result = await _userManager.Users.Include(u => u.UserRoles).ThenInclude(u => u.Role).FirstOrDefaultAsync(u => u.ProviderId == providerId && u.Email == email);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<User> GetByEmailOrUser(string data)
        {
            try
            {
                var result = await _userManager.Users.Include(u => u.UserRoles).ThenInclude(u => u.Role).FirstOrDefaultAsync(u => u.Email.ToLower() == data.ToLower() || u.UserName == data || u.Id == data);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<AuthenticateResponse> Authenticate(User obj)
        {

            var getUser = await GetByEmailOrUser(obj.Email);
            var role = getUser.UserRoles.FirstOrDefault();
            JwtSecurityToken generateToken = new JwtSecurityToken(
                claims: new[] {
                    new Claim(JwtRegisteredClaimNames.Email, obj.Email),
                    new Claim("username", obj.UserName),
                    new Claim("role", $"{role.Role.Name}"),
                },
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: new SigningCredentials(
                    key: new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Secret)),
                    algorithm: SecurityAlgorithms.HmacSha256
                )
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(generateToken);
            var user = new AuthenticateResponse(obj, token);
            user.Message = "Yey Berhasil Login";
            user.IsSucceeded = true;

            return user;

        }
        public async Task<IEnumerable<User>> GetByName(string name)
        {
            var results = await _context.Users.Where(c => c.Name.Contains(name) ).OrderBy(c => c.Name).ToListAsync();
            return results;
        }
   

        public async Task<string> GenerateCustomerNo()
        {
            Random res = new Random();
            String num = "1234567890";
            String ran = "";
            int size = 9;

            for (int i = 0; i < size; i++)
            {
                int x = res.Next(10);
                ran = ran + num[x];
            }
            string result = "007" + ran;
            return result;
        }
        public async Task<string> GenerateOTP(string usernameOrEmail, bool typeNumberOTP)
        {
            Random res = new Random();
            try
            {
                var result = await _userManager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == usernameOrEmail.ToUpper() || x.NormalizedEmail == usernameOrEmail.ToUpper());
                if (result == null) throw new Exception($"Error: User tidak ditemukan");
                #region Generate OTP
                String str = "abcdefghijklmnopqrstuvwxyz";
                String num = "1234567890";
                str = $"{str.ToUpper()}{num}{str}";
                int size = 6;
                String ran = "";
                if (typeNumberOTP)
                {
                    for (int i = 0; i < size; i++)
                    {
                        int x = res.Next(10);
                        ran = ran + num[x];
                    }
                }
                else
                {
                    for (int i = 0; i < size; i++)
                    {
                        int x = res.Next(50);
                        ran = ran + str[x];
                    }
                }
                result.OTP = ran;
                result.OTPcreated = DateTime.Now;
                if (ran.Length != size)
                {
                    var getOTPagain = await GenerateOTP(result.Email, true);
                    result.OTP = getOTPagain;
                    result.OTPcreated = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return ran;
                }
                await _context.SaveChangesAsync();
                return ran;
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<BaseResponse> VerifyOTP(verifikasiOTPDTO obj, int expireMinutes, string type)
        {
            try
            {
                BaseResponse result = new BaseResponse();
                var getOTP = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == obj.usernameOrEmail || x.UserName == obj.usernameOrEmail);
                if (getOTP != null)
                {
                    if (getOTP.OTP != obj.OTP)
                    {
                        result.Message = "Maaf, code OTP Salah";
                        result.IsSucceeded = false;
                        return result;
                    }
                    if (getOTP.OTPcreated != null && DateTime.Now.Subtract(DateTime.Parse(getOTP.OTPcreated.ToString())) >= TimeSpan.FromMinutes(expireMinutes))
                    {
                        result.Message = "code OTP sudah kadaluarsa";
                        result.IsSucceeded = false;
                        getOTP.OTP = null;
                        getOTP.OTPcreated = null;
                        await _context.SaveChangesAsync();
                        return result;
                    }
                }
                else
                {
                    result.Message = "User Tidak Ditemukan";
                    result.IsSucceeded = false;
                    return result;
                }

                if (type == "email")
                {
                    getOTP.EmailConfirmed = true;
                    result.Message = "Berhasil Verifikasi Email";
                }
                if (type == "password")
                {
                    var RemovePass = await _userManager.RemovePasswordAsync(getOTP);
                    var changePass = await _userManager.AddPasswordAsync(getOTP, obj.password);

                    if (!changePass.Succeeded)
                    {
                        StringBuilder sb = new StringBuilder();
                        var errors = changePass.Errors;
                        foreach (var error in errors)
                        {
                            sb.Append($"{error.Code} - {error.Description} \n");
                        }
                        throw new Exception($"Error: {sb.ToString()}");
                    }
                    result.Message = "Ganti Password Berhasil";
                }
                getOTP.OTP = null;
                getOTP.OTPcreated = null;
                await _context.SaveChangesAsync();
                result.IsSucceeded = true;
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<BaseResponse> SendEmailAsync(SendEmailDTO obj)
        {
            try
            {
                if (string.IsNullOrEmpty(obj.NameTo)) obj.NameTo = string.Empty;
                BaseResponse baseResponse = new BaseResponse();
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(obj.SendFrom, obj.NameFrom);
                mailMessage.Subject = obj.Subject;
                mailMessage.To.Add(new MailAddress(obj.SendTo, obj.NameTo));
                mailMessage.Body = obj.BodyHTML;
                mailMessage.IsBodyHtml = true;

                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(obj.SendFrom, obj.Password.Replace(" ", ""))
                };
                client.Send(mailMessage);

                baseResponse.IsSucceeded = true;
                baseResponse.Message = $"Successfully Sending Email to {obj.SendTo}";
                return baseResponse;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
       


    }
}
