using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaretStoreWebAPI.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserID { get; set; } = string.Empty;
        public List<Ebook> Ebook { get; set; }

    }
}
