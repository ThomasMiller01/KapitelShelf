// <copyright file="Mapper.Metadata.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Api.DTOs.MetadataScraper;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The metadata mappers.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a metadata dto to a create author dto.
    /// </summary>
    /// <param name="metadata">The metadata dto.</param>
    /// <returns>The create author dto.</returns>
    public CreateAuthorDTO? MetadataDtoToCreateAuthorDto(MetadataDTO metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        if (metadata.Authors.Count == 0)
        {
            return null;
        }

        var (firstName, lastName) = SplitName(metadata.Authors[0]);
        return new CreateAuthorDTO
        {
            FirstName = firstName,
            LastName = lastName,
        };
    }

    /// <summary>
    /// Map a metadata source to a location type dto.
    /// </summary>
    /// <param name="source">The metadata source.</param>
    /// <returns>The location type dto.</returns>
    public LocationTypeDTO MetadataSourceToLocationTypeDto(MetadataSources source)
    {
        return source switch
        {
            MetadataSources.Amazon => LocationTypeDTO.Kindle,
            MetadataSources.GoogleBooks => LocationTypeDTO.KapitelShelf,
            MetadataSources.OpenLibrary => LocationTypeDTO.KapitelShelf,
            _ => LocationTypeDTO.KapitelShelf,
        };
    }
}
