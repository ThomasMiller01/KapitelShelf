// <copyright file="UsersLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.User;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="UsersLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The auto mapper.</param>
public class UsersLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper)
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<IList<UserDTO>> GetUsersAsync()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Users
            .AsNoTracking()
            .Select(x => this.mapper.Map<UserDTO>(x))
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a user by its id.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => this.mapper.Map<UserDTO>(x))
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="createBookDTO">The dto containing information for the new book.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<BookDTO?> CreateUserAsync(CreateBookDTO createBookDTO)
    {
        if (createBookDTO is null)
        {
            return null;
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        // context.Books.Add(book);
        await context.SaveChangesAsync();

        // return this.mapper.Map<BookDTO>(book);
        return null;
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="bookId">The id of the book to update.</param>
    /// <param name="bookDto">The dto containing updated book information.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<BookDTO?> UpdateUserAsync(Guid bookId, BookDTO bookDto)
    {
        if (bookDto is null)
        {
            return null;
        }

        await using var context = await this.dbContextFactory.CreateDbContextAsync();

        var book = await context.Books
            .Include(b => b.Author)
            .Include(b => b.Series)
            .Include(b => b.Cover)
            .Include(b => b.Location)!
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                .ThenInclude(l => l.FileInfo)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
            .Include(b => b.Tags)
                .ThenInclude(bt => bt.Tag)
            .AsSingleQuery()
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book is null)
        {
            return null;
        }

        // patch book root scalars
        context.Entry(book).CurrentValues.SetValues(new
        {
            bookDto.Title,
            bookDto.Description,
            bookDto.PageNumber,
            bookDto.ReleaseDate,
            bookDto.SeriesNumber,
        });

        // commit
        await context.SaveChangesAsync();

        return this.mapper.Map<BookDTO>(book);
    }

    /// <summary>
    /// Deletes a user by its id.
    /// </summary>
    /// <param name="userId">The id of the user to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<UserDTO?> DeleteUserAsync(Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FindAsync(userId);
        if (user is null)
        {
            return null;
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return this.mapper.Map<UserDTO>(user);
    }
}
