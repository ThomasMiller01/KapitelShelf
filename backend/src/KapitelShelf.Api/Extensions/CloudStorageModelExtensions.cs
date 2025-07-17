// <copyright file="CloudStorageModelExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Utils;
using KapitelShelf.Data.Models.CloudStorage;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// The cloud storage model extensions.
/// </summary>
public static class CloudStorageModelExtensions
{
    /// <summary>
    /// Execute an rclone command with a storage.
    /// </summary>
    /// <param name="cloudStorage">The cloud storage.</param>
    /// <param name="rclonePath">The rclone executable.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The rclone output.</returns>
    /// <exception cref="InvalidOperationException">The rclone command returned an error.</exception>
    public static async Task<string> ExecuteRCloneCommand(this CloudStorageModel cloudStorage, string rclonePath, List<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(cloudStorage);

        arguments.Add("--config");
        arguments.Add(cloudStorage.RCloneConfig);

        var (exitCode, stdout, stderr) = await ProcessUtils.RunProcessAsync(rclonePath, string.Join(" ", arguments));
        if (exitCode != 0)
        {
            throw new InvalidOperationException(stderr);
        }

        return stdout;
    }
}
