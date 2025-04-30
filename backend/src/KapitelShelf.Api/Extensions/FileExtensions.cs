// <copyright file="FileExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

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
        if (!provider.TryGetContentType(file.Name, out var contentType))
        {
            contentType = DEFAULT_CONTENT_TYPE;
        }

        return contentType;
    }
}
