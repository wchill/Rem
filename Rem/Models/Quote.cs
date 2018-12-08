using System;
using SQLite;

namespace Rem.Models
{
    [Table("Quotes")]
    public class Quote
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string QuoteString { get; set; }
        public string QuotedBy { get; set; }
        public DateTime QuotedAt { get; set; }
        public int QuoteCount { get; set; }
    }
}
