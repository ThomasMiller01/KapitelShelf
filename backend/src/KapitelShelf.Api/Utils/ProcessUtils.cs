// <copyright file="ProcessUtils.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Text;

namespace KapitelShelf.Api.Utils;

/// <summary>
/// The process utils.
/// </summary>
public static class ProcessUtils
{
    /// <summary>
    /// Executes an external process asynchronously and captures stdout/stderr.
    /// </summary>
    /// <param name="process">The executable to run (e.g., "rclone", "bash", "cmd.exe").</param>
    /// <param name="arguments">Command-line arguments for the process.</param>
    /// <param name="workingDirectory">Optional working directory for the process.</param>
    /// <returns>Exit code, standard output, and standard error.</returns>
    public static async Task<(int ExitCode, string Stdout, string Stderr)> RunProcessAsync(
        string process,
        string arguments,
        string? workingDirectory = null)
    {
        var processObj = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = process,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? string.Empty,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            },
        };

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        processObj.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null)
            {
                stdout.AppendLine(e.Data);
            }
        };

        processObj.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null)
            {
                stderr.AppendLine(e.Data);
            }
        };

        processObj.Start();
        processObj.BeginOutputReadLine();
        processObj.BeginErrorReadLine();

        await processObj.WaitForExitAsync();

        return (processObj.ExitCode, stdout.ToString(), stderr.ToString());
    }
}
