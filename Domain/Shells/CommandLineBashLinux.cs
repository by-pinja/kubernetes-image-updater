using System;
using System.Diagnostics;
using LanguageExt;

namespace Updater.Domain
{
    public class CommandLineBashLinux : ICommandLine
    {
        public Either<Exception, string> Run(string command)
        {
            if(command == null)
                throw new ArgumentNullException(nameof(command));

            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"",
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