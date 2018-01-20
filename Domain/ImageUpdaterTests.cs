using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
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
            var updater = new ImageUpdater(shell, Substitute.For<ILogger<ImageUpdater>>(), TestUtils.CreateInMemoryContext());

            shell.Run(Arg.Any<string>()).Returns("result");
            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json"));

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            shell.Received(1).Run("kubectl set image deployment/authorization-master authorization-master=eu.gcr.io/ptcs-docker-registry/authorization:123-master --namespace=authorization");
        }

        [Fact]
        public void WhenValidJsonIsRespondedFromCtl_ThenOnlyUpdateMatchingDeployments()
        {
            var shell = Substitute.For<ICommandLine>();
            var updater = new ImageUpdater(shell, Substitute.For<ILogger<ImageUpdater>>(), TestUtils.CreateInMemoryContext());

            shell.Run(Arg.Any<string>()).Returns("result");
            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json"));

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            shell.Received(1).Run(Arg.Is<string>(cmd => cmd.Contains("kubectl set image")));
        }

        [Fact]
        public void WhenValidJsonIsRespondedFromCtl_ThenPersistUpdates()
        {
            var shell = Substitute.For<ICommandLine>();
            var db = TestUtils.CreateInMemoryContext();
            var updater = new ImageUpdater(shell, Substitute.For<ILogger<ImageUpdater>>(), db);

            shell.Run(Arg.Any<string>()).Returns("result");
            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json"));

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            db.EventHistory.Should().Contain(x => x.Image == "eu.gcr.io/ptcs-docker-registry/authorization" && x.Tag == "123-master");
        }

        [Fact]
        public void WhenGatheringDeploymentsReturnsError_ThenLogError()
        {
            var shell = Substitute.For<ICommandLine>();
            var logger = Substitute.For<ILogger<ImageUpdater>>();
            var updater = new ImageUpdater(shell, logger, TestUtils.CreateInMemoryContext());

            shell.Run(Arg.Any<string>()).Returns("result");
            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(new InvalidOperationException());

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            shell.DidNotReceive().Run(Arg.Is<string>(cmd => cmd.Contains("kubectl set image")));
            logger.CheckErrorMessage();
        }

        [Fact]
        public void WhenUpdingImageReturnsError_ThenLogIt()
        {
            var shell = Substitute.For<ICommandLine>();
            var logger = Substitute.For<ILogger<ImageUpdater>>();
            var updater = new ImageUpdater(shell, logger, TestUtils.CreateInMemoryContext());

            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json"));

            shell.Run(Arg.Is<string>(cmd => cmd.Contains("kubectl set image")))
                .Returns(new InvalidOperationException());

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");
            logger.CheckErrorMessage();
        }
    }
}