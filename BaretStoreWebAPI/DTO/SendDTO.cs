namespace BaretStoreWebAPI.DTO
{
    public class SendDTO
    {
    }
    public class SendEbookToEmailDTO
    {
        public string Subject { get; set; } = string.Empty;
        public string NameFrom { get; set; } = string.Empty;
        public string SendFrom { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? NameTo { get; set; } = string.Empty;
        public string SendTo { get; set; } = string.Empty;
        public string BodyHTML { get; set; } = string.Empty;
    }
}
