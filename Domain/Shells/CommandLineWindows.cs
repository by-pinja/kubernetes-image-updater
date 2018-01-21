using System;
using System.Diagnostics;
using System.IO;
using Optional;
using Updater.Domain;
using Updater.Util;

namespace Updater.Domain
{
    public class CommandLineWindows : ICommandLine
    {
        public Option<string, Exception> Run (string command)
        {
            if (command == null)
                throw new ArgumentNullException (nameof (command));

            // Important to notice: this is workaround...
            // When cmd returns large json, there is probably some control characters or something in stream
            // basically without this any larger JSON response locks terminal and nothing will get ever returned.
            // Which of course causes timeout error after some waiting.
            var tempFileLocation = GetTempFilePath();

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command} > {tempFileLocation}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var proc = new Process
            {
                StartInfo = psi
            };

            proc.Start ();
            proc.WaitForExit (30*1000);

            if (!proc.HasExited)
            {
                return $"Command '{command}' failed to exit during timeout, likely command hangs.".AsInvalidOperation<string> ();
            }

            var error = proc.StandardError.ReadToEnd ();

            if (!string.IsNullOrEmpty (error))
                return $"Failed to run commandline command, error: '{error}'".AsInvalidOperation<string> ();

            var output = File.ReadAllText(tempFileLocation);
            File.Delete(tempFileLocation);

            return output.Some<string, Exception> ();
        }

        public static string GetTempFilePath ()
        {
            var path = Path.GetTempPath ();
            var fileName = Guid.NewGuid ().ToString () + ".json";
            return Path.Combine (path, fileName);
        }
    }
}