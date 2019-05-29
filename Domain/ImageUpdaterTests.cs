using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Updater.Util;
using Xunit;

namespace Updater.Domain
{
    public class ImageUpdaterTests
    {
        [Fact]
        public void WhenValidImageIsFound_ThenItWillBeUpdated()
        {
            var k8sApi = Substitute.For<IK8sApi>();
            var updater = new ImageUpdater(Substitute.For<ILogger<ImageUpdater>>(), TestUtils.GetAppSettings(), k8sApi);

            var imageInCluster = GetImageInCluster("eu.gcr.io/ptcs-docker-registry/authorization:123-master");
            k8sApi.GetImages().Returns(new[] { imageInCluster });

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            k8sApi.Received(1).ForceUpdateOfDeployment(Arg.Is<ImageInCluster>(x => x.Image == imageInCluster.Image));
        }

        private static ImageInCluster GetImageInCluster(string image)
        {
            return new ImageInCluster(image, containerName: "authorization-master", deploymentName: "authorization-master", nameSpace: "authorization", imagePullPolicy: "Always");
        }

        [Fact]
        public void WhenTagFilteringIsSet_ThenDontUpdateNonMatchingImages()
        {
            var k8sApi = Substitute.For<IK8sApi>();
            var updater = new ImageUpdater(Substitute.For<ILogger<ImageUpdater>>(), TestUtils.GetAppSettings(imageTagValidator: ".*this_is_not_match.*"), k8sApi);

            var imageInCluster = GetImageInCluster("eu.gcr.io/ptcs-docker-registry/authorization:123-master");
            k8sApi.GetImages().Returns(new [] { imageInCluster });

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            k8sApi.DidNotReceive().ForceUpdateOfDeployment(Arg.Any<ImageInCluster>());
        }

        [Fact]
        public void WhenValidJsonIsRespondedFromCtl_ThenOnlyUpdateMatchingDeployments()
        {
            var k8sApi = Substitute.For<IK8sApi>();
            var updater = new ImageUpdater(Substitute.For<ILogger<ImageUpdater>>(), TestUtils.GetAppSettings(), k8sApi);

            var matchingImageInCluster = GetImageInCluster("eu.gcr.io/ptcs-docker-registry/authorization:123-master");
            var nonMatchinImageInCluster = GetImageInCluster("node:latest");

            k8sApi.GetImages().Returns(new [] { matchingImageInCluster, nonMatchinImageInCluster });

            var result = updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            k8sApi.Received(1).ForceUpdateOfDeployment(Arg.Any<ImageInCluster>());
            result.Should().Contain(x => x.Image == "eu.gcr.io/ptcs-docker-registry/authorization");
        }
    }
}