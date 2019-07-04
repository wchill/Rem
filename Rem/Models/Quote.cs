using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Rem.Models
{
    public class Quote
    {
        [Key]
        public int Id { get; set; }
        public string QuoteString { get; set; }
        public string AuthorId { get; set; }
        public string QuotedById { get; set; }
        public DateTime QuoteTime { get; set; }
    }
}
