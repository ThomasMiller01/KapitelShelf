// <copyright file="CloudStorageExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using KapitelShelf.Api.Utils;
using KapitelShelf.Data.Models.CloudStorage;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// The cloud storage model extensions.
/// </summary>
public static class CloudStorageExtensions
{
    /// <summary>
    /// Execute an rclone command with a storage.
    /// </summary>
    /// <param name="cloudStorage">The cloud storage.</param>
    /// <param name="rclonePath">The rclone executable.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="processUtils">The process utils.</param>
    /// <param name="onStdout">Called when stdout gets written to.</param>
    /// <param name="onStderr">Called when stderr gets written to.</param>
    /// <param name="onProcessStarted">Called once the process started.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rclone output.</returns>
    /// <exception cref="InvalidOperationException">The rclone command returned an error.</exception>
    public static async Task<string> ExecuteRCloneCommand(
        this CloudStorageModel cloudStorage,
        string rclonePath,
        List<string> arguments,
        IProcessUtils processUtils,
        Action<string>? onStdout = null,
        Action<string>? onStderr = null,
        Action<Process>? onProcessStarted = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cloudStorage);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(processUtils);

        arguments.Add("--config");
        arguments.Add($"\"{cloudStorage.RCloneConfig}\"");

        var (exitCode, stdout, stderr) = await processUtils.RunProcessAsync(
            rclonePath,
            string.Join(" ", arguments),
            onStdout: onStdout,
            onStderr: onStderr,
            onProcessStarted: onProcessStarted,
            cancellationToken: cancellationToken);

        if (exitCode != 0)
        {
            throw new InvalidOperationException(stderr);
        }

        return stdout;
    }
}
