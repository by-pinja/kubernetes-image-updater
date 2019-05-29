using System;
using System.Collections.Generic;
using System.Linq;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Updater.Domain
{
    public class K8sApi : IK8sApi
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
                .SelectMany(x => x.Spec.Template.Spec.Containers.Select((container, idx) => (idx, container)), (parentDeployment, child) => (parentDeployment, child.container, index: child.idx))
                .Where(x => !x.container.Image.Contains("/google_containers/") && !x.container.Image.Contains("/google-containers/"))
                .Distinct();

            return pids.Select(x => new ImageInCluster(x.container.Image, x.container.Name, x.parentDeployment.Metadata.Name, x.parentDeployment.Metadata.NamespaceProperty, x.container.ImagePullPolicy)
            {
                ContainerIndex = x.index
            });
        }

        // This updates metadata of deployment and therefore reloads deployment and it's images as latest.
        public ImageInCluster ForceUpdateOfDeployment(ImageInCluster inClusterImage)
        {
            var patch = new JsonPatchDocument<V1Deployment>();
            patch.Replace(x => x.Spec.Template.Metadata.Labels["kubernetes-imageupdater-updated-timestamp"], DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
            _kubernetesClient.Value.PatchNamespacedDeployment(new V1Patch(patch), inClusterImage.DeploymentName, inClusterImage.NameSpace);
            return inClusterImage;
        }

        private KubernetesClientConfiguration GetConfig()
        {
            if (!string.IsNullOrEmpty(_settings.Value.K8sProxyAddress))
                return new KubernetesClientConfiguration { Host = _settings.Value.K8sProxyAddress };

            _logger.LogInformation($"Proxy not set, using {nameof(KubernetesClientConfiguration.InClusterConfig)}");
            return KubernetesClientConfiguration.InClusterConfig();
        }
    }
}