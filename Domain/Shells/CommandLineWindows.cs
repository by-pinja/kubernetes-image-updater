using System;
using System.Diagnostics;
using LanguageExt;
using Updater.Domain;

namespace Updater.Domain
{
    public class CommandLineWindows : ICommandLine
    {
        public Either<Exception, string> Run(string command)
        {
            if(command == null)
                throw new ArgumentNullException(nameof(command));

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var proc = new Process
            {
                StartInfo = psi
            };

            proc.Start();
            proc.WaitForExit(5000);

            if (!proc.HasExited)
            {
                return new InvalidOperationException($"Command '{command}' failed to exit during timeout, likely command hangs.");
            }

            var error = proc.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(error))
                return new InvalidOperationException($"Failed to run commandline command, error: '{error}'");

            var output = proc.StandardOutput.ReadToEnd();

            return output;
        }
    }
}