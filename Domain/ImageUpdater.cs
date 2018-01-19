using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Updater.Domain
{
    public class ImageUpdater
    {
        private readonly ICommandLine _shell;
        private readonly Microsoft.Extensions.Logging.ILogger<ImageUpdater> _logger;

        public ImageUpdater(ICommandLine shell, ILogger<ImageUpdater> logger)
        {
            _shell = shell;
            _logger = logger;
        }

        public void UpdateEventHandler(string imageUri)
        {
            var parsedUri = ImageUriParser.ParseUri(imageUri);

            var allDeploymentsCmdAsJson = "kubectl get deployments --all-namespaces -o json";

            _shell.Run(allDeploymentsCmdAsJson).Match(
                success =>
                {
                    var asJObject = JObject.Parse(success);

                    var images = asJObject["items"].Children()
                        .Where(item => item["kind"].Value<string>() == "Deployment")
                        .SelectMany(deployment => {
                            var containers = deployment["spec"]["template"]["spec"]["containers"].Value<JArray>();

                            return containers
                                .Select(container => new
                                {
                                    Image = container["image"].Value<string>() ??
                                        throw new InvalidOperationException($"Missing containers->image"),
                                    ContainerName = container["name"].Value<string>() ??
                                        throw new InvalidOperationException($"Missing containers->name"),
                                    DeplymentName = deployment["metadata"]["name"].Value<string>() ??
                                        throw new InvalidOperationException($"Missing metadata->name"),
                                    NameSpace = deployment["metadata"]["namespace"].Value<string>() ??
                                        throw new InvalidOperationException($"Missing metadata->namespace")
                                });
                        })
                        .ToList();

                    images
                        .Where(image => ImageUriParser.ParseUri(image.Image).uri == parsedUri.uri).ToList()
                        .ForEach(image =>
                        {
                            _shell.Run($"kubectl set image deployment/{image.DeplymentName} {image.ContainerName}={imageUri} --namespace={image.NameSpace}");
                        });
                }, error => {
                    _logger.LogError($"Failed to fetch deployment json from kubernetes, error: {error}");
                });
        }
    }
}