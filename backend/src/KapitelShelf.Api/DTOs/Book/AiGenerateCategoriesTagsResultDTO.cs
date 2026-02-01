// <copyright file="AiGenerateCategoriesTagsResultDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Book;

/// <summary>
/// The ai generate categories and tags result dto.
/// </summary>
public class AiGenerateCategoriesTagsResultDTO
{
    /// <summary>
    /// Gets or sets the categories.
    /// </summary>
    public List<string> Categories { get; set; } = null!;

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public List<string> Tags { get; set; } = null!;
}
