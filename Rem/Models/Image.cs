using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Rem.Models
{
    public enum ImageType
    {
        Pat,
        Hug,
        Cuddle
    }

    public static class ImageTypeExtensions
    {
        public static string ToFriendlyString(this ImageType t)
        {
            switch (t)
            {
                default:
                    return t.ToString().ToLower();
            }
        }
    }

    [Table("Images")]
    public class Image
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        [Unique]
        public string Url { get; set; }
        public ImageType Type { get; set; }
    }
}
