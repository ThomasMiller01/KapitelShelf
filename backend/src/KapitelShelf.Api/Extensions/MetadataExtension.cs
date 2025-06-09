// <copyright file="MetadataExtension.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.MetadataScraper;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// Metadata extension methods.
/// </summary>
public static class MetadataExtension
{
    /// <summary>
    /// Convert a metadata source to its string representation.
    /// </summary>
    /// <param name="source">The metadata source.</param>
    /// <returns>The metadata source name.</returns>
    public static string? ToSourceName(this MetadataSources source) => Enum.GetName(source.GetType(), source);
}
