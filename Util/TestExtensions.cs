using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Updater.Domain;

namespace Updater.Util
{
    public static class TestUtils
    {
        public static void CheckErrorMessage<TLogger>(this ILogger<TLogger> logger)
        {
            logger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception, string>>());
        }

        public static IOptions<AppSettings> GetAppSettings(string imageTagValidator = ".*")
        {
            return Options.Create(new AppSettings() { UpdateTagsMatching = imageTagValidator });
        }

        public static UpdaterDbContext CreateInMemoryContext()
        {
                var options = new DbContextOptionsBuilder<UpdaterDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                return new UpdaterDbContext(options);
        }
    }
}