using System;
using System.Collections.Generic;
using System.Text;

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
    
    public class Image
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public ImageType Type { get; set; }
    }
}
