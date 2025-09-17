// <copyright file="IProcessUtils.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;

namespace KapitelShelf.Api.Utils;

/// <summary>
/// The process utils interface.
/// </summary>
public interface IProcessUtils
{
    /// <summary>
    /// Executes an external process asynchronously and captures stdout/stderr.
    /// </summary>
    /// <param name="process">The executable to run (e.g., "rclone", "bash", "cmd.exe").</param>
    /// <param name="arguments">Command-line arguments for the process.</param>
    /// <param name="workingDirectory">Optional working directory for the process.</param>
    /// <param name="onStdout">Called when stdout gets written to.</param>
    /// <param name="onStderr">Called when stderr gets written to.</param>
    /// <param name="onProcessStarted">Called once the process is started.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Exit code, standard output, and standard error.</returns>
    Task<(int ExitCode, string Stdout, string Stderr)> RunProcessAsync(
        string process,
        string arguments,
        string? workingDirectory = null,
        Action<string>? onStdout = null,
        Action<string>? onStderr = null,
        Action<Process>? onProcessStarted = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The interface for a proces.
/// </summary>
public interface IProcess
{
    /// <summary>
    /// Gets a value indicating whether the process has exited.
    /// </summary>
    bool HasExited { get; }

    /// <summary>
    /// Kill the process.
    /// </summary>
    /// <param name="entireProcessTree">A valud indicating whether to kill the entire process tree.</param>
    void Kill(bool entireProcessTree);
}
