using System.Collections.Generic;

namespace Updater.Domain
{
    public interface IK8sApi
    {
        IEnumerable<ImageInCluster> GetImages();
        ImageInCluster SetImage(ImageInCluster inClusterImage, string newImage);
    }
}