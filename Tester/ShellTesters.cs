using Updater.Domain;
using Xunit;
using System.Linq;

namespace Updater.Tester
{
    public class Shell
    {
        [Fact(Skip="Tester")]
        public void WindowsShellTester()
        {
            var shell = new CommandLineWindows();
            var result = shell.Run("kubectl get deployments --all-namespaces -o json");
        }
    }
}