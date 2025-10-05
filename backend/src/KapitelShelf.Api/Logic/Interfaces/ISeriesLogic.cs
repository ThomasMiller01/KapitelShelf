﻿// <copyright file="ISeriesLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Series;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// The series logic interface.
/// </summary>
public interface ISeriesLogic
{
    /// <summary>
    /// Get all series.
    /// </summary>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    Task<PagedResult<SeriesDTO>> GetSeriesAsync(int page, int pageSize);

    /// <summary>
    /// Get a series by id.
    /// </summary>
    /// <param name="seriesId">The id of the series to fetch.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    Task<SeriesDTO?> GetSeriesByIdAsync(Guid seriesId);

    /// <summary>
    /// Search all series by their name.
    /// </summary>
    /// <param name="name">The series name.</param>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    Task<PagedResult<SeriesDTO>> Search(string name, int page, int pageSize);

    /// <summary>
    /// Get a series by id.
    /// </summary>
    /// <param name="seriesId">The id of the series to fetch.</param>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    Task<PagedResult<BookDTO>> GetBooksBySeriesIdAsync(Guid seriesId, int page, int pageSize);

    /// <summary>
    /// Delete a series.
    /// </summary>
    /// <param name="seriesId">The id of the series to delete.</param>
    /// <returns>A <see cref="Task{SeriesDTO}"/> representing the result of the asynchronous operation.</returns>
    Task<SeriesDTO?> DeleteSeriesAsync(Guid seriesId);

    /// <summary>
    /// Update a series.
    /// </summary>
    /// <param name="seriesId">The id of the series to update.</param>
    /// <param name="seriesDto">The updated series dto.</param>
    /// <returns>A <see cref="Task{SeriesDTO}"/> representing the result of the asynchronous operation.</returns>
    Task<SeriesDTO?> UpdateSeriesAsync(Guid seriesId, SeriesDTO seriesDto);

    /// <summary>
    /// Delete the files of all books from a series.
    /// </summary>
    /// <param name="seriesId">The id of the series.</param>
    /// <returns>A task.</returns>
    Task DeleteFilesAsync(Guid seriesId);

    /// <summary>
    /// Merge all books from the source series into the target series.
    /// </summary>
    /// <param name="sourceSeriesId">The source series id.</param>
    /// <param name="targetSeriesId">The target series id.</param>
    /// <returns>A task.</returns>
    Task MergeSeries(Guid sourceSeriesId, Guid targetSeriesId);
}
