// <copyright file="Mapper.Books.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Data.Models;
using Riok.Mapperly.Abstractions;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

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
    [MapperIgnoreSource(nameof(BookModel.CreatedAt))]
    [MapperIgnoreSource(nameof(BookModel.UpdatedAt))]
    [MapperIgnoreSource(nameof(BookModel.UserMetadata))]
    [MapperIgnoreTarget(nameof(BookDTO.Categories))]
    [MapperIgnoreTarget(nameof(BookDTO.Tags))]
    [MapperIgnoreTarget(nameof(BookDTO.UserMetadata))]
    public partial BookDTO BookModelToBookDtoCore(BookModel model);

    /// <summary>
    /// Map a book model to a book dto, filling custom fields manually.
    /// </summary>
    /// <param name="model">The book model.</param>
    /// <returns>The book dto.</returns>
    [UserMapping(Default = true)]
    public BookDTO BookModelToBookDto(BookModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var dto = this.BookModelToBookDtoCore(model);

        // categories
        dto.Categories = model.Categories
            .Select(x => this.CategoryModelToCategoryDto(x.Category))
            .ToList();

        // tags
        dto.Tags = model.Tags
            .Select(x => this.TagModelToTagDto(x.Tag))
            .ToList();

        // user metadata
        dto.UserMetadata = model.UserMetadata
            .Select(this.UserBookMetadataModelToUserBookMetadataDto)
            .ToList();

        // series: prefer full entity, fallback to ID
        if (dto.Series is not null)
        {
            dto.Series.LastVolume = null;
        }
        else
        {
            dto.Series = new SeriesDTO
            {
                Id = model.SeriesId,
            };
        }

        return dto;
    }

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
    [MapperIgnoreTarget(nameof(BookModel.CreatedAt))]
    [MapperIgnoreTarget(nameof(BookModel.UpdatedAt))]
    [MapperIgnoreTarget(nameof(BookModel.UserMetadata))]
    public partial BookModel CreateBookDtoToBookModelCore(CreateBookDTO dto);

    /// <summary>
    /// Map a create book dto to a book model including nested objects.
    /// </summary>
    /// <param name="dto">The create dto.</param>
    /// <returns>The book model.</returns>
    [UserMapping(Default = true)]
    public BookModel CreateBookDtoToBookModel(CreateBookDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var model = this.CreateBookDtoToBookModelCore(dto);

        // series
        if (dto.Series != null)
        {
            model.Series = new SeriesModel
            {
                Name = dto.Series.Name ?? string.Empty,
            };
        }

        model.SeriesNumber = dto.SeriesNumber ?? 0;

        // author
        if (dto.Author != null)
        {
            model.Author = new AuthorModel
            {
                FirstName = dto.Author.FirstName ?? string.Empty,
                LastName = dto.Author.LastName ?? string.Empty,
            };
        }

        // location
        if (dto.Location != null)
        {
            model.Location = new LocationModel
            {
                Type = this.LocationTypeDtoToLocationType(dto.Location.Type),
                Url = dto.Location.Url,
            };
        }

        // categories
        model.Categories = dto.Categories
            .Select(c => new BookCategoryModel
            {
                Category = new CategoryModel { Name = c.Name ?? string.Empty },
            })
            .ToList();

        // tags
        model.Tags = dto.Tags
            .Select(t => new BookTagModel
            {
                Tag = new TagModel { Name = t.Name ?? string.Empty },
            })
            .ToList();

        return model;
    }

    /// <summary>
    /// Map a book dto to a create book dto.
    /// </summary>
    /// <param name="dto">The book dto.</param>
    /// <returns>The create dto.</returns>
    [MapperIgnoreSource(nameof(BookDTO.Id))]
    [MapperIgnoreSource(nameof(BookDTO.Cover))]
    [MapperIgnoreSource(nameof(BookDTO.UserMetadata))]
    [MapperIgnoreSource(nameof(BookDTO.Rating))]
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
    [MapperIgnoreTarget(nameof(BookDTO.UserMetadata))]
    public partial BookDTO BookSearchViewToBookDtoCore(BookSearchView view);

    /// <summary>
    /// Map a book search view to a book dto, applying additional manual field logic.
    /// </summary>
    /// <param name="view">The book search view.</param>
    /// <returns>The mapped book dto.</returns>
    [UserMapping(Default = true)]
    public BookDTO BookSearchViewToBookDto(BookSearchView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        var dto = this.BookSearchViewToBookDtoCore(view);

        // map series
        if (!string.IsNullOrWhiteSpace(view.SeriesName))
        {
            dto.Series = new SeriesDTO
            {
                Name = view.SeriesName,
            };
        }

        // map author (may contain multiple comma-separated values)
        if (!string.IsNullOrWhiteSpace(view.AuthorNames))
        {
            var firstAuthor = view.AuthorNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();

            if (firstAuthor is not null)
            {
                var (firstName, lastName) = SplitName(firstAuthor);
                dto.Author = new AuthorDTO
                {
                    FirstName = firstName,
                    LastName = lastName,
                };
            }
        }

        // map categories
        if (!string.IsNullOrWhiteSpace(view.CategoryNames))
        {
            dto.Categories = view.CategoryNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(c => new CategoryDTO { Name = c })
                .ToList();
        }

        // map tags
        if (!string.IsNullOrWhiteSpace(view.TagNames))
        {
            dto.Tags = view.TagNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => new TagDTO { Name = t })
                .ToList();
        }

        return dto;
    }

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
    public partial CreateBookDTO MetadataDtoToCreateBookDtoCore(MetadataDTO dto);

    /// <summary>
    /// Map a metadata dto to a create book dto with additional conversions.
    /// </summary>
    /// <param name="dto">The metadata dto.</param>
    /// <returns>The create dto.</returns>
    [UserMapping(Default = true)]
    public CreateBookDTO MetadataDtoToCreateBookDto(MetadataDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var target = this.MetadataDtoToCreateBookDtoCore(dto);

        target.ReleaseDate = ParseDate(dto.ReleaseDate);

        if (!string.IsNullOrWhiteSpace(dto.Series))
        {
            target.Series = new CreateSeriesDTO { Name = dto.Series };
        }

        if (dto.Volume.HasValue)
        {
            target.SeriesNumber = dto.Volume;
        }

        if (dto.Authors.Count > 0)
        {
            var (first, last) = SplitName(dto.Authors.First());
            target.Author = new CreateAuthorDTO { FirstName = first, LastName = last };
        }

        target.Categories = dto.Categories.Select(c => new CreateCategoryDTO { Name = c }).ToList();
        target.Tags = dto.Tags.Select(t => new CreateTagDTO { Name = t }).ToList();

        target.PageNumber = dto.Pages;
        target.Title = dto.Title;
        target.Description = dto.Description ?? string.Empty;

        return target;
    }

    internal static DateTime? ParseDate(string? value)
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
