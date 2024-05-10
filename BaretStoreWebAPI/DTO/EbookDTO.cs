using System.ComponentModel.DataAnnotations;

namespace BaretStoreWebAPI.DTO
{
    public class EbookDTO
    {
        public int Id { get; set; }
        public int SKU { get; set; }
        public string EbookName { get; set; } = string.Empty;
        public string EbookLinkPDF { get; set; } = string.Empty;
        public string EbookLinkImage { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
    public class AddEbookDTO
    {
        public int SKU { get; set; }
        public string EbookName { get; set; } = string.Empty;
        public string EbookLinkPDF { get; set; } = string.Empty;
        public string EbookLinkImage { get; set; } = string.Empty;
    }
    public class AddMultipleEbookDTO
    {
        public List<AddEbookDTO> Ebooks { get; set; }
    }
    public class EbookResponseDTO : BaseResponse
    {
        public int Id { get; set; }
        public int SKU { get; set; }
        public string EbookName { get; set; } = string.Empty;
        public string EbookLinkPDF { get; set; } = string.Empty;
        public string EbookLinkImage { get; set; } = string.Empty;
    }
    public class SendEbookDTO
    {

        public string Email { get; set; } = string.Empty;
        [Required]
        public int[]? SKU { get; set; }
    }
    public class MultipleEbookDTO : BaseResponse
    {
        public List<EbookDTO> Ebooks { get; set; }
    }
    public class ReadSKU
    {
        public int SKU { get; set; }
    }
    public class EbookNameDTO
    {
        public string EbookName { get; set; } = string.Empty;
    }
}
