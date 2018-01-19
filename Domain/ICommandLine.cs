using System;
using LanguageExt;

namespace Updater.Domain
{
    public interface ICommandLine
    {
        Either<Exception, string> Run(string command);
    }
}