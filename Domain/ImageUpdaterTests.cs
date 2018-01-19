using NSubstitute;
using Updater.Domain.TestData;
using Xunit;

namespace Updater.Domain
{
    public class ImageUpdaterTests
    {
        [Fact]
        public void WhenValidJsonIsRespondedFromCtl_ThenInvokeUpdateCommandsCorretly()
        {
            var shell = Substitute.For<ICommandLine>();
            var updater = new ImageUpdater(shell);

            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json"));

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            shell.Received(1).Run("kubectl set image deployment/authorization-master authorization-master=eu.gcr.io/ptcs-docker-registry/authorization:123-master --namespace=authorization");
        }

        [Fact]
        public void WhenValidJsonIsRespondedFromCtl_ThenOnlyUpdateMatchingDeployments()
        {
            var shell = Substitute.For<ICommandLine>();
            var updater = new ImageUpdater(shell);

            shell.Run("kubectl get deployments --all-namespaces -o json")
                .Returns(TestPathUtil.GetTestDataContent("realdata.json"));

            updater.UpdateEventHandler("eu.gcr.io/ptcs-docker-registry/authorization:123-master");

            shell.Received(1).Run(Arg.Is<string>(cmd => cmd.Contains("kubectl set image")));
        }
    }
}