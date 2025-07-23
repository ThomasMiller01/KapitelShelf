// <copyright file="FileExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using KapitelShelf.Api.DTOs.FileInfo;
using Microsoft.AspNetCore.StaticFiles;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// IFileInfo extension methods.
/// </summary>
public static class FileExtensions
{
    private const string DEFAULT_CONTENT_TYPE = "application/unkown";

    /// <summary>
    /// Get the mimetype from the file extension.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>The file mimetype.</returns>
    public static string GetMimeType(this IFormFile file)
    {
        if (file is null)
        {
            return DEFAULT_CONTENT_TYPE;
        }

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(file.FileName, out var contentType))
        {
            contentType = DEFAULT_CONTENT_TYPE;
        }

        return contentType;
    }

    /// <summary>
    /// Calculate the SHA256 checksum of a file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>The SHA256 checksum.</returns>
    public static string Checksum(this IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        using var stream = file.OpenReadStream();
        return stream.Checksum();
    }

    /// <summary>
    /// Calculate the SHA256 checksum of a file.
    /// </summary>
    /// <param name="stream">The file stream.</param>
    /// <returns>The SHA256 checksum.</returns>
    public static string Checksum(this Stream stream)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// Convert bytes to a file.
    /// </summary>
    /// <param name="bytes">The bytes to convert into the file.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The file.</returns>
    public static IFormFile ToFile(this byte[] bytes, string fileName)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }

    /// <summary>
    /// Read a file from the filePath.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file.</returns>
    public static IFormFile ToFile(this string filePath)
    {
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var fileInfo = new FileInfo(filePath);
        return new FormFile(fileStream, 0, fileInfo.Length, "file", fileInfo.Name);
    }

    /// <summary>
    /// Convert a form file to a file info..
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file info.</returns>
    public static FileInfoDTO ToFileInfo(this IFormFile file, string filePath)
    {
        ArgumentNullException.ThrowIfNull(file);

        return new FileInfoDTO
        {
            FilePath = filePath,
            FileSizeBytes = file.Length,
            MimeType = file.GetMimeType(),
            Sha256 = file.Checksum(),
        };
    }
}
