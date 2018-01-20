using System;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Updater.Util
{
    public static class TestExtensions
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
    }
}