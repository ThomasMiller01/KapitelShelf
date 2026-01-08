// <copyright file="NotificationsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Localization;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models.Notifications;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The notifications logic.
/// </summary>
public class NotificationsLogic(
    IDbContextFactory<KapitelShelfDBContext> dbContextFactory,
    Mapper mapper,
    LocalizationProvider<Notifications> notificationsLocalizations
) : INotificationsLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    private readonly LocalizationProvider<Notifications> notificationsLocalizations = notificationsLocalizations;

    /// <inheritdoc/>
    public async Task AddNotification(
        string localizationKey,
        object[]? titleArgs = null,
        object[]? messageArgs = null,
        NotificationTypeDto type = NotificationTypeDto.Info,
        NotificationSeverityDto severity = NotificationSeverityDto.Low,
        DateTime? expires = null,
        string source = "",
        Guid? userId = null,
        Guid? parentId = null)
    {
        var title = titleArgs is null
            ? this.notificationsLocalizations.Get($"{localizationKey}_Title")
            : this.notificationsLocalizations.Get($"{localizationKey}_Title", titleArgs);

        var message = messageArgs is null
            ? this.notificationsLocalizations.Get($"{localizationKey}_Message")
            : this.notificationsLocalizations.Get($"{localizationKey}_Message", messageArgs);

        await this.AddNotification(title, message, type, severity, expires, source, userId, parentId);
    }

    /// <inheritdoc/>
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
#pragma warning disable CA1304 // Specify CultureInfo
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning disable CA1311 // Specify a culture or use an invariant version
        var existingParent = await context.Notifications
            .Where(x =>
                x.UserId == userId &&
                x.ParentId == null &&
                x.Title != null &&
                x.Title.ToLower() == title.ToLower())
            .FirstOrDefaultAsync();
#pragma warning restore CA1311 // Specify a culture or use an invariant version
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning restore CA1304 // Specify CultureInfo
        var parentToUse = parentId ?? existingParent?.Id;

        var notificationModel = new NotificationModel
        {
            Title = title,
            Message = message,
            Type = this.mapper.NotificationTypeDtoToNotificationType(type),
            Severity = this.mapper.NotificationSeverityDtoToNotificationSeverity(severity),
            Created = DateTime.UtcNow,
            Expires = expires ?? DateTime.UtcNow.AddDays(7),
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
    public async Task<NotificationStatsDto> GetStatsAsync(Guid userId)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var userNotifications = context.Notifications
            .Where(x => x.UserId == userId && x.ParentId == null)
            .AsQueryable();

        var unreadNotifications = userNotifications
            .Where(x => !x.IsRead || x.Children.Any(c => !c.IsRead))
            .AsQueryable();

        var unreadCount = await unreadNotifications
            .CountAsync();

        var unreadHasCritical = await unreadNotifications
            .AnyAsync(x => x.Severity == NotificationSeverity.Critical);

        var totalMessages = await userNotifications
            .CountAsync();

        return new NotificationStatsDto
        {
            UnreadCount = unreadCount,
            UnreadHasCritical = unreadHasCritical,
            TotalMessages = totalMessages,
        };
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

        // mark notification as well as all children as read
        var affectedRows = await context.Notifications
            .Where(x =>
                (x.Id == id && x.UserId == userId) ||
                (x.ParentId == id && x.UserId == userId))
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
