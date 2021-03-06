using System;

namespace Updater.Domain
{
    public static class ImageUriParser
    {
        public static (string uri, string tag) ParseUri(string imageUri)
        {
            var results = imageUri.Split(":");

            if(results.Length == 1)
                return (imageUri, "latest");

            if(results.Length > 2)
                throw new ArgumentException($"Cound not parse uri + tag pair from {imageUri}");

            if(string.IsNullOrEmpty(results[0]))
                throw new ArgumentException($"Parsing failed, given uri is invalid {imageUri}");

            return (results[0],results[1]);
        }
    }
}