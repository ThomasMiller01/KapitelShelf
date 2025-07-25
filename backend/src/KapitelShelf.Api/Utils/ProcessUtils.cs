// <copyright file="ProcessUtils.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Text;

namespace KapitelShelf.Api.Utils;

/// <summary>
/// The process utils.
/// </summary>
public class ProcessUtils : IProcessUtils
{
    /// <inheritdoc/>
    public async Task<(int ExitCode, string Stdout, string Stderr)> RunProcessAsync(
        string process,
        string arguments,
        string? workingDirectory = null,
        Action<string>? onStdout = null,
        Action<string>? onStderr = null,
        string? stdoutSeperator = null,
        Action<Process>? onProcessStarted = null,
        CancellationToken cancellationToken = default)
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

        processObj.Start();
        onProcessStarted?.Invoke(processObj);

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        var stdoutTask = Task.Run(
            async () =>
            {
                var sb = new StringBuilder();
                var buffer = new char[1];
                using var reader = processObj.StandardOutput;
                while (true)
                {
                    int read = await reader.ReadAsync(buffer, 0, 1);
                    if (read == 0)
                    {
                        break;
                    }

                    char ch = buffer[0];

                    var isLineEnding = ch is '\r' or '\n';
                    var isSeperator = stdoutSeperator is not null && sb.ToString().EndsWith(stdoutSeperator, StringComparison.InvariantCulture);

                    if (isLineEnding || isSeperator)
                    {
                        if (sb.Length > 0)
                        {
                            stdout.AppendLine(sb.ToString());
                            onStdout?.Invoke(sb.ToString());

                            sb.Clear();
                        }

                        // Handle possible \r\n sequence: skip \n if it immediately follows \r
                        if (ch == '\r')
                        {
                            // peek ahead to see if the next char is '\n'
                            int next = reader.Peek();
                            if (next == '\n')
                            {
                                await reader.ReadAsync(buffer, 0, 1); // consume the \n
                            }
                        }
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }

                // Final flush if something left
                if (sb.Length > 0)
                {
                    stdout.AppendLine(sb.ToString());
                    onStdout?.Invoke(sb.ToString());
                }
            },
            cancellationToken);

        var stderrTask = Task.Run(
            async () =>
            {
                using var reader = processObj.StandardError;
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    stderr.AppendLine(line);
                    onStderr?.Invoke(line);
                }
            },
            cancellationToken);

        await Task.WhenAll(stdoutTask, stderrTask, processObj.WaitForExitAsync(cancellationToken));

        return (processObj.ExitCode, stdout.ToString(), stderr.ToString());
    }
}

/// <summary>
/// The process wrapper.
/// </summary>
public class ProcessWrapper(Process process) : IProcess
{
    private readonly Process process = process;

    /// <inheritdoc/>
    public bool HasExited => this.process.HasExited;

    /// <inheritdoc/>
    public void Kill(bool entireProcessTree) => this.process.Kill(entireProcessTree);
}
