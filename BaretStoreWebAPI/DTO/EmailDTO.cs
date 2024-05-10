
namespace BaretStoreWebAPI.DTO
{
    public class EmailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    public class MultipleEmailDTO : BaseResponse
    {
        public List<EmailDTO> Data { get; set; }
    }
    public class ResponseCreateEmailDTO : BaseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    public class CreateEmailDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
    public class CreateMultipleEmailDTO
    {
        public List<CreateEmailDTO> Data { get; set; }
    }
    public class SendEmailDTO
    {
        public string Subject { get; set; } = string.Empty;
        public string NameFrom { get; set; } = string.Empty;
        public string SendFrom { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? NameTo { get; set; } = string.Empty;
        public string SendTo { get; set; } = string.Empty;      
        public string? BodyHTML { get; set; } = string.Empty;
        public int[]? SKU { get; set; }
    }
    public class verifikasiOTPDTO
    {
        public string password { get; set; } = string.Empty;
        public string usernameOrEmail { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }
    
}
