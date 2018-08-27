using System.IO;

namespace Updater.Domain.TestData
{
    public static class TestPathUtil
    {
        public static string GetTestDataContent(string fileName)
        {
            return GetContent("Domain/TestData", fileName);
        }

        private static string GetContent(string subdir, string fileName)
        {
            var pathToJson =
                    Path.GetFullPath(Path.Combine(
                        Path.GetDirectoryName(typeof(TestPathUtil).Assembly.Location), $"../../../{subdir}/{fileName}"));

            var satelTestNetworkFile = File.ReadAllText(pathToJson);
            return satelTestNetworkFile;
        }
    }
}