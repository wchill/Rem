using System;
using System.Collections.Generic;
using System.Net;
using MemeGenerator;
using RestSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rem.Utilities
{
    public class StringImageHttpDownloadTransformer : IStringTransformer
    {
        private static readonly HashSet<string> ValidMimeTypes = new HashSet<string>
        {
            "image/bmp",
            "image/gif",
            "image/jpeg",
            "image/png",
            // "image/webp"
        };

        public bool TryTransform(string input, out object transformed)
        {
            if (TryCreateUrl(input, out var url))
            {
                var lazy = new AsyncLazy<Image<Rgba32>>(async () =>
                {
                    var client = new RestClient($"{url.Scheme}://{url.Authority}");
                    var request = new RestRequest(url.AbsolutePath);
                    var response = await client.ExecuteGetTaskAsync(request);

                    if (response.StatusCode == HttpStatusCode.OK && ValidMimeTypes.Contains(response.ContentType))
                    {
                        return Image.Load(response.RawBytes);
                    }

                    return null;
                });
                lazy.Start();
                transformed = lazy;
                return true;
            }

            transformed = null;
            return false;
        }

        private static bool TryCreateUrl(string source, out Uri output)
        {
            return Uri.TryCreate(source, UriKind.Absolute, out output) && 
                   (output.Scheme == Uri.UriSchemeHttp || output.Scheme == Uri.UriSchemeHttps);
        }
    }
}