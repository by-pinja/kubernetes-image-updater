using System;

namespace Updater.Domain
{
    public class ImageUriParser
    {
        public static (string uri, string tag) ParseUri(string imageUri)
        {
            var results = imageUri.Split(":");

            if(results.Length == 1)
                return (imageUri, "latest");

            if(results.Length > 2)
                throw new ArgumentException($"Cound not parse uri + tag pair from {imageUri}");

            return (results[0],results[1]);
        }
    }
}