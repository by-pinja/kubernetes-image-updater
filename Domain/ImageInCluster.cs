namespace Updater.Domain
{
    public class ImageInCluster
    {
        public ImageInCluster(string image, string containerName, string deploymentName, string nameSpace)
        {
            NameSpace = nameSpace;
            DeploymentName = deploymentName;
            ContainerName = containerName;
            Image = image;
        }

        public string Image { get; }
        public int ContainerIndex { get; set; }
        public string ContainerName { get; }
        public string DeploymentName { get; }
        public string NameSpace { get; }
    }
}