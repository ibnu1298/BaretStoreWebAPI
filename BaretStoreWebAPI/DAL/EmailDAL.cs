using AutoMapper;
using BaretStoreWebAPI.DTO;
using BaretStoreWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace BaretStoreWebAPI.DAL
{
    public interface IEMail
    {
        Task<BaseResponse> SendEmailAsync(SendEmailDTO obj);
        Task<EmailCustomer> Insert(CreateEmailDTO obj);
        Task<IEnumerable<EmailCustomer>> InsertBulk(EmailCustomer[] obj);
        Task<IEnumerable<EmailCustomer>> GetAll();
        Task<BaseResponse> CheckEmail(string email);
        Task<IEnumerable<EmailCustomer>> CheckMultipleEmail(EmailCustomer[] obj);
        Task<List<EmailCustomer>> CheckDuplicate(EmailCustomer[] obj);
        Task<List<EmailCustomer>> CheckDuplicateEmailButDisable(EmailCustomer[] obj);
        Task<IEnumerable<EmailCustomer>> GetsByEmail(EmailCustomer[] obj);
        Task<EmailCustomer> DisableEmail(string email);
        Task<EmailCustomer> EnableEmail(string email);
        Task<EmailCustomer> UpdateEmail(EmailCustomer linkEbook);
        Task<BaseResponse> CheckFormatEmail(string email);

    }
    public class EmailDAL  : IEMail
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public EmailDAL(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<BaseResponse> SendEmailAsync(SendEmailDTO obj)
        {
            try
            {
                if(string.IsNullOrEmpty(obj.NameTo)) obj.NameTo = string.Empty;
                BaseResponse baseResponse= new BaseResponse();
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(obj.SendFrom,obj.NameFrom);
                mailMessage.Subject = obj.Subject;
                mailMessage.To.Add(new MailAddress(obj.SendTo,obj.NameTo));
                
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

        public async Task<IEnumerable<EmailCustomer>> GetAll()
        {
            try
            {
                var results = await _context.EmailCustomers.OrderBy(x => x.Email).Where(x => x.RowStatus == 0).ToListAsync();
                
                return results;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task<EmailCustomer> Insert(CreateEmailDTO obj)
        {
            try
            {
                EmailCustomer emailCustomer = new EmailCustomer();
                if(!string.IsNullOrEmpty(obj.Name)) emailCustomer.Name = obj.Name;
                emailCustomer.Email = obj.Email.ToLower();
                _context.EmailCustomers.Add(emailCustomer);
                await _context.SaveChangesAsync();
                return emailCustomer;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task<IEnumerable<EmailCustomer>> InsertBulk(EmailCustomer[] obj)
        {
            try
            {
                List<EmailCustomer> emails = new List<EmailCustomer>();
                foreach (var item in obj)
                {
                    var result = await _context.EmailCustomers.FirstOrDefaultAsync(x => x.Email.ToLower() == item.Email.ToLower() && x.RowStatus == 0);
                    if (result != null) throw new Exception($"Error: Email {result.Email} Sudah Terdaftar");
                    if (string.IsNullOrEmpty(item.Name)) item.Name = "";
                    item.Email = item.Email.ToLower();
                    _context.EmailCustomers.Add(item);
                    emails.Add(item);
                }
                await _context.SaveChangesAsync();
                return emails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<EmailCustomer> UpdateEmail(EmailCustomer obj)
        {
            try
            {
                var edit = await _context.EmailCustomers.FirstOrDefaultAsync(c => c.Email == obj.Email && obj.RowStatus == 0);
                if (edit == null)
                    throw new Exception($"Error: Data Tidak ditemukan");
                if (!string.IsNullOrEmpty(obj.Name)) edit.Name = obj.Name;
                if (!string.IsNullOrEmpty(obj.Email)) edit.Email = obj.Email;
                await _context.SaveChangesAsync();
                return edit;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<BaseResponse> CheckEmail(string email)
        {
            BaseResponse baseResponse = new BaseResponse();
            try
            {
                var result = await _context.EmailCustomers.FirstOrDefaultAsync(x => x.Email == email.ToLower());
                if (result != null)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "Email has been registered";
                    return baseResponse;
                }
                if (!email.Contains("@") || !email.Contains("."))
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
        public async Task<BaseResponse> CheckFormatEmail(string email)
        {
            BaseResponse baseResponse = new BaseResponse();
            try
            {
                email = email.ToLower();
                if (!email.Contains("@") || !email.Contains("."))
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
        public async Task<IEnumerable<EmailCustomer>> CheckMultipleEmail(EmailCustomer[] obj)
        {
            List<EmailCustomer> emails = new List<EmailCustomer>();
            try
            {
                foreach (var item in obj)
                {
                    var result = await _context.EmailCustomers.FirstOrDefaultAsync(x => x.Email == item.Email.ToLower());
                    if (!item.Email.Contains("@") || !item.Email.Contains(".")) emails.Add(item);                    
                }
                return emails;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<EmailCustomer>> CheckDuplicate(EmailCustomer[] obj)
        {
            try
            {
                List<EmailCustomer> emails = new List<EmailCustomer>();
                foreach (var item in obj)
                {
                    var result = await _context.EmailCustomers.FirstOrDefaultAsync(x => x.Email == item.Email.ToLower());
                    if (result != null) emails.Add(result);
                }
                return emails;
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<EmailCustomer>> CheckDuplicateEmailButDisable(EmailCustomer[] obj)
        {
            try
            {
                List<EmailCustomer> emails = new List<EmailCustomer>();
                foreach (var item in obj)
                {
                    var result = await _context.EmailCustomers.FirstOrDefaultAsync(x => x.Email == item.Email.ToLower() && x.RowStatus == 1);
                    if (result != null) emails.Add(result);
                }
                return emails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IEnumerable<EmailCustomer>> GetsByEmail(EmailCustomer[] obj)
        {
            try
            {
                List<EmailCustomer> emails = new List<EmailCustomer>();
                foreach (var item in obj)
                {
                    var result = await _context.EmailCustomers.FirstOrDefaultAsync(x => x.Email == item.Email.ToLower() && x.RowStatus == 0);
                    if (result != null)
                    {
                        if (emails.Contains(emails.FirstOrDefault(x => x.Email == item.Email.ToLower()))) emails.Remove(result);
                        emails.Add(result);
                    }
                }
                return emails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<EmailCustomer> DisableEmail(string email)
        {
            try
            {
                var check = await _context.EmailCustomers.FirstOrDefaultAsync(x => x.Email == email.ToLower() && x.RowStatus == 0);
                if (check == null) throw new Exception($"Error: Data Tidak Ditemukan");
                check.RowStatus = 1;
                await _context.SaveChangesAsync();
                return check;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task<EmailCustomer> EnableEmail(string email)
        {
            try
            {
                var check = await _context.EmailCustomers.FirstOrDefaultAsync(x => x.Email == email.ToLower() && x.RowStatus == 1);
                if (check == null) throw new Exception($"Error: Data Tidak ditemukan");
                check.RowStatus = 0;
                await _context.SaveChangesAsync();
                return check;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
