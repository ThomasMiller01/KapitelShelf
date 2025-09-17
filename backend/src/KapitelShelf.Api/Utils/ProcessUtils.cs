// <copyright file="ProcessUtils.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using CliWrap;
using CliWrap.EventStream;

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
        Action<Process>? onProcessStarted = null,
        CancellationToken cancellationToken = default)
    {
        var stdout = new System.Text.StringBuilder();
        var stderr = new System.Text.StringBuilder();

        var cmd = Cli.Wrap(process)
            .WithArguments(arguments)
            .WithWorkingDirectory(workingDirectory ?? Environment.CurrentDirectory)
            .WithValidation(CommandResultValidation.None); // dont throw on non-zero exit code

        await foreach (var cmdEvent in cmd.ListenAsync(cancellationToken))
        {
            switch (cmdEvent)
            {
                case StandardOutputCommandEvent stdOut:
                    stdout.AppendLine(stdOut.Text);
                    onStdout?.Invoke(stdOut.Text);
                    break;

                case StandardErrorCommandEvent stdErr:
                    stderr.AppendLine(stdErr.Text);
                    onStderr?.Invoke(stdErr.Text);
                    break;

                case ExitedCommandEvent exited:
                    return (exited.ExitCode, stdout.ToString(), stderr.ToString());

                default:
                    break;
            }
        }

        // fallback
        return (-1, stdout.ToString(), stderr.ToString());
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
