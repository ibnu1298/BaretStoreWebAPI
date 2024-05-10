using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaretStoreWebAPI.Models
{
    public class Ebook
    {
        public int Id { get; set; }     
        public int SKU { get; set; }
        public string EbookName { get; set; } = string.Empty;
        public string EbookLinkPDF { get; set; } = string.Empty;
        public string EbookLinkImage { get; set; } = string.Empty;
        public int Price { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModificationTime { get; set; } = DateTime.Now;
        public string? ModificationBy { get; set; } = string.Empty;
        public int RowStatus { get; set; } = 0;
        public List<Cart> Cart { get; set; }

    }
}
