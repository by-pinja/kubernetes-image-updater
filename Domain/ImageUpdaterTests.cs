using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Optional;
using Updater.Domain.TestData;
using Updater.Util;
using Xunit;

namespace Updater.Domain
{
    public class ImageUpdaterTests
    {
        [Fact]
        public void WhenValidJsonIsRespondedFromCtl_ThenInvokeUpdateCommandsCorretly()
        {
            var shell = Substitute.For<ICommandLine>();
            var updater = new ImageUpdater(shell, Substitute.For<ILogger<ImageUpdater>>(), TestUtils.CreateInMemoryContext(), TestUtils.GetAppSettings(".*master.*"));

            shell.Run(Arg.Any<string>()).Returns("result".Some<string, Exception>());
            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json").Some<string, Exception>());

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            shell.Received(1).Run("kubectl set image deployment/authorization-master authorization-master=eu.gcr.io/ptcs-docker-registry/authorization:123-master --namespace=authorization");
        }

        [Fact]
        public void WhenTagFilteringIsSet_ThenDontUpdateNonMatchingImages()
        {
            var shell = Substitute.For<ICommandLine>();
            var updater = new ImageUpdater(shell, Substitute.For<ILogger<ImageUpdater>>(), TestUtils.CreateInMemoryContext(), TestUtils.GetAppSettings(".*this_is_not_match.*"));

            shell.Run(Arg.Any<string>()).Returns("result".Some<string, Exception>());
            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json").Some<string, Exception>());

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            shell.DidNotReceive().Run(Arg.Is<string>(x => x.Contains("kubectl set image")));
        }

        [Fact]
        public void WhenValidJsonIsRespondedFromCtl_ThenOnlyUpdateMatchingDeployments()
        {
            var shell = Substitute.For<ICommandLine>();
            var updater = new ImageUpdater(shell, Substitute.For<ILogger<ImageUpdater>>(), TestUtils.CreateInMemoryContext(), TestUtils.GetAppSettings());

            shell.Run(Arg.Any<string>()).Returns("result".Some<string, Exception>());
            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json").Some<string, Exception>());

            var result = updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            shell.Received(1).Run(Arg.Is<string>(cmd => cmd.Contains("kubectl set image")));
            result.Should().Contain(x => x.Image == "eu.gcr.io/ptcs-docker-registry/authorization");
        }

        [Fact]
        public void WhenValidJsonIsRespondedFromCtl_ThenPersistUpdates()
        {
            var shell = Substitute.For<ICommandLine>();
            var db = TestUtils.CreateInMemoryContext();
            var updater = new ImageUpdater(shell, Substitute.For<ILogger<ImageUpdater>>(), db, TestUtils.GetAppSettings());

            shell.Run(Arg.Any<string>()).Returns("result".Some<string, Exception>());
            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json").Some<string, Exception>());

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            db.EventHistory.Should().Contain(x => x.Image == "eu.gcr.io/ptcs-docker-registry/authorization" && x.Tag == "123-master");
        }

        [Fact]
        public void WhenGatheringDeploymentsReturnsError_ThenLogError()
        {
            var shell = Substitute.For<ICommandLine>();
            var logger = Substitute.For<ILogger<ImageUpdater>>();
            var updater = new ImageUpdater(shell, logger, TestUtils.CreateInMemoryContext(), TestUtils.GetAppSettings());

            shell.Run(Arg.Any<string>()).Returns("result".Some<string, Exception>());
            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(Option.None<string, Exception>(new InvalidOperationException()));

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            shell.DidNotReceive().Run(Arg.Is<string>(cmd => cmd.Contains("kubectl set image")));
            logger.CheckErrorMessage();
        }

        [Fact]
        public void WhenUpdingImageReturnsError_ThenLogIt()
        {
            var shell = Substitute.For<ICommandLine>();
            var logger = Substitute.For<ILogger<ImageUpdater>>();
            var updater = new ImageUpdater(shell, logger, TestUtils.CreateInMemoryContext(), TestUtils.GetAppSettings());

            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json").Some<string, Exception>());

            shell.Run(Arg.Is<string>(cmd => cmd.Contains("kubectl set image")))
                .Returns(Option.None<string, Exception>(new InvalidOperationException()));

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");
            logger.CheckErrorMessage();
        }

    }
}