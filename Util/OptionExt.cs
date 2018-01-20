using System;
using Optional;

namespace Updater.Util
{
    public static class OptionExt
    {
        public static Option<T, Exception> AsInvalidOperation<T>(this string errorMessage)
        {
            return Option.None<T, Exception>(new InvalidOperationException(errorMessage));
        }
    }
}