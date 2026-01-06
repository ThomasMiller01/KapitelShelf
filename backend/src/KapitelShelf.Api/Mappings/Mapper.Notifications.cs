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
    [MapperIgnoreSource(nameof(NotificationModel.ParentId))]
    [MapperIgnoreSource(nameof(NotificationModel.Parent))]
    public partial NotificationDto NotificationModelToNotificationDtoCore(NotificationModel model);

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

    /// <summary>
    /// Map a notification type dto to a notification type.
    /// </summary>
    /// <param name="dto">The notification type dto.</param>
    /// <returns>The notification type.</returns>
    public partial NotificationType NotificationTypeDtoToNotificationType(NotificationTypeDto dto);

    /// <summary>
    /// Map a notification severity dto to a notification severity.
    /// </summary>
    /// <param name="dto">The notification severity dto.</param>
    /// <returns>The notification severity.</returns>
    public partial NotificationSeverity NotificationSeverityDtoToNotificationSeverity(NotificationSeverityDto dto);

    /// <summary>
    /// Map a notification model to a notification dto.
    /// </summary>
    /// <param name="model">The notification model.</param>
    /// <returns>The notification dto.</returns>
    [UserMapping(Default = true)]
    public NotificationDto NotificationModelToNotificationDto(NotificationModel model)
    {
        var dto = this.NotificationModelToNotificationDtoCore(model);

        // calculate properties based on children
        if (dto.Children.Count != 0)
        {
            dto.IsRead = dto.Children.All(x => x.IsRead);
            dto.Severity = dto.Children.Max(x => x.Severity);
            dto.Expires = dto.Children.Max(x => x.Expires);
            dto.Children = dto.Children
                .OrderByDescending(x => x.Created)
                .ToList();
        }

        return dto;
    }
}
