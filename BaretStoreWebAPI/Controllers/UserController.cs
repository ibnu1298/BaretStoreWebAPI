using AutoMapper;
using BaretStoreWebAPI.Authentication;
using BaretStoreWebAPI.DTO;
using BaretStoreWebAPI.Models;
using EbookWebAPI.DAL;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;

namespace BaretStoreWebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUser _user;
        private readonly IMapper _mapper;
        private UserManager<User> _userManager;
        private readonly DataContext _context;
        private readonly RoleManager<CustomRole> _roleManager;
        public UserController(IUser user, IMapper mapper, DataContext context, RoleManager<CustomRole> roleManager, UserManager<User> userManager)
        {
            _mapper = mapper;
            _context = context;
            _user = user;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        BaseResponse baseResponse = new BaseResponse();
        SendEmailDTO sendEmailDTO = new SendEmailDTO();
        string nameFrom = "Baret Store";
        
        [HttpPost("UpdateImage")]
        public async Task<ActionResult> UpdateImage(UpdateImageDTO obj)
        {
            try
            {
                var result = await _user.GetByEmailOrUser(obj.Id);
                if (result == null)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = $"User Not Found";
                    return NotFound(baseResponse);
                }
                result.URLImage = obj.ImageURL;
                await _context.SaveChangesAsync();
                result = await _user.GetByEmailOrUser(obj.Id);
                var userDTO = _mapper.Map<UserDTO>(result);
                userDTO.IsSucceeded = true;
                userDTO.Message = $"Update Image Berhasil";
                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                baseResponse.Message = ex.Message;
                baseResponse.IsSucceeded = false;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }

        [HttpPost("UpdateUser")]
        public async Task<ActionResult> UpdateUser(UpdateUserDTO obj)
        {
            try
            {
                var result = await _user.GetByEmailOrUser(obj.Id);
                if (result == null)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = $"User Not Found";
                    return NotFound(baseResponse);
                }
                var updateUser = await _user.Update(obj);
                var userDTO = _mapper.Map<UserDTO>(updateUser);
                userDTO.IsSucceeded = true;
                userDTO.Message = $"Update Data Berhasil";
                return StatusCode((int)HttpStatusCode.Created, userDTO);
            }
            catch (Exception ex)
            {
                baseResponse.Message = ex.Message;
                baseResponse.IsSucceeded = false;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("UpdateCredential")]
        public async Task<ActionResult> UpdateCredential(UpdateCredentialDTO obj)
        {
            try
            {
                var result = await _user.GetByEmailOrUser(obj.Id);
                if (result == null)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = $"User Not Found";
                    return NotFound(baseResponse);
                }
                if ((!string.IsNullOrEmpty(obj.UserName) || !string.IsNullOrEmpty(obj.Email)) && !obj.ChangePassword)
                {
                    if (obj.Email != result.Email)
                    {
                        var checkEmail = await _user.CheckEmail(obj.Email);
                        if (!checkEmail.IsSucceeded && !string.IsNullOrEmpty(obj.Email))
                        {
                            baseResponse.IsSucceeded = checkEmail.IsSucceeded;
                            baseResponse.Message = checkEmail.Message;
                            return BadRequest(baseResponse);
                        }
                    }
                    if (obj.UserName != result.UserName)
                    {
                        var checkUsername = await _user.CheckUsername(obj.UserName);
                        if (!checkUsername.IsSucceeded && !string.IsNullOrEmpty(obj.UserName))
                        {
                            baseResponse.IsSucceeded = checkUsername.IsSucceeded;
                            baseResponse.Message = checkUsername.Message;
                            return BadRequest(baseResponse);
                        }
                    }
                }
                var userResult = await _userManager.CheckPasswordAsync(result, obj.OldPassword);
                if (obj.ChangePassword)
                {
                    if (result.EmailConfirmed)
                    {
                        if (obj.Email == result.Email)
                        {
                            if (!userResult)
                            {
                                baseResponse.IsSucceeded = false;
                                baseResponse.Message = "Autentikasi gagal!, Password Salah";
                                return Unauthorized(baseResponse);
                            }
                            else
                            {
                                var changePass = await _userManager.ChangePasswordAsync(result, obj.OldPassword, obj.NewPassword);
                                if (changePass.Succeeded)
                                {
                                    baseResponse.IsSucceeded = true;
                                    baseResponse.Message = "Change Password Successfully";
                                    return Ok(baseResponse);
                                }
                                else
                                {
                                    baseResponse.IsSucceeded = false;
                                    baseResponse.Message = changePass.ToString();
                                    return BadRequest(baseResponse);
                                }
                            }
                        }
                        baseResponse.IsSucceeded = false;
                        baseResponse.Message = "Email Salah";
                        return BadRequest(baseResponse);
                    }
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "Belum Verifikasi Email";
                    return BadRequest(baseResponse);
                }

                if (!obj.ChangePassword && !string.IsNullOrEmpty(result.PasswordHash))
                {
                    if (!userResult)
                    {
                        baseResponse.IsSucceeded = false;
                        baseResponse.Message = "Autentikasi gagal!, Password Salah";
                        return Unauthorized(baseResponse);
                    }
                }

                var updateUser = await _user.UpdateCredential(obj);
                if (!updateUser.EmailConfirmed)
                {
                    #region Prepare Send Email
                    var generateOTP = await _user.GenerateOTP(result.Email, true);
                    sendEmailDTO.NameTo = $"{result.Name}";
                    sendEmailDTO.SendTo = result.Email;
                    sendEmailDTO.SendFrom = "wifi.booyah.net@gmail.com";
                    sendEmailDTO.NameFrom = nameFrom;
                    sendEmailDTO.Password = "lapf dqpn lhnf epcc";
                    sendEmailDTO.Subject = $"Email verification code: {generateOTP}";
                    sendEmailDTO.BodyHTML = $"<body> <div style=\"margin: auto; padding: 50px;width: 720px; border: 1px solid #444444; border-radius: 15px; padding: 10px; \" ><h2 style=\"text-align: center\">Konfirmasi Email {result.Email}</h2><hr/><p>Hallo {result.Name} <br/>Silakan gunakan Code berikut untuk melakukan Verifikasi email di BooyahNet:</p><h1 style=\"text-align: center;font-size: 50px;\">{generateOTP}</h1><p>Code berlaku selama 24 Jam setelah anda menerima Email ini</p><p>Terimakasih</p><p>{nameFrom}</p></div></body>";
                    #endregion
                    var sendEmail = await _user.SendEmailAsync(sendEmailDTO);
                }
                var userDTO = _mapper.Map<UserDTO>(updateUser);
                userDTO.IsSucceeded = true;
                userDTO.Message = $"Update Data Berhasil";
                userDTO.PasswordExist = !string.IsNullOrEmpty(result.PasswordHash) ? true : false;
                return StatusCode((int)HttpStatusCode.Created, userDTO);
            }
            catch (Exception ex)
            {
                baseResponse.Message = ex.Message;
                baseResponse.IsSucceeded = false;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("GetByEmailOrUsername")]
        public async Task<ActionResult> GetByEmail(UsernameOrEmailDTO obj)
        {
            try
            {
                var result = await _user.GetByEmailOrUser(obj.UsernameOrEmail);
                if (result == null)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = $"User Not Found";
                    return NotFound(baseResponse);
                }
                var userDTO = _mapper.Map<UserDTO>(result);
                userDTO.PasswordExist = !string.IsNullOrEmpty(result.PasswordHash) ? true : false;
                userDTO.IsSucceeded = true;
                userDTO.Message = $"Pengambilan data User Berhasil";
                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                baseResponse.Message = ex.Message;
                baseResponse.IsSucceeded = false;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetById(string id)
        {

            try
            {
                var result = await _user.GetById(id);
                if (result == null)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "User Tidak Ditemukan";
                    return NotFound(baseResponse);
                }

                var userDTO = _mapper.Map<UserDTO>(result);
                if (result != null)
                {
                    userDTO.IsSucceeded = true;
                    userDTO.Message = $"Pengambilan data User {userDTO.UserName} Berhasil";
                }
                return userDTO;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("CekUserName")]
        public async Task<ActionResult<BaseResponse>> CheckUsername(UsernameOrEmailDTO obj)
        {
            try
            {
                var result = await _user.GetByEmailOrUser(obj.UsernameOrEmail);
                var username = await _user.CheckEmail(result.UserName);
                return username;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("CekEmail")]
        public async Task<ActionResult<BaseResponse>> CheckEmail(UsernameOrEmailDTO obj)
        {
            try
            {
                var result = await _user.GetByEmailOrUser(obj.UsernameOrEmail);
                var email = await _user.CheckEmail(result.Email);
                return email;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("EditUserName")]
        public async Task<ActionResult> EditUsername(UpdateUsernameDTO obj)
        {
            try
            {
                UserDTO userDTO = new UserDTO();
                #region Validate
                var user = await _context.Users.FirstOrDefaultAsync(c => c.Id == obj.Id);
                if (user == null) return BadRequest($"Username: {obj.Username} Tidak ditemukan");
                var userResult = await _userManager.CheckPasswordAsync(user, obj.Password);
                if (!userResult) return BadRequest($"Password Salah");
                #endregion
                user.UserName = obj.Username;
                var username = await _context.Users.FirstOrDefaultAsync(c => c.UserName == obj.Username);
                if (username != null) return BadRequest($"{obj.Username} Sudah digunakan");
                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    userDTO.Message = "Update Data Email Berhasil";
                    userDTO.IsSucceeded = true;
                }
                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("EditEmail")]
        public async Task<ActionResult<UserDTO>> EditEmail(UpdateEmailDTO obj)
        {
            try
            {
                UserDTO emailDTO = new UserDTO();
                #region Validate
                var user = await _context.Users.FirstOrDefaultAsync(c => c.Id == obj.Id);
                if (user == null) return BadRequest($"Data dengan Id: {obj.Id} Tidak ditemukan");
                var userResult = await _userManager.CheckPasswordAsync(user, obj.Password);
                if (!userResult) return BadRequest($"Password Salah");
                var checkEmail = await _user.CheckEmail(obj.Email);
                if (!checkEmail.IsSucceeded)
                {
                    baseResponse.Message = checkEmail.Message;
                    baseResponse.IsSucceeded = checkEmail.IsSucceeded;
                    return BadRequest(baseResponse);
                }
                #endregion
                user.Email = obj.Email;
                var result = await _context.SaveChangesAsync();
                emailDTO = _mapper.Map<UserDTO>(user);
                if (result > 0)
                {
                    emailDTO.Message = "Update Data Email Berhasil";
                    emailDTO.IsSucceeded = true;
                }
                return Ok(emailDTO);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult> LoginUsernameOrEmail(AuthenticateRequest obj)
        {
            try
            {
                var getUser = await _user.GetByEmailOrUser(obj.UsernameOrEmail);
                if ((getUser == null || (getUser.UserName != obj.UsernameOrEmail && getUser.Email != obj.UsernameOrEmail.ToLower())) && string.IsNullOrEmpty(obj.Type))
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "Autentikasi gagal!, Email atau Username Salah";
                    return Unauthorized(baseResponse);
                }
                if (getUser != null && (getUser.UserName == obj.UsernameOrEmail || getUser.Email == obj.UsernameOrEmail.ToLower()) && !string.IsNullOrEmpty(obj.Type))
                {
                    getUser.EmailConfirmed = true;
                    getUser.ProviderId = obj.ProviderId;
                    await _context.SaveChangesAsync();
                }
                if ((getUser == null || (getUser.UserName != obj.UsernameOrEmail && getUser.Email != obj.UsernameOrEmail.ToLower())) && !string.IsNullOrEmpty(obj.Type))
                {
                    var generateCustNo = await _user.GenerateCustomerNo();
                    var results = await _context.Users.OrderBy(c => c.Name).ToListAsync();
                    
                    var newUser = new User
                    {
                        Name = obj.Name,
                        UserName = obj.UsernameOrEmail,
                        URLImage = obj.URLImage,
                        Email = obj.UsernameOrEmail,
                        EmailConfirmed = true,

                    };
                    var result = await _userManager.CreateAsync(newUser);
                    if (!result.Succeeded)
                    {
                        baseResponse.IsSucceeded = false;
                        baseResponse.Message = result.ToString();
                        return Unauthorized(baseResponse);
                    }
                    getUser = await _user.GetByEmailOrUser(obj.UsernameOrEmail);
                    await _userManager.AddToRoleAsync(getUser, "user");
                }
                else
                {
                    if (!string.IsNullOrEmpty(obj.ProviderId))
                    {
                        getUser = await _user.GetByEmailProvider(getUser.Email, obj.ProviderId);
                        getUser.Email = obj.UsernameOrEmail.ToLower();
                        getUser.NormalizedEmail = obj.UsernameOrEmail.ToUpper();
                        await _context.SaveChangesAsync();
                    }
                }
                getUser = await _user.GetByEmailOrUser(obj.UsernameOrEmail);
                var currUser = await _userManager.FindByNameAsync(getUser.UserName);
                if (string.IsNullOrEmpty(obj.Type))
                {
                    var userResult = await _userManager.CheckPasswordAsync(currUser, obj.Password);
                    if (!userResult && string.IsNullOrEmpty(obj.Type))
                    {
                        baseResponse.IsSucceeded = false;
                        baseResponse.Message = "Autentikasi gagal!, Password Salah";
                        return Unauthorized(baseResponse);
                    }
                }
                var response = await _user.Authenticate(currUser);
                TokenDTO token = new TokenDTO();
                token.IsSucceeded = true;
                token.Message = response.Message;
                token.Token = response.Token;
                return Ok(token);
            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                baseResponse.IsSucceeded = false;
                baseResponse.Message = $"{ex.Message},Line - {line}";
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult> Register(CreateUserDTO obj)
        {

            try
            {
                UserDTO userDTO = new UserDTO();
                #region CheckEmailandUserName
                var checkEmail = await _user.CheckEmail(obj.Email);
                if (!checkEmail.IsSucceeded && !string.IsNullOrEmpty(obj.Email))
                {
                    baseResponse.IsSucceeded = checkEmail.IsSucceeded;
                    baseResponse.Message = checkEmail.Message;
                    return BadRequest(baseResponse);
                }
                var checkUsername = await _user.CheckUsername(obj.UserName);
                if (!checkUsername.IsSucceeded && !string.IsNullOrEmpty(obj.UserName))
                {
                    baseResponse.IsSucceeded = checkUsername.IsSucceeded;
                    baseResponse.Message = checkUsername.Message;
                    return BadRequest(baseResponse);
                }
                #endregion
                var result = await _user.Registration(obj);
                if (result == null)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = $"Registration Failed";
                }
                await _userManager.AddToRoleAsync(result, "user");
                var generateOTP = await _user.GenerateOTP(result.Email, true);
                userDTO = _mapper.Map<UserDTO>(result);
                if (result != null)
                {
                    userDTO.Message = $"Registration Success";
                    userDTO.IsSucceeded = true;
                    #region Prepare Send Email
                    sendEmailDTO.NameTo = $"{result.Name}";
                    sendEmailDTO.SendTo = result.Email;
                    sendEmailDTO.SendFrom = "wifi.booyah.net@gmail.com";
                    sendEmailDTO.NameFrom = nameFrom;
                    sendEmailDTO.Password = "lapf dqpn lhnf epcc";
                    sendEmailDTO.Subject = $"Email verification code: {generateOTP}";
                    sendEmailDTO.BodyHTML = $"<body> <div style=\"margin: auto; padding: 50px;width: 720px; border: 1px solid #444444; border-radius: 15px; padding: 10px; \" ><h2 style=\"text-align: center\">Konfirmasi Email {result.Email}</h2><hr/><p>Hallo {result.Name} <br/>Silakan gunakan Code berikut untuk melakukan Verifikasi email di BooyahNet:</p><h1 style=\"text-align: center;font-size: 50px;\">{generateOTP}</h1><p>Code berlaku selama 24 Jam setelah anda menerima Email ini</p><p>Terimakasih</p><p>{nameFrom}</p></div></body>";
                    #endregion
                    var sendEmail = await _user.SendEmailAsync(sendEmailDTO);
                }
                return StatusCode((int)HttpStatusCode.Created, userDTO);
            }
            catch (Exception ex)
            {
                baseResponse.Message = ex.Message;
                baseResponse.IsSucceeded = false;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("OTP-VerifikasiEmail")]
        public async Task<ActionResult> VerifikasiEmailOTP(verifikasiOTPDTO obj)
        {
            BaseResponse baseResponse = new BaseResponse();
            int jam = 24;
            int menit = jam * 60;
            var result = await _user.VerifyOTP(obj, menit, "email");
            if (result.IsSucceeded == false)
            {
                baseResponse.IsSucceeded = result.IsSucceeded;
                baseResponse.Message = result.Message;
                return BadRequest(baseResponse);
            }
            else
            {
                baseResponse.IsSucceeded = result.IsSucceeded;
                baseResponse.Message = result.Message;
            }
            return Ok(baseResponse);
        }
        [AllowAnonymous]
        [HttpPost("OTP-ForgotPasswod")]
        public async Task<ActionResult> ForgotPasswodOTP(verifikasiOTPDTO obj)
        {
            BaseResponse baseResponse = new BaseResponse();
            int jam = 24;
            int menit = jam * 60;
            var result = await _user.VerifyOTP(obj, menit, "password");
            if (!result.IsSucceeded)
            {
                baseResponse.IsSucceeded = result.IsSucceeded;
                baseResponse.Message = result.Message;
                return BadRequest(baseResponse);
            }
            baseResponse.IsSucceeded = result.IsSucceeded;
            baseResponse.Message = result.Message;
            return Ok(baseResponse);
        }
        [AllowAnonymous]
        [HttpPost("SendOTP")]
        public async Task<ActionResult> SendOTP(UsernameOrEmailDTO obj)
        {
            BaseResponse baseResponse = new BaseResponse();
            SendEmailDTO sendEmailDTO = new SendEmailDTO();
            var getUser = await _user.GetByEmailOrUser(obj.UsernameOrEmail);
            if (getUser == null)
            {
                baseResponse.Message = $"{obj.UsernameOrEmail} Tidak ditemukan";
                baseResponse.IsSucceeded = false;
                return StatusCode((int)HttpStatusCode.BadRequest, baseResponse);
            }
            try
            {
                #region Prepare Send Email
                var nameFrom = "BooyahNet";
                var generateOTP = await _user.GenerateOTP(getUser.Email, true);
                sendEmailDTO.NameTo = $"{getUser.Name}";
                sendEmailDTO.SendTo = getUser.Email;
                sendEmailDTO.SendFrom = "wifi.booyah.net@gmail.com";
                sendEmailDTO.NameFrom = nameFrom;
                sendEmailDTO.Password = "lapf dqpn lhnf epcc";
                sendEmailDTO.Subject = $"Email verification code: {generateOTP}";
                sendEmailDTO.BodyHTML = $"<body> <div style=\"margin: auto; padding: 50px;width: 720px; border: 1px solid #444444; border-radius: 15px; padding: 10px; \" ><h2 style=\"text-align: center\">Code OTP BooyahNet</h2><hr/><p>Hallo {getUser.Name} <br/>Harap tidak memberitahukan code OTP ini kepada siapapun:</p><h1 style=\"text-align: center;font-size: 50px;\">{generateOTP}</h1><p>Code OTP berlaku selama 24 Jam setelah anda menerima Email ini</p><p>Terimakasih</p><p>{nameFrom}</p></div></body>";
                #endregion
                var sendEmail = await _user.SendEmailAsync(sendEmailDTO);
                if (!sendEmail.IsSucceeded)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = $"Send OTP to Email {getUser.Email} Failed";
                    return BadRequest(baseResponse);
                }
                baseResponse.Message = $"Send OTP to Email {getUser.Email} Successful";
                baseResponse.IsSucceeded = sendEmail.IsSucceeded;
                return Ok(baseResponse);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return Ok(baseResponse);
        }
        [HttpPost("AddRole")]
        public async Task<ActionResult> Post(RoleNameDTO role)
        {
            try
            {
                CustomRole myRole = new CustomRole
                {
                    Name = role.RoleName.ToLower()
                };
                var getRole = await _context.Roles.FirstOrDefaultAsync(y => y.Name.Contains(role.RoleName));
                if (getRole != null)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = $"Role {role.RoleName} already exists";
                    return StatusCode((int)HttpStatusCode.BadRequest, baseResponse);
                }
                var result = await _roleManager.CreateAsync(myRole);
                if (!result.Succeeded)
                {                    
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "Gagal membuat Role Baru";
                    return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
                }             
                baseResponse.IsSucceeded = true;
                baseResponse.Message = $"Role {role.RoleName} added successfully";
                return StatusCode((int)HttpStatusCode.Created, baseResponse);
            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }

    }
}

