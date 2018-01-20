using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        public static UpdaterDbContext CreateInMemoryContext()
        {
                var options = new DbContextOptionsBuilder<UpdaterDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                return new UpdaterDbContext(options);
        }
    }

}