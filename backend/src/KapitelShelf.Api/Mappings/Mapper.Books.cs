// <copyright file="Mapper.Books.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Data.Models;
using Riok.Mapperly.Abstractions;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The books mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a book model to a book dto.
    /// </summary>
    /// <param name="model">The book model.</param>
    /// <returns>The book dto.</returns>
    [MapperIgnoreSource(nameof(BookModel.Categories))]
    [MapperIgnoreSource(nameof(BookModel.Tags))]
    [MapperIgnoreSource(nameof(BookModel.SeriesId))]
    [MapperIgnoreSource(nameof(BookModel.AuthorId))]
    [MapperIgnoreTarget(nameof(BookDTO.Categories))]
    [MapperIgnoreTarget(nameof(BookDTO.Tags))]
    public partial BookDTO BookModelToBookDto(BookModel model);

    /// <summary>
    /// Map a create book dto to a book model.
    /// </summary>
    /// <param name="dto">The create dto.</param>
    /// <returns>The book model.</returns>
    [MapperIgnoreTarget(nameof(BookModel.Categories))]
    [MapperIgnoreTarget(nameof(BookModel.Tags))]
    [MapperIgnoreTarget(nameof(BookModel.Series))]
    [MapperIgnoreTarget(nameof(BookModel.Author))]
    [MapperIgnoreTarget(nameof(BookModel.Cover))]
    public partial BookModel CreateBookDtoToBookModel(CreateBookDTO dto);

    /// <summary>
    /// Map a book dto to a create book dto.
    /// </summary>
    /// <param name="dto">The book dto.</param>
    /// <returns>The create dto.</returns>
    [MapperIgnoreSource(nameof(BookDTO.Id))]
    [MapperIgnoreSource(nameof(BookDTO.Cover))]
    public partial CreateBookDTO BookDtoToCreateBookDto(BookDTO dto);

    /// <summary>
    /// Map a book search view to a book dto.
    /// </summary>
    /// <param name="view">The book search view.</param>
    /// <returns>The book dto.</returns>
    public partial BookDTO BookSearchViewToBookDto(BookSearchView view);

    /// <summary>
    /// Map a metadata dto to a create book dto.
    /// </summary>
    /// <param name="dto">The metadata dto.</param>
    /// <returns>The create book dto.</returns>
    public partial CreateBookDTO MetadataDtoToCreateBookDto(MetadataDTO dto);

    /// <summary>
    /// Map a book model to a book dto.
    /// </summary>
    /// <param name="source">The book model.</param>
    /// <param name="target">The book dto.</param>
    [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Mapperly User-Implemented")]
    private void BookModelToBookDto(BookModel source, BookDTO target)
    {
        // categories
        target.Categories = source.Categories
            .Select(x => this.CategoryModelToCategoryDto(x.Category))
            .ToList();

        // tags
        target.Tags = source.Tags
            .Select(x => this.TagModelToTagDto(x.Tag))
            .ToList();

        // series: use full series dto if present, otherwise fallback to { Id = SeriesId }
        if (source.Series is not null)
        {
            // generated mapping already filled this if SeriesModel -> SeriesDTO map exists.
            // ensure circular cut:
            if (target.Series is not null)
            {
                target.Series.LastVolume = null;
            }
        }
        else
        {
            // fallback if no series entity is loaded
            target.Series = new SeriesDTO
            {
                Id = source.SeriesId,
            };
        }
    }

    /// <summary>
    /// Map a metadata dto to a create book dto.
    /// </summary>
    /// <param name="source">The metadata dto.</param>
    /// <param name="target">The create dto.</param>
    [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Mapperly User-Implemented")]
    private static void MetadataDtoToCreateBookDto(MetadataDTO source, CreateBookDTO target)
    {
        // release date parsing (string -> DateTime?)
        if (!string.IsNullOrWhiteSpace(source.ReleaseDate)
            && DateTime.TryParse(source.ReleaseDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            target.ReleaseDate = parsed;
        }

        // series name + volume -> CreateSeriesDTO + SeriesNumber
        if (!string.IsNullOrWhiteSpace(source.Series))
        {
            target.Series = new CreateSeriesDTO { Name = source.Series };
        }

        if (source.Volume.HasValue)
        {
            target.SeriesNumber = source.Volume;
        }

        // author: take first if available
        if (source.Authors.Count > 0)
        {
            var (firstName, lastName) = SplitName(source.Authors.First());
            target.Author = new CreateAuthorDTO
            {
                FirstName = firstName,
                LastName = lastName,
            };
        }

        // categories
        target.Categories = source.Categories
            .Select(c => new CreateCategoryDTO { Name = c })
            .ToList();

        // tags
        target.Tags = source.Tags
            .Select(t => new CreateTagDTO { Name = t })
            .ToList();

        // scalar fields
        target.PageNumber = source.Pages;
        target.Title = source.Title;
        target.Description = source.Description ?? string.Empty;
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            return result.ToUniversalTime();
        }

        return null;
    }
}
