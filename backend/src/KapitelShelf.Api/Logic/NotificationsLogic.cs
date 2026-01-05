// <copyright file="NotificationsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models.Notifications;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The notifications logic.
/// </summary>
public class NotificationsLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper) : INotificationsLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    /// <summary>
    /// Add a notification.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="message">The message.</param>
    /// <param name="type">The type.</param>
    /// <param name="severity">The severity.</param>
    /// <param name="expires">The expiry date.</param>
    /// <param name="source">The source.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="parentId">The parent id.</param>
    /// <returns>A task.</returns>
    /// <remarks>If userId is null, the notification will be added to all users.</remarks>
    public async Task AddNotification(
        string title,
        string message,
        NotificationTypeDto type = NotificationTypeDto.Info,
        NotificationSeverityDto severity = NotificationSeverityDto.Low,
        DateTime? expires = null,
        string source = "",
        Guid? userId = null,
        Guid? parentId = null)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        // add notification to all users, if userId is null.
        if (userId is null)
        {
            var users = context.Users
                .Select(x => x.Id)
                .ToList();

            foreach (var user in users)
            {
                await this.AddNotification(title, message, type, severity, expires, source, user);
            }

            return;
        }

        // if a notification with that title already exists,
        // add this notification as a child of the existing one
        var existingParent = await context.Notifications
            .Where(x =>
                x.UserId == userId &&
                x.ParentId == null &&
                x.Title != null &&
                x.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefaultAsync();
        var parentToUse = parentId ?? existingParent?.Id;

        var notificationModel = new NotificationModel
        {
            Title = title,
            Message = message,
            Type = this.mapper.NotificationTypeDtoToNotificationType(type),
            Severity = this.mapper.NotificationSeverityDtoToNotificationSeverity(severity),
            Created = DateTime.UtcNow,
            Expires = expires ?? DateTime.MaxValue,
            IsRead = false,
            Source = source,
            UserId = (Guid)userId,
            ParentId = parentToUse,
        };

        context.Notifications.Add(notificationModel);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<NotificationDto?> GetByIdAsync(Guid id, Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Notifications
            .Include(x => x.Children)
            .Where(x => x.Id == id && x.UserId == userId && x.ParentId == null)
            .Select(x => this.mapper.NotificationModelToNotificationDto(x))
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<List<NotificationDto>> GetByUserIdAsync(Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Notifications
            .Include(x => x.Children)
            .Where(x => x.UserId == userId && x.ParentId == null)
            .OrderByDescending(x => x.Created)
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

    /// <inheritdoc/>
    public async Task DeleteExpiredNotificationsAsync()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        await context.Notifications
            .Where(x => x.Expires <= DateTime.UtcNow)
            .ExecuteDeleteAsync();
    }
}
