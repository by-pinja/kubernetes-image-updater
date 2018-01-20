using System;
using Optional;

namespace Updater.Domain
{
    public interface ICommandLine
    {
        Option<string, Exception> Run(string command);
    }
}