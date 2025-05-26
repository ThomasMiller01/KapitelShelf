// <copyright file="MetadataScraperDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.MetadataScraper;

/// <summary>
/// The dto for metadata scrape results, extending the basic metadata dto with additional scores.
/// </summary>
public class MetadataScraperDTO : MetadataDTO
{
    /// <summary>
    /// Gets or sets the score for how well the title matches the search query.
    /// </summary>
    public int TitleMatchScore { get; set; } = 0;

    /// <summary>
    /// Gets or sets the score for how complete the metadata is.
    /// </summary>
    public int CompletenessScore { get; set; } = 0;
}
