using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Rem.Models
{
    [Table("Quotes")]
    public class Quote
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string QuoteString { get; set; }
        public string AuthorId { get; set; }
        public string QuotedById { get; set; }
        public DateTime QuoteTime { get; set; }
    }
}
