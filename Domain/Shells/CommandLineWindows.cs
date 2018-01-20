using System;
using System.Diagnostics;
using Optional;
using Updater.Domain;
using Updater.Util;

namespace Updater.Domain
{
    public class CommandLineWindows : ICommandLine
    {
        public Option<string, Exception> Run(string command)
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
                return $"Command '{command}' failed to exit during timeout, likely command hangs.".AsInvalidOperation<string>();
            }

            var error = proc.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(error))
                return $"Failed to run commandline command, error: '{error}'".AsInvalidOperation<string>();

            var output = proc.StandardOutput.ReadToEnd();

            return output.Some<string, Exception>();
        }
    }
}