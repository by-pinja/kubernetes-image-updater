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
            var updater = new ImageUpdater(Substitute.For<ILogger<ImageUpdater>>(), TestUtils.CreateInMemoryContext(), TestUtils.GetAppSettings(), k8sApi);

            var imageInCluster = new ImageInCluster("eu.gcr.io/ptcs-docker-registry/authorization:123-master", containerName: "authorization-master", deploymentName: "authorization-master", nameSpace: "authorization" );
            k8sApi.GetImages().Returns(new [] { imageInCluster });

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:999-master");

            k8sApi.Received(1).SetImage(Arg.Is<ImageInCluster>(x => x.Image == imageInCluster.Image), "eu.gcr.io/ptcs-docker-registry/authorization:999-master");
        }

        [Fact]
        public void WhenTagFilteringIsSet_ThenDontUpdateNonMatchingImages()
        {
            var k8sApi = Substitute.For<IK8sApi>();
            var updater = new ImageUpdater(Substitute.For<ILogger<ImageUpdater>>(), TestUtils.CreateInMemoryContext(), TestUtils.GetAppSettings(imageTagValidator: ".*this_is_not_match.*"), k8sApi);

            var imageInCluster = new ImageInCluster("eu.gcr.io/ptcs-docker-registry/authorization:123-master", containerName: "authorization-master", deploymentName: "authorization-master", nameSpace: "authorization" );
            k8sApi.GetImages().Returns(new [] { imageInCluster });

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            k8sApi.DidNotReceive().SetImage(Arg.Any<ImageInCluster>(), Arg.Any<string>());
        }

        [Fact]
        public void WhenValidJsonIsRespondedFromCtl_ThenOnlyUpdateMatchingDeployments()
        {
            var k8sApi = Substitute.For<IK8sApi>();
            var updater = new ImageUpdater(Substitute.For<ILogger<ImageUpdater>>(), TestUtils.CreateInMemoryContext(), TestUtils.GetAppSettings(), k8sApi);

            var matchingImageInCluster = new ImageInCluster("eu.gcr.io/ptcs-docker-registry/authorization:123-master", containerName: "authorization-master", deploymentName: "authorization-master", nameSpace: "authorization" );
            var nonMatchinImageInCluster = new ImageInCluster("node:latest", containerName: "authorization-master", deploymentName: "authorization-master", nameSpace: "authorization" );

            k8sApi.GetImages().Returns(new [] { matchingImageInCluster, nonMatchinImageInCluster });

            var result = updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            k8sApi.Received(1).SetImage(Arg.Any<ImageInCluster>(), Arg.Any<string>());
            result.Should().Contain(x => x.Image == "eu.gcr.io/ptcs-docker-registry/authorization");
        }

        [Fact]
        public void WhenImageIsUpdated_ThenPersistUpdates()
        {
            var db = TestUtils.CreateInMemoryContext();
            var k8sApi = Substitute.For<IK8sApi>();
            var updater = new ImageUpdater(Substitute.For<ILogger<ImageUpdater>>(), db, TestUtils.GetAppSettings(".*"), k8sApi);

            var imageInCluster = new ImageInCluster("eu.gcr.io/ptcs-docker-registry/authorization:123-master", containerName: "authorization-master", deploymentName: "authorization-master", nameSpace: "authorization" );
            k8sApi.GetImages().Returns(new [] { imageInCluster });

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            db.EventHistory.Should().Contain(x => x.Image == "eu.gcr.io/ptcs-docker-registry/authorization" && x.Tag == "123-master");
        }
    }
}