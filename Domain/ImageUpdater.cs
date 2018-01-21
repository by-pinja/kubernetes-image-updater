using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Updater.Domain
{
    public class ImageUpdater
    {
        private readonly ICommandLine _shell;
        private readonly Microsoft.Extensions.Logging.ILogger<ImageUpdater> _logger;
        private readonly UpdaterDbContext _context;

        public ImageUpdater(ICommandLine shell, ILogger<ImageUpdater> logger, UpdaterDbContext context)
        {
            _shell = shell;
            _logger = logger;
            _context = context;
        }

        private class ImageRow
        {
            private ImageRow() {}
            public string Image { get; private set; }
            public string ContainerName { get; private set; }
            public string DeploymentName { get; private set; }
            public string NameSpace { get; private set; }

            public static ImageRow Create(JToken container, JToken deployment)
            {
                return new ImageRow
                {
                    Image = container["image"].Value<string>() ??
                        throw new InvalidOperationException($"Missing containers->image"),
                    ContainerName = container["name"].Value<string>() ??
                        throw new InvalidOperationException($"Missing containers->name"),
                    DeploymentName = deployment["metadata"]["name"].Value<string>() ??
                        throw new InvalidOperationException($"Missing metadata->name"),
                    NameSpace = deployment["metadata"]["namespace"].Value<string>() ??
                        throw new InvalidOperationException($"Missing metadata->namespace")
                };
            }
        }

        public void UpdateEventHandler(string newImageUri)
        {
            var parsedUri = ImageUriParser.ParseUri(newImageUri);

            _shell.Run("kubectl get deployments --all-namespaces -o json")
                .Match<IEnumerable<ImageRow>>(
                    success =>
                    {
                        var asJObject = JObject.Parse(success);
                        return ParseImageRowsFromJsonResponse(asJObject);
                    }, error => {
                        _logger.LogError($"Failed to fetch deployment json from kubernetes, error: {error}");
                        return Enumerable.Empty<ImageRow>();
                    })
                .Where(image => ImageUriParser.ParseUri(image.Image).uri == parsedUri.uri)
                .ToList()
                .ForEach(image => SetNewImage(parsedUri, image));
        }

        private static IEnumerable<ImageRow> ParseImageRowsFromJsonResponse(JObject asJObject)
        {
            return asJObject["items"].Children()
                .Where(item => item["kind"].Value<string>() == "Deployment")
                .SelectMany(deployment =>
                {
                    var containers = deployment["spec"]["template"]["spec"]["containers"].Value<JArray>();

                    return containers
                        .Select(container => ImageRow.Create(container, deployment));
                })
                .ToList();
        }

        private void SetNewImage((string uri, string tag) imageUri, ImageRow image) =>
            _shell.Run($"kubectl set image deployment/{image.DeploymentName} {image.ContainerName}={imageUri.uri}:{imageUri.tag} --namespace={image.NameSpace}")
                .Match(
                    output =>
                    {
                        _context.EventHistory.Add(new ImageEvent()
                        {
                            Image = imageUri.uri,
                            Tag = imageUri.tag,
                            Stamp = DateTime.UtcNow
                        });
                        _context.SaveChanges();
                        _logger.LogInformation($"Updated image 'deployment/{image.DeploymentName} {image.ContainerName}={imageUri.uri}:{imageUri.tag} --namespace={image.NameSpace}', output: {output}");
                    },
                    error => _logger.LogError(error.ToString()));
    }
}