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
    /// Map a nullable book model to a nullable book dto.
    /// </summary>
    /// <param name="model">The book model.</param>
    /// <returns>The book dto.</returns>
    public BookDTO? BookModelToBookDtoNullable(BookModel? model) => model is null ? null : this.BookModelToBookDto(model);

    /// <summary>
    /// Map a create book dto to a book model.
    /// </summary>
    /// <param name="dto">The create dto.</param>
    /// <returns>The book model.</returns>
    [MapperIgnoreSource(nameof(CreateBookDTO.Series))]
    [MapperIgnoreSource(nameof(CreateBookDTO.SeriesNumber))]
    [MapperIgnoreSource(nameof(CreateBookDTO.Author))]
    [MapperIgnoreSource(nameof(CreateBookDTO.Categories))]
    [MapperIgnoreSource(nameof(CreateBookDTO.Tags))]
    [MapperIgnoreSource(nameof(CreateBookDTO.Location))]
    [MapperIgnoreTarget(nameof(BookModel.Id))]
    [MapperIgnoreTarget(nameof(BookModel.Series))]
    [MapperIgnoreTarget(nameof(BookModel.SeriesId))]
    [MapperIgnoreTarget(nameof(BookModel.SeriesNumber))]
    [MapperIgnoreTarget(nameof(BookModel.Author))]
    [MapperIgnoreTarget(nameof(BookModel.AuthorId))]
    [MapperIgnoreTarget(nameof(BookModel.Categories))]
    [MapperIgnoreTarget(nameof(BookModel.Tags))]
    [MapperIgnoreTarget(nameof(BookModel.Cover))]
    [MapperIgnoreTarget(nameof(BookModel.Location))]
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
    [MapperIgnoreSource(nameof(BookSearchView.SeriesName))]
    [MapperIgnoreSource(nameof(BookSearchView.AuthorNames))]
    [MapperIgnoreSource(nameof(BookSearchView.CategoryNames))]
    [MapperIgnoreSource(nameof(BookSearchView.TagNames))]
    [MapperIgnoreSource(nameof(BookSearchView.SearchVector))]
    [MapperIgnoreSource(nameof(BookSearchView.SearchText))]
    [MapperIgnoreSource(nameof(BookSearchView.BookModel))]
    [MapperIgnoreTarget(nameof(BookDTO.Series))]
    [MapperIgnoreTarget(nameof(BookDTO.Author))]
    [MapperIgnoreTarget(nameof(BookDTO.Categories))]
    [MapperIgnoreTarget(nameof(BookDTO.Tags))]
    [MapperIgnoreTarget(nameof(BookDTO.Cover))]
    [MapperIgnoreTarget(nameof(BookDTO.Location))]
    [MapperIgnoreTarget(nameof(BookDTO.ReleaseDate))]
    [MapperIgnoreTarget(nameof(BookDTO.PageNumber))]
    [MapperIgnoreTarget(nameof(BookDTO.SeriesNumber))]
    public partial BookDTO BookSearchViewToBookDto(BookSearchView view);

    /// <summary>
    /// Map a metadata dto to a create book dto.
    /// </summary>
    /// <param name="dto">The metadata dto.</param>
    /// <returns>The create book dto.</returns>
    [MapperIgnoreSource(nameof(MetadataDTO.Description))]
    [MapperIgnoreSource(nameof(MetadataDTO.Series))]
    [MapperIgnoreSource(nameof(MetadataDTO.Volume))]
    [MapperIgnoreSource(nameof(MetadataDTO.Authors))]
    [MapperIgnoreSource(nameof(MetadataDTO.ReleaseDate))]
    [MapperIgnoreSource(nameof(MetadataDTO.Pages))]
    [MapperIgnoreSource(nameof(MetadataDTO.CoverUrl))]
    [MapperIgnoreSource(nameof(MetadataDTO.Categories))]
    [MapperIgnoreSource(nameof(MetadataDTO.Tags))]
    [MapperIgnoreSource(nameof(MetadataDTO.TitleMatchScore))]
    [MapperIgnoreSource(nameof(MetadataDTO.CompletenessScore))]
    [MapperIgnoreTarget(nameof(CreateBookDTO.Series))]
    [MapperIgnoreTarget(nameof(CreateBookDTO.Description))]
    [MapperIgnoreTarget(nameof(CreateBookDTO.SeriesNumber))]
    [MapperIgnoreTarget(nameof(CreateBookDTO.Author))]
    [MapperIgnoreTarget(nameof(CreateBookDTO.Categories))]
    [MapperIgnoreTarget(nameof(CreateBookDTO.Tags))]
    [MapperIgnoreTarget(nameof(CreateBookDTO.Location))]
    [MapperIgnoreTarget(nameof(CreateBookDTO.ReleaseDate))]
    [MapperIgnoreTarget(nameof(CreateBookDTO.PageNumber))]
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
    /// Map a create book dto to a book model.
    /// </summary>
    /// <param name="source">the dto source.</param>
    /// <param name="target">the model target.</param>
    [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Mapperly User-Implemented")]
    private void CreateBookDtoToBookModel(CreateBookDTO source, BookModel target)
    {
        // map basic scalar fields are handled automatically by Mapperly
        // now map nested objects and relationship entities

        // map series if provided
        if (source.Series != null)
        {
            target.Series = new SeriesModel
            {
                Name = source.Series.Name ?? string.Empty,
            };

            // if DTO didn't include an id, leave Guid.Empty (handled by persistence layer)
            if (target.Series.Id == Guid.Empty)
            {
                target.Series.Id = Guid.NewGuid();
            }

            target.SeriesId = target.Series.Id;
        }

        // if series number is nullable in dto, use 0 fallback
        target.SeriesNumber = source.SeriesNumber ?? 0;

        // map author if provided
        if (source.Author != null)
        {
            target.Author = new AuthorModel
            {
                FirstName = source.Author.FirstName ?? string.Empty,
                LastName = source.Author.LastName ?? string.Empty,
            };
        }

        // map location if provided
        if (source.Location != null)
        {
            target.Location = new LocationModel
            {
                Type = this.LocationTypeDtoToLocationType(source.Location.Type),
                Url = source.Location.Url,
            };
        }

        // map categories (CreateCategoryDTO -> BookCategoryModel)
        target.Categories = source.Categories
            .Select(c => new BookCategoryModel
            {
                Category = new CategoryModel
                {
                    Name = c.Name ?? string.Empty,
                },
            })
            .ToList();

        // map tags (CreateTagDTO -> BookTagModel)
        target.Tags = source.Tags
            .Select(t => new BookTagModel
            {
                Tag = new TagModel
                {
                    Name = t.Name ?? string.Empty,
                },
            })
            .ToList();
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

    /// <summary>
    /// Map a book search view to a book dto.
    /// </summary>
    /// <param name="source">the book search view.</param>
    /// <param name="target">the resulting book dto.</param>
    [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Mapperly User-Implemented")]
    private static void BookSearchViewToBookDto(BookSearchView source, BookDTO target)
    {
        // map series
        if (!string.IsNullOrWhiteSpace(source.SeriesName))
        {
            target.Series = new SeriesDTO
            {
                Name = source.SeriesName,
            };
        }

        // map author (AuthorNames may contain multiple comma-separated values)
        if (!string.IsNullOrWhiteSpace(source.AuthorNames))
        {
            var firstAuthor = source.AuthorNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();

            if (firstAuthor is not null)
            {
                var (firstName, lastName) = SplitName(firstAuthor);

                target.Author = new AuthorDTO
                {
                    FirstName = firstName,
                    LastName = lastName,
                };
            }
        }

        // map categories (comma-separated)
        if (!string.IsNullOrWhiteSpace(source.CategoryNames))
        {
            target.Categories = source.CategoryNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(c => new CategoryDTO { Name = c })
                .ToList();
        }

        // map tags (comma-separated)
        if (!string.IsNullOrWhiteSpace(source.TagNames))
        {
            target.Tags = source.TagNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => new TagDTO { Name = t })
                .ToList();
        }
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
