// <copyright file="UsersLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.User;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models.User;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="UsersLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The mapper.</param>
public class UsersLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper) : IUsersLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    /// <inheritdoc/>
    public async Task<IList<UserDTO>> GetUsersAsync()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Users
            .AsNoTracking()
            .OrderBy(x => x.Username)
            .Select(x => this.mapper.UserModelToUserDto(x))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => this.mapper.UserModelToUserDto(x))
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<UserDTO?> CreateUserAsync(CreateUserDTO createUserDto)
    {
        if (createUserDto is null)
        {
            return null;
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var user = this.mapper.CreateUserDtoToUserModel(createUserDto);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return this.mapper.UserModelToUserDto(user);
    }

    /// <inheritdoc/>
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

        return this.mapper.UserModelToUserDto(user);
    }

    /// <inheritdoc/>
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

        return this.mapper.UserModelToUserDto(user);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<BookDTO>> GetLastVisitedBooksAsync(Guid userId, int page, int pageSize)
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
            .Include(x => x.Book)
                .ThenInclude(x => x.UserMetadata)
                    .ThenInclude(x => x.User)
            .AsSingleQuery()

            .Where(x => x.UserId == userId);

        var items = await query
            .OrderByDescending(x => x.VisitedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.BookModelToBookDto(x.Book))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<BookDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc/>
    public async Task<List<UserSettingDTO>> GetSettingsByUserIdAsync(Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.UserSettings
            .Where(x => x.UserId == userId)
            .Select(x => this.mapper.UserSettingModelToUserSettingDto(x))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateSettingAsync(Guid userId, UserSettingDTO settingDto)
    {
        ArgumentNullException.ThrowIfNull(settingDto);

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var setting = await context.UserSettings
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Key == settingDto.Key);

        if (setting is null)
        {
            // add new setting
            setting = this.mapper.UserSettingDtoToUserSettingModel(settingDto);
            setting.UserId = userId;
            context.UserSettings.Add(setting);
        }
        else
        {
            // update existing setting
            context.Entry(setting).CurrentValues.SetValues(new
            {
                settingDto.Value,
                settingDto.Type,
            });
        }

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task AddOrUpdateBookMetadata(Guid bookId, Guid userId, CreateOrUpdateUserBookMetadataDTO bookMetadata)
    {
        ArgumentNullException.ThrowIfNull(bookMetadata);

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var bookMetadataModel = await context.UserBookMetadata
            .FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == bookId);

        if (bookMetadataModel is null)
        {
            // add new book metadata
            context.UserBookMetadata.Add(new UserBookMetadataModel
            {
                BookId = bookId,
                UserId = userId,
                Rating = bookMetadata.Rating,
                Notes = bookMetadata.Notes,
                CreatedOn = DateTime.UtcNow,
            });
        }
        else
        {
            // update existing book metadata
            context.Entry(bookMetadataModel).CurrentValues.SetValues(new
            {
                bookMetadata.Rating,
                bookMetadata.Notes,
                CreatedOn = DateTime.UtcNow,
            });
        }

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteBookMetadata(Guid bookId, Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        await context.UserBookMetadata.Where(x => x.UserId == userId && x.BookId == bookId)
            .ExecuteDeleteAsync();
    }
}
