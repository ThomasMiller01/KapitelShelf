// <copyright file="StorageBase.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using KapitelShelf.Api.Settings;

namespace KapitelShelf.Api.Logic.Storage;

/// <summary>
/// Interface with the filesystem to retrieve and store book files.
/// </summary>
/// <param name="settings">The settings.</param>
public class StorageBase(KapitelShelfSettings settings) : IStorageBase
{
#pragma warning disable SA1401 // Fields should be private
    internal readonly KapitelShelfSettings Settings = settings;
#pragma warning restore SA1401 // Fields should be private

    /// <inheritdoc/>
    public FileStream? Stream(FileInfoDTO file)
    {
        ArgumentNullException.ThrowIfNull(file);

        return Stream(file.FilePath);
    }

    /// <inheritdoc/>
    public void DeleteFile(string filePath)
    {
        var fullFilePath = FullPath(filePath);
        if (!File.Exists(fullFilePath))
        {
            return;
        }

        File.Delete(fullFilePath);
    }

    /// <inheritdoc/>
    public async Task<FileInfoDTO> Save(string filePath, IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var fullFilePath = FullPath(filePath);

        var directory = Path.GetDirectoryName(fullFilePath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = new FileStream(fullFilePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Reset stream position before computing hash
        stream.Seek(0, SeekOrigin.Begin);

        return new FileInfoDTO
        {
            FilePath = filePath,
            FileSizeBytes = file.Length,
            MimeType = file.GetMimeType(),
            Sha256 = stream.Checksum(),
        };
    }

    /// <inheritdoc/>
    public FileStream? Stream(string filePath)
    {
        var fullFilePath = FullPath(filePath);
        if (!File.Exists(fullFilePath))
        {
            return null;
        }

        return File.OpenRead(fullFilePath);
    }

    /// <inheritdoc/>
    public void DeleteDirectory(string directoryPath)
    {
        var fullDirectoryPath = FullPath(directoryPath);
        if (!Directory.Exists(fullDirectoryPath))
        {
            return;
        }

        Directory.Delete(fullDirectoryPath, recursive: true);
    }

    /// <inheritdoc/>
    public string FullPath(string subPath)
    {
        var combinedPath = Path.Combine(this.Settings.DataDir, subPath);
        var absPath = Path.GetFullPath(combinedPath);
        return absPath;
    }
}
