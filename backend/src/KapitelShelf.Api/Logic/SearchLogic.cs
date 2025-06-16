// <copyright file="SearchLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="SearchLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The auto mapper.</param>
public class SearchLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper)
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Search all books by search term.
    /// </summary>
    /// <param name="searchterm">The search term.</param>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    public async Task<PagedResult<BookDTO>> SearchBySearchterm(string searchterm, int page, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(searchterm))
        {
            return new PagedResult<BookDTO> { Items = [], TotalCount = 0 };
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();
        context.ChangeTracker.LazyLoadingEnabled = false;

        var query = context.BookSearchView
            .AsNoTracking()

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            .Include(x => x.BookModel)
                .ThenInclude(x => x.Cover)
            .Include(x => x.BookModel)
                .ThenInclude(x => x.Author)
            .Include(x => x.BookModel)
                .ThenInclude(x => x.Series)
            .Include(x => x.BookModel)
                .ThenInclude(x => x.Location)
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            .FilterBySearchtermQuery(searchterm);

        var items = await query
            .SortBySearchtermQuery(searchterm)

            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.Map<BookDTO>(x.BookModel))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<BookDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }
}
