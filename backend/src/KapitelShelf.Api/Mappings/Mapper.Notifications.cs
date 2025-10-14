// <copyright file="Mapper.Notifications.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Data.Models.Notifications;
using Riok.Mapperly.Abstractions;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The watchlists mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a notification model to a notification dto.
    /// </summary>
    /// <param name="model">The notification model.</param>
    /// <returns>The notification dto.</returns>
    [MapperIgnoreSource(nameof(NotificationModel.UserId))]
    [MapperIgnoreSource(nameof(NotificationModel.User))]
    public partial NotificationDto NotificationModelToNotificationDto(NotificationModel model);

    /// <summary>
    /// Map a notification type to a notification type dto.
    /// </summary>
    /// <param name="model">The notification type.</param>
    /// <returns>The notification type dto.</returns>
    public partial NotificationTypeDto NotificationTypeToNotificationTypeDto(NotificationType model);

    /// <summary>
    /// Map a notification severity to a notification severity dto.
    /// </summary>
    /// <param name="model">The notification severity.</param>
    /// <returns>The notification severity dto.</returns>
    public partial NotificationSeverityDto NotificationSeverityToNotificationSeverityDto(NotificationSeverity model);
}
