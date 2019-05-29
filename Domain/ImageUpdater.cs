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
        private readonly UpdaterDbContext _context;
        private readonly Regex _tagFilter;
        private readonly IK8sApi _k8sApi;

        public ImageUpdater(ILogger<ImageUpdater> logger, UpdaterDbContext context, IOptions<AppSettings> settings, IK8sApi k8sApi)
        {
            _logger = logger;
            _context = context;
            _tagFilter = new Regex(settings.Value.UpdateTagsMatching);
            _k8sApi = k8sApi;
        }

        public IEnumerable<ImageEvent> UpdateEventHandler(string newImageUri)
        {
            var parsedUri = ImageUriParser.ParseUri(newImageUri);

            return _k8sApi.GetImages()
                .Where(currentClusterImage => CheckIfImageIsApplicapleForDeployment(currentClusterImage, parsedUri))
                .Select(image => UpdateEventHistory(parsedUri, image))
                .ToList();
        }

        private bool CheckIfImageIsApplicapleForDeployment(ImageInCluster currentClusterImage, (string uri, string tag) parsedUri)
        {
            var isApplicaple = _tagFilter.IsMatch(ImageUriParser.ParseUri(currentClusterImage.Image).tag) && ImageUriParser.ParseUri(currentClusterImage.Image).uri == parsedUri.uri;

            _logger.LogDebug($"Checked {ImageUriParser.ParseUri(currentClusterImage.Image)} against {parsedUri} and {nameof(isApplicaple)} returned {isApplicaple}");

            return isApplicaple;
        }

        private ImageEvent UpdateEventHistory((string uri, string tag) imageUri, ImageInCluster image)
        {
            var entity = _context.EventHistory.Add(new ImageEvent()
            {
                Image = imageUri.uri,
                Tag = imageUri.tag,
                Stamp = DateTime.UtcNow
            }).Entity;

            _context.SaveChanges();

            _logger.LogInformation($"Updated image 'deployment/{image.DeploymentName} {image.ContainerName}={imageUri.uri}:{imageUri.tag} --namespace={image.NameSpace}'");

            return entity;
        }
    }
}