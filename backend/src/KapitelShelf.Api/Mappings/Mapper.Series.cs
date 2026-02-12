// <copyright file="Mapper.Series.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Data.Models;
using Riok.Mapperly.Abstractions;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The series mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a series model to a series dto.
    /// </summary>
    /// <param name="model">The series model.</param>
    /// <returns>The series dto.</returns>
    public SeriesDTO SeriesModelToSeriesDto(SeriesModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var dto = new SeriesDTO
        {
            Id = model.Id,
            Name = model.Name,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            Rating = model.Rating,
            TotalBooks = model.Books.Count,
        };

        var lastVolume = model.Books
            .OrderByDescending(b => b.SeriesNumber)
            .FirstOrDefault();

        if (lastVolume is not null)
        {
            // prevent circular dependencies
            lastVolume.Series = null;

            dto.LastVolume = this.BookModelToBookDto(lastVolume);
        }

        // calculate rating from books
        var ratings = model.Books
                    .SelectMany(x => x.UserMetadata, (_, y) => y.Rating)
                    .Where(x => x.HasValue)
                    .ToList();

        if (ratings.Count != 0)
        {
            var average = ratings.Average(x => x!.Value);
            dto.CalculatedRating = (int)Math.Round(average, MidpointRounding.AwayFromZero);
        }

        return dto;
    }

    /// <summary>
    /// map a series dto to a series model.
    /// </summary>
    /// <param name="dto">the series dto.</param>
    /// <returns>the series model.</returns>
    public SeriesModel SeriesDtoToSeriesModel(SeriesDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var model = new SeriesModel
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            Books = [],
        };

        return model;
    }

    /// <summary>
    /// Maps a create series dto to a series model.
    /// </summary>
    /// <param name="dto">The create series dto.</param>
    /// <returns>The series model.</returns>
    [MapperIgnoreTarget(nameof(SeriesModel.Id))]
    [MapperIgnoreTarget(nameof(SeriesModel.CreatedAt))]
    [MapperIgnoreTarget(nameof(SeriesModel.UpdatedAt))]
    [MapperIgnoreTarget(nameof(SeriesModel.Books))]
    public partial SeriesModel CreateSeriesDtoToSeriesModel(CreateSeriesDTO dto);

    /// <summary>
    /// Map a series dto to a create series dto.
    /// </summary>
    /// <param name="dto">The series dto.</param>
    /// <returns>The create series dto.</returns>
    [MapperIgnoreSource(nameof(SeriesDTO.Id))]
    [MapperIgnoreSource(nameof(SeriesDTO.CreatedAt))]
    [MapperIgnoreSource(nameof(SeriesDTO.UpdatedAt))]
    [MapperIgnoreSource(nameof(SeriesDTO.LastVolume))]
    [MapperIgnoreSource(nameof(SeriesDTO.TotalBooks))]
    [MapperIgnoreSource(nameof(SeriesDTO.CalculatedRating))]
    public partial CreateSeriesDTO SeriesDtoToCreateSeriesDto(SeriesDTO dto);
}
