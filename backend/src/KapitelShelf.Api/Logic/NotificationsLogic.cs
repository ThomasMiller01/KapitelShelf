// <copyright file="NotificationsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The notifications logic.
/// </summary>
public class NotificationsLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper) : INotificationsLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    /// <inheritdoc/>
    public async Task<NotificationDto?> GetByIdAsync(Guid id, Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Notifications
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => this.mapper.NotificationModelToNotificationDto(x))
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<List<NotificationDto>> GetByUserIdAsync(Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Notifications
            .Where(x => x.UserId == userId)
            .Select(x => this.mapper.NotificationModelToNotificationDto(x))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var affectedRows = await context.Notifications
            .Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsRead, true));

        return affectedRows != 0;
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAsReadAsync(Guid id, Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var affectedRows = await context.Notifications
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsRead, true));

        return affectedRows == 1;
    }
}
