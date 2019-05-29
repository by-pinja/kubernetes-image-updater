using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Updater.Domain
{
    public class ImageUpdater
    {
        private readonly ILogger<ImageUpdater> _logger;
        private readonly Regex _tagFilter;
        private readonly IK8sApi _k8sApi;

        public ImageUpdater(ILogger<ImageUpdater> logger, IOptions<AppSettings> settings, IK8sApi k8sApi)
        {
            _logger = logger;
            _tagFilter = new Regex(settings.Value.UpdateTagsMatching);
            _k8sApi = k8sApi;
        }

        public IEnumerable<ImageEvent> UpdateEventHandler(string imageToUpdateUri)
        {
            var parsedUri = ImageUriParser.ParseUri(imageToUpdateUri);

            var imagesToUpdate = _k8sApi.GetImages()
                .Where(currentClusterImage => CheckIfImageIsApplicapleForDeployment(currentClusterImage, parsedUri));

            foreach(var imageToUpdate in imagesToUpdate)
            {
                _k8sApi.ForceUpdateOfDeployment(imageToUpdate);
                _logger.LogInformation($"Updated image 'deployment/{imageToUpdate.DeploymentName} {imageToUpdate.ContainerName}={parsedUri.uri}:{parsedUri.tag} --namespace={imageToUpdate.NameSpace}'");
            }

            return imagesToUpdate.Select(image => new ImageEvent()
            {
                Image = parsedUri.uri,
                Tag = parsedUri.tag,
                Deployment = image.DeploymentName,
                NameSpace = image.NameSpace,
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Message = ResolveMessage(image)
            });
        }

        private string ResolveMessage(ImageInCluster image)
        {
            if(image.ImagePullPolicy == "Always")
            {
                return "Updated";
            }

            return $"Image pull policy was '{image.ImagePullPolicy}', expected to it be 'Always'. This likely causes image not to be updated as expected.";
        }

        private bool CheckIfImageIsApplicapleForDeployment(ImageInCluster currentClusterImage, (string uri, string tag) parsedUri)
        {
            var isApplicaple = _tagFilter.IsMatch(parsedUri.tag)
                && ImageUriParser.ParseUri(currentClusterImage.Image).uri == parsedUri.uri
                && ImageUriParser.ParseUri(currentClusterImage.Image).tag == parsedUri.tag;

            _logger.LogDebug($"Checked {ImageUriParser.ParseUri(currentClusterImage.Image)} against {parsedUri} and {nameof(isApplicaple)} returned {isApplicaple}");

            return isApplicaple;
        }
    }
}