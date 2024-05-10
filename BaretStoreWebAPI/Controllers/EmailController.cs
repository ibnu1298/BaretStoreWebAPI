using AutoMapper;
using BaretStoreWebAPI.DAL;
using BaretStoreWebAPI.DTO;
using BaretStoreWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BaretStoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEMail _email;
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        MultipleEmailDTO multipleEmailDTO = new MultipleEmailDTO();
        BaseResponse baseResponse = new BaseResponse();

        public EmailController(IEMail email, IMapper mapper, DataContext context)
        {
            _email = email;
            _mapper = mapper;
            _context = context;
        }
        [HttpPost("SendToEmail")]
        public async Task<ActionResult> SendEmail(SendEmailDTO obj)
        {
            CreateEmailDTO createEmailDTO = new CreateEmailDTO();
            try
            {
                #region Check Email
                var checkEmailTo = await _email.CheckFormatEmail(obj.SendTo);
                if (!checkEmailTo.IsSucceeded) 
                {
                    baseResponse.IsSucceeded = checkEmailTo.IsSucceeded;
                    baseResponse.Message = $"{checkEmailTo.Message}, Email To: {obj.SendTo}";
                    return BadRequest(baseResponse);
                }
                var checkEmailFrom = await _email.CheckFormatEmail(obj.SendFrom);
                if (!checkEmailFrom.IsSucceeded) 
                {
                    baseResponse.IsSucceeded = checkEmailFrom.IsSucceeded;
                    baseResponse.Message = $"{checkEmailFrom.Message}, Email From: {obj.SendFrom}";
                    return BadRequest(baseResponse);
                }
                #endregion
                var sendEmail = await _email.SendEmailAsync(obj);
                #region Register Email
                var checkEmail = await _email.CheckEmail(obj.SendTo);
                if (!checkEmail.IsSucceeded && sendEmail.IsSucceeded)
                {
                    baseResponse.IsSucceeded = true;
                    baseResponse.Message = $"{sendEmail.Message}\nRegister Failed: {checkEmail.Message}";
                }
                if(checkEmail.IsSucceeded && sendEmail.IsSucceeded)
                {
                    createEmailDTO.Email = obj.SendTo;
                    createEmailDTO.Name = obj.NameTo;
                    var addEmail = await _email.Insert(createEmailDTO);
                    baseResponse.IsSucceeded = true;
                    baseResponse.Message = $"{sendEmail.Message}\nRegister Email Succeeded";
                }
                #endregion
                return Ok(baseResponse);
            }
            catch (Exception ex)
            {
                 baseResponse.IsSucceeded = false;baseResponse.Message = ex.Message;return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAll()
        {
            try
            {                               
                var results = await _email.GetAll();
                if (results.Count() <= 0)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "Belum Ada Data Email";
                    return StatusCode((int)HttpStatusCode.NotFound, baseResponse);
                }         
                multipleEmailDTO.IsSucceeded = true;
                multipleEmailDTO.Message = $"Berhasil Mengambil {results.Count()} Email";
                multipleEmailDTO.Data = _mapper.Map<List<EmailDTO>>(results);
                return Ok(multipleEmailDTO);
            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
          
            }
        }

        [HttpPost("AddSingleEmail")]
        public async Task<ActionResult> InsertEmail(CreateEmailDTO obj)
        {
            try
            {
                #region Check Email
                var checkEmail = await _email.CheckEmail(obj.Email);
                if (!checkEmail.IsSucceeded) 
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = checkEmail.Message;
                    return BadRequest(baseResponse);
                }
                #endregion

                var results = await _email.Insert(obj);
                var response = _mapper.Map<ResponseCreateEmailDTO>(results);
                if (results != null)
                {
                    response.IsSucceeded = true;
                    response.Message = "Email has been successfully registered";
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("AddMultipleEmail")]
        public async Task<ActionResult> InsertMultipleEmail(CreateMultipleEmailDTO obj)
        {
            try
            {
                #region Check Email
                var data = _mapper.Map<EmailCustomer[]>(obj.Data);
                var checkEmails = await _email.CheckMultipleEmail(data);
                if (checkEmails.Count() > 0)
                {
                    multipleEmailDTO.IsSucceeded = false;
                    multipleEmailDTO.Message = "Incorrect Email Format"; ;
                    multipleEmailDTO.Data = _mapper.Map<List<EmailDTO>>(checkEmails);
                    return BadRequest(multipleEmailDTO);
                }
                var dupDisable = await _email.CheckDuplicateEmailButDisable(data);
                if (dupDisable.Count() > 0)
                {
                    multipleEmailDTO.IsSucceeded = false;
                    multipleEmailDTO.Message = $"Insert Failed, {dupDisable.Count()} Email is registered but disabled";
                    multipleEmailDTO.Data = _mapper.Map<List<EmailDTO>>(dupDisable);
                    return BadRequest(multipleEmailDTO);
                }
                var results = await _email.CheckDuplicate(data);
                if (results.Count() > 0 ) 
                {
                    multipleEmailDTO.IsSucceeded = false;
                    multipleEmailDTO.Message = $"Insert Failed, {results.Count()} Email already registered";
                    multipleEmailDTO.Data = _mapper.Map<List<EmailDTO>>(results);
                    return BadRequest(multipleEmailDTO);
                }
                #endregion
                var result = await _email.InsertBulk(data);
                if (result != null)
                {
                    multipleEmailDTO.IsSucceeded = true;
                    multipleEmailDTO.Message = $"Berhasil Insert {result.Count()}";
                    multipleEmailDTO.Data = _mapper.Map<List<EmailDTO>>(result);
                }
                return Ok(multipleEmailDTO);
            }
            catch (Exception ex)
            {
                 baseResponse.IsSucceeded = false;baseResponse.Message = ex.Message;return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("Disable")]
        public async Task<ActionResult> DisableLink(CreateEmailDTO obj)
        {
            try
            {
                var results = await _email.DisableEmail(obj.Email);
                var email = _mapper.Map<ResponseCreateEmailDTO>(results);
                if (results != null)
                {
                    email.IsSucceeded = true;
                    email.Message = $"Email {obj.Email} has been Successfully Disabled";
                }
                return Ok(email);
            }
            catch (Exception ex)
            {
                 baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("Enable")]
        public async Task<ActionResult> EnableLink(CreateEmailDTO obj)
        {
            try
            {
                var results = await _email.EnableEmail(obj.Email);
                var email = _mapper.Map<ResponseCreateEmailDTO>(results);
                if (results != null)
                {
                    email.IsSucceeded = true;
                    email.Message = $"Email {obj.Email} has been Successfully Activated";
                }
                return Ok(email);
            }
            catch (Exception ex)
            {
                 baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("GetMultipleByEmail")]
        public async Task<ActionResult> GetMultipleLinkEbookBySKU(CreateEmailDTO[] obj)
        {
            MultipleEmailDTO emails = new MultipleEmailDTO();
            try
            {
                var data = _mapper.Map<EmailCustomer[]>(obj);
                var result = await _email.GetsByEmail(data);
                if (result.Count() > 0)
                {
                    emails.IsSucceeded = true;
                    emails.Message = $"Berhasil Mengambil {result.Count()} Email";
                    emails.Data = _mapper.Map<List<EmailDTO>>(result);
                }
                else
                {
                    emails.IsSucceeded = false;
                    emails.Message = $"Email Not Found";
                    emails.Data = null;
                    return NotFound(emails);
                }
                return Ok(emails);
            }
            catch (Exception ex)
            {
                 baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }

        [HttpPost("Edit")]
        public async Task<ActionResult> EditEBook(CreateEmailDTO obj)
        {
            try
            {
                var data = _mapper.Map<EmailCustomer>(obj);
                var edit = await _email.UpdateEmail(data);
                var email = _mapper.Map<ResponseCreateEmailDTO>(edit);
                if (edit != null)
                {
                    email.IsSucceeded = true;
                    email.Message = "Edit Data E-Book Berhasil";
                }
                return Ok(email);
            }
            catch (Exception ex)
            {
                 baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }

        [HttpPost("GetAllDuplicate")]
        public async Task<ActionResult> GetAllDup(CreateMultipleEmailDTO obj)
        {
            try
            {
                var data = _mapper.Map<EmailCustomer[]>(obj.Data);
                var results = await _email.CheckDuplicate(data);
                if (results.Count() > 0)
                {
                    multipleEmailDTO.IsSucceeded = true;
                    multipleEmailDTO.Message = $"Berhasil Mengambil {results.Count()} Email Duplicate";
                    multipleEmailDTO.Data = _mapper.Map<List<EmailDTO>>(results);
                }
                else
                {
                    multipleEmailDTO.IsSucceeded = false;
                    multipleEmailDTO.Message = "Tidak Ada Data Duplicate";
                    multipleEmailDTO.Data = null;
                    return NotFound(multipleEmailDTO);
                }
                return Ok(multipleEmailDTO);
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
