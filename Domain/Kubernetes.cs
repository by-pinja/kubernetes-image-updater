using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Updater.Domain
{
    public class K8sApi
    {
        private readonly Lazy<Kubernetes> _kubernetesClient;
        private readonly ILogger<Kubernetes> _logger;
        private readonly IOptions<AppSettings> _settings;

        public K8sApi(ILogger<Kubernetes> logger, IOptions<AppSettings> settings)
        {
            _logger = logger;
            _settings = settings;
            _kubernetesClient = new Lazy<Kubernetes>(() => new Kubernetes(GetConfig()));
        }

        public IEnumerable<ImageInCluster> GetImages()
        {
            var pods = _kubernetesClient.Value.ListDeploymentForAllNamespaces();

            var pids = pods.Items
                .SelectMany(x => x.Spec.Template.Spec.Containers, (parentDeployment, container) => (parentDeployment: parentDeployment, container: container))
                .Where(x => !x.container.Image.Contains("/google_containers/") && !x.container.Image.Contains("/google-containers/"))
                .Distinct();

            return pids.Select(x => new ImageInCluster(x.container.Image, x.container.Name, x.parentDeployment.Metadata.Name, x.parentDeployment.Metadata.NamespaceProperty));
        }

        public void SetImage(ImageInCluster inClusterImage, string newImage)
        {
            var patch = new JsonPatchDocument<V1Deployment>();
            patch.Replace(x => x.Spec.Template.Spec.Containers.Single(c => c.Name == inClusterImage.ContainerName).Image, newImage);
            _kubernetesClient.Value.PatchNamespacedDeployment(new V1Patch(patch), inClusterImage.DeploymentName, inClusterImage.NameSpace);
        }

        private KubernetesClientConfiguration GetConfig()
        {
            if(!string.IsNullOrEmpty(_settings.Value.ProxyAddress))
                return new KubernetesClientConfiguration { Host = _settings.Value.ProxyAddress };

            _logger.LogInformation($"Proxy not set, using {nameof(KubernetesClientConfiguration.InClusterConfig)}");
            return KubernetesClientConfiguration.InClusterConfig();
        }
    }
}