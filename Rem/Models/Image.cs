using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Rem.Models
{
    public enum ImageType
    {
        Headpat,
        Hug,
        Cuddle
    }

    public static class ImageTypeExtensions
    {
        public static string ToFriendlyString(this ImageType t)
        {
            switch (t)
            {
                case ImageType.Headpat:
                    return "pat";
                case ImageType.Hug:
                    return "hug";
                case ImageType.Cuddle:
                    return "cuddle";
                default:
                    return t.ToString();
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
