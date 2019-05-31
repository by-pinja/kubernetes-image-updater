using System.Collections.Generic;

namespace Updater.Domain
{
    public interface IK8sApi
    {
        IEnumerable<ImageInCluster> GetImages();
        ImageInCluster ForceUpdateOfDeployment(ImageInCluster inClusterImage);
    }
}