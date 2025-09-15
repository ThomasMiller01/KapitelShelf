// <copyright file="UsersLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.User;
using KapitelShelf.Data;
using KapitelShelf.Data.Models.User;
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
            .OrderBy(x => x.Username)
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
    /// <param name="createUserDto">The dto containing information for the new user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<UserDTO?> CreateUserAsync(CreateUserDTO createUserDto)
    {
        if (createUserDto is null)
        {
            return null;
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var user = this.mapper.Map<UserModel>(createUserDto);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return this.mapper.Map<UserDTO>(user);
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="userId">The id of the user to update.</param>
    /// <param name="userDto">The dto containing updated user information.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<UserDTO?> UpdateUserAsync(Guid userId, UserDTO userDto)
    {
        if (userDto is null)
        {
            return null;
        }

        await using var context = await this.dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(b => b.Id == userId);
        if (user is null)
        {
            return null;
        }

        // patch user root scalars
        context.Entry(user).CurrentValues.SetValues(new
        {
            userDto.Username,
            userDto.Image,
            userDto.Color,
        });

        // commit
        await context.SaveChangesAsync();

        return this.mapper.Map<UserDTO>(user);
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

    /// <summary>
    /// Get the last visited books of a user.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<PagedResult<BookDTO>> GetLastVisitedBooks(Guid userId, int page, int pageSize)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();
        context.ChangeTracker.LazyLoadingEnabled = false;

        var query = context.VisitedBooks
            .AsNoTracking()

            .Include(x => x.Book)
                .ThenInclude(x => x.Author)
            .Include(x => x.Book)
                .ThenInclude(x => x.Cover)
            .Include(x => x.Book)
                .ThenInclude(x => x.Location)
            .Include(x => x.Book)
                .ThenInclude(x => x.Categories)
                    .ThenInclude(x => x.Category)
            .Include(x => x.Book)
                .ThenInclude(x => x.Tags)
                    .ThenInclude(x => x.Tag)
            .AsSingleQuery()

            .Where(x => x.UserId == userId);

        var items = await query
            .OrderByDescending(x => x.VisitedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.Map<BookDTO>(x.Book))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<BookDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }
}
