using System;
using Newtonsoft.Json.Linq;

namespace Updater.Domain
{
    public class ImageInCluster
    {
        private ImageInCluster() { }
        public ImageInCluster(string image, string containerName, string deploymentName, string nameSpace)
        {
            NameSpace = nameSpace;
            DeploymentName = deploymentName;
            ContainerName = containerName;
            Image = image;
        }

        public string Image { get; private set; }
        public string ContainerName { get; private set; }
        public string DeploymentName { get; private set; }
        public string NameSpace { get; private set; }

        public static ImageInCluster Create(JToken container, JToken deployment)
        {
            return new ImageInCluster
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
}