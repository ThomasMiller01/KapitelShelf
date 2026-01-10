// <copyright file="MapperNotificationsTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data.Models.Notifications;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the notifications mapper.
/// </summary>
[TestFixture]
public class MapperNotificationsTests
{
    private Mapper testee;

    /// <summary>
    /// Sets up the mapper instance before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // setup
#pragma warning disable IDE0022 // Use expression body for method
        this.testee = new Mapper();
#pragma warning restore IDE0022 // Use expression body for method
    }

    /// <summary>
    /// Tests that NotificationModelToNotificationDto maps core properties and keeps children unchanged when no children exist.
    /// </summary>
    [Test]
    public void NotificationModelToNotificationDto_MapsCoreProperties_WhenNoChildren()
    {
        // setup
        var model = new NotificationModel
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Message = "Message",
            Type = NotificationType.Info,
            Severity = NotificationSeverity.Low,
            Created = DateTime.UtcNow.AddMinutes(-10),
            Expires = DateTime.UtcNow.AddDays(1),
            IsRead = false,
            Children = [],
        };

        // execute
        var dto = this.testee.NotificationModelToNotificationDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Title, Is.EqualTo(model.Title));
            Assert.That(dto.Message, Is.EqualTo(model.Message));
            Assert.That(dto.Type, Is.EqualTo(this.testee.NotificationTypeToNotificationTypeDto(model.Type)));
            Assert.That(dto.Severity, Is.EqualTo(this.testee.NotificationSeverityToNotificationSeverityDto(model.Severity)));
            Assert.That(dto.Created, Is.EqualTo(model.Created));
            Assert.That(dto.Expires, Is.EqualTo(model.Expires));
            Assert.That(dto.IsRead, Is.EqualTo(model.IsRead));
            Assert.That(dto.Children, Is.Empty);
        });
    }

    /// <summary>
    /// Tests that NotificationModelToNotificationDto calculates read/severity/expires and sorts children by Created descending.
    /// </summary>
    [Test]
    public void NotificationModelToNotificationDto_CalculatesFieldsAndSortsChildren_WhenChildrenExist()
    {
        // setup
        var parentExpires = DateTime.UtcNow.AddDays(1);

        var child1 = new NotificationModel
        {
            Id = Guid.NewGuid(),
            Title = "Child1",
            Message = "Child1",
            Type = NotificationType.Info,
            Severity = NotificationSeverity.Low,
            Created = DateTime.UtcNow.AddMinutes(-30),
            Expires = DateTime.UtcNow.AddDays(1),
            IsRead = true,
            Children = [],
        };

        var child2 = new NotificationModel
        {
            Id = Guid.NewGuid(),
            Title = "Child2",
            Message = "Child2",
            Type = NotificationType.Info,
            Severity = NotificationSeverity.Critical,
            Created = DateTime.UtcNow.AddMinutes(-5),
            Expires = DateTime.UtcNow.AddDays(5),
            IsRead = false,
            Children = [],
        };

        var model = new NotificationModel
        {
            Id = Guid.NewGuid(),
            Title = "Parent",
            Message = "Parent",
            Type = NotificationType.Warning,
            Severity = NotificationSeverity.Medium,
            Created = DateTime.UtcNow.AddHours(-1),
            Expires = parentExpires,
            IsRead = true,
            Children = [child1, child2],
        };

        // execute
        var dto = this.testee.NotificationModelToNotificationDto(model);

        // assert
        Assert.That(dto, Is.Not.Null);

        Assert.Multiple(() =>
        {
            // read: all children must be read
            Assert.That(dto.IsRead, Is.False);

            // severity: max of children
            Assert.That(dto.Severity, Is.EqualTo(NotificationSeverityDto.Critical));

            // expires: max of children
            Assert.That(dto.Expires, Is.EqualTo(child2.Expires));

            // children sorted by created desc (child2 newer than child1)
            Assert.That(dto.Children, Has.Count.EqualTo(2));
            Assert.That(dto.Children[0].Id, Is.EqualTo(child2.Id));
            Assert.That(dto.Children[1].Id, Is.EqualTo(child1.Id));
        });
    }

    /// <summary>
    /// Tests NotificationType enum mapping is stable.
    /// </summary>
    [Test]
    public void NotificationTypeToNotificationTypeDto_MapsValue()
    {
        // setup
        var model = NotificationType.Error;

        // execute
        var dto = this.testee.NotificationTypeToNotificationTypeDto(model);

        // assert
        Assert.That(dto, Is.EqualTo(NotificationTypeDto.Error));
    }

    /// <summary>
    /// Tests NotificationSeverity enum mapping is stable.
    /// </summary>
    [Test]
    public void NotificationSeverityToNotificationSeverityDto_MapsValue()
    {
        // setup
        var model = NotificationSeverity.Medium;

        // execute
        var dto = this.testee.NotificationSeverityToNotificationSeverityDto(model);

        // assert
        Assert.That(dto, Is.EqualTo(NotificationSeverityDto.Medium));
    }

    /// <summary>
    /// Tests NotificationTypeDto enum mapping is stable.
    /// </summary>
    [Test]
    public void NotificationTypeDtoToNotificationType_MapsValue()
    {
        // setup
        var dto = NotificationTypeDto.Warning;

        // execute
        var model = this.testee.NotificationTypeDtoToNotificationType(dto);

        // assert
        Assert.That(model, Is.EqualTo(NotificationType.Warning));
    }

    /// <summary>
    /// Tests NotificationSeverityDto enum mapping is stable.
    /// </summary>
    [Test]
    public void NotificationSeverityDtoToNotificationSeverity_MapsValue()
    {
        // setup
        var dto = NotificationSeverityDto.Low;

        // execute
        var model = this.testee.NotificationSeverityDtoToNotificationSeverity(dto);

        // assert
        Assert.That(model, Is.EqualTo(NotificationSeverity.Low));
    }
}
