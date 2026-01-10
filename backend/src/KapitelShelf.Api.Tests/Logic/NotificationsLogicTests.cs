// <copyright file="NotificationsLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Localization;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models.Notifications;
using KapitelShelf.Data.Models.User;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the NotificationsLogic class.
/// </summary>
[TestFixture]
public class NotificationsLogicTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private Mapper mapper;
    private ILocalizationProvider<Notifications> notificationsLocalizations;
    private INotificationsLogic testee;

    /// <summary>
    /// Setup database container.
    /// </summary>
    /// <returns>A task.</returns>
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        this.postgres = new PostgreSqlBuilder()
            .WithDatabase("kapitelshelf")
            .WithUsername("kapitelshelf")
            .WithPassword("kapitelshelf")
            .WithImage("postgres:16.8")
            .Build();

        await this.postgres.StartAsync();
    }

    /// <summary>
    /// Teardown database container.
    /// </summary>
    /// <returns>A task.</returns>
    [OneTimeTearDown]
    public async Task Cleanup() => await this.postgres.DisposeAsync();

    /// <summary>
    /// Sets up the testee and fakes before each test.
    /// </summary>
    /// <returns>A task.</returns>
    [SetUp]
    public async Task SetUp()
    {
        this.dbOptions = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseNpgsql(this.postgres.GetConnectionString(), x => x.MigrationsAssembly("KapitelShelf.Data.Migrations"))
            .Options;

        // datamigrations
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            await context.Database.MigrateAsync();
        }

        this.dbContextFactory = Substitute.For<IDbContextFactory<KapitelShelfDBContext>>();
        this.dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(new KapitelShelfDBContext(this.dbOptions)));

        this.mapper = Testhelper.CreateMapper();

        this.notificationsLocalizations = Substitute.For<ILocalizationProvider<Notifications>>();

        this.testee = new NotificationsLogic(this.dbContextFactory, this.mapper, this.notificationsLocalizations);
    }

    /// <summary>
    /// Tests AddNotification(localizationKey) uses localization provider and persists notification.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddNotification_UsesLocalizationKey_AndPersists()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        this.notificationsLocalizations.Get("TestKey_Title").Returns("Localized Title");
        this.notificationsLocalizations.Get("TestKey_Message").Returns("Localized Message");

        // Execute
        var result = await this.testee.AddNotification(
            localizationKey: "TestKey",
            type: NotificationTypeDto.Info,
            severity: NotificationSeverityDto.Low,
            source: "UnitTest",
            userId: userId);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Title, Is.EqualTo("Localized Title"));
            Assert.That(result[0].Message, Is.EqualTo("Localized Message"));
            Assert.That(result[0].Type, Is.EqualTo(NotificationTypeDto.Info));
            Assert.That(result[0].Severity, Is.EqualTo(NotificationSeverityDto.Low));
            Assert.That(result[0].Source, Is.EqualTo("UnitTest"));
        });

        using var context = new KapitelShelfDBContext(this.dbOptions);

        var saved = await context.Notifications.Where(x => x.Id == result[0].Id).FirstOrDefaultAsync();
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved.Title, Is.EqualTo("Localized Title"));
            Assert.That(saved.Message, Is.EqualTo("Localized Message"));
            Assert.That(saved.UserId, Is.EqualTo(userId));
            Assert.That(saved.ParentId, Is.Null);
            Assert.That(saved.IsRead, Is.False);
        });
    }

    /// <summary>
    /// Tests AddNotification(localizationKey) passes args to localization provider.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddNotification_UsesLocalizationArgs()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        this.notificationsLocalizations.Get("Key_Title", Arg.Any<object[]>()).Returns("T");
        this.notificationsLocalizations.Get("Key_Message", Arg.Any<object[]>()).Returns("M");

        // Execute
        _ = await this.testee.AddNotification(
            localizationKey: "Key",
            titleArgs: [123],
            messageArgs: ["abc"],
            userId: userId);

        // Assert
        _ = this.notificationsLocalizations.Received(1).Get("Key_Title", Arg.Is<object[]>(a => a.Length == 1 && (int)a[0] == 123));
        _ = this.notificationsLocalizations.Received(1).Get("Key_Message", Arg.Is<object[]>(a => a.Length == 1 && (string)a[0] == "abc"));
    }

    /// <summary>
    /// Tests AddNotification(title,message) creates a notification for a specific user.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddNotification_CreatesNotification_ForSpecificUser()
    {
        // Setup
        var userId = await this.InsertUserAsync();
        var expires = DateTime.UtcNow.AddHours(4);

        // Execute
        var result = await this.testee.AddNotification(
            title: "Hello",
            message: "World",
            type: NotificationTypeDto.Warning,
            severity: NotificationSeverityDto.Medium,
            expires: expires,
            source: "UnitTest",
            userId: userId);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Title, Is.EqualTo("Hello"));
            Assert.That(result[0].Message, Is.EqualTo("World"));
            Assert.That(result[0].Type, Is.EqualTo(NotificationTypeDto.Warning));
            Assert.That(result[0].Severity, Is.EqualTo(NotificationSeverityDto.Medium));
            Assert.That(result[0].Expires, Is.EqualTo(expires));
        });

        using var context = new KapitelShelfDBContext(this.dbOptions);

        var saved = await context.Notifications.Where(x => x.Id == result[0].Id).FirstOrDefaultAsync();
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved.UserId, Is.EqualTo(userId));
    }

    /// <summary>
    /// Tests AddNotification(title,message) with userId null creates notifications for all users.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddNotification_WhenUserIdNull_AddsToAllUsers()
    {
        // Setup
        var user1 = await this.InsertUserAsync();
        var user2 = await this.InsertUserAsync();

        // Execute
        var result = await this.testee.AddNotification(
            title: "Broadcast",
            message: "All users",
            userId: null);

        // Assert
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(2));

        using var context = new KapitelShelfDBContext(this.dbOptions);

        var resultIds = result.Select(x => x.Id).ToList();
        var saved = await context.Notifications.Where(x => resultIds.Contains(x.Id)).ToListAsync();
        Assert.That(saved, Has.Count.GreaterThanOrEqualTo(2));
        Assert.That(saved.Any(x => x.Title == "Broadcast"), Is.True);
    }

    /// <summary>
    /// Tests auto-grouping: same title (case-insensitive) creates child notification under existing parent.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddNotification_AutoGroupsByTitle_WhenExistingParentExists()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        // Execute
        var first = await this.testee.AddNotification(
            title: "Same Title",
            message: "First",
            userId: userId);

        var second = await this.testee.AddNotification(
            title: "same title", // different casing
            message: "Second",
            userId: userId);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(first, Has.Count.EqualTo(1));
            Assert.That(second, Has.Count.EqualTo(1));
        });

        using var context = new KapitelShelfDBContext(this.dbOptions);

        var all = await context.Notifications.AsNoTracking().OrderBy(x => x.Created).ToListAsync();
        Assert.That(all, Has.Count.EqualTo(2));

        var parent = all.Single(x => x.ParentId == null);
        var child = all.Single(x => x.ParentId != null);

        Assert.Multiple(() =>
        {
            Assert.That(child.ParentId, Is.EqualTo(parent.Id));
            Assert.That(parent.Title, Is.EqualTo("Same Title"));
            Assert.That(child.Title, Is.EqualTo("same title"));
        });
    }

    /// <summary>
    /// Tests disableAutoGrouping=true creates separate parent notifications.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddNotification_DisableAutoGrouping_CreatesSeparateParents()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        // Execute
        var notification1 = await this.testee.AddNotification(
            title: "Title",
            message: "1",
            userId: userId,
            disableAutoGrouping: true);

        var notification2 = await this.testee.AddNotification(
            title: "title",
            message: "2",
            userId: userId,
            disableAutoGrouping: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(notification1, Has.Count.EqualTo(1));
            Assert.That(notification2, Has.Count.EqualTo(1));
        });

        using var context = new KapitelShelfDBContext(this.dbOptions);

        Assert.Multiple(async () =>
        {
            var model1 = await context.Notifications.Where(x => x.Id == notification1[0].Id).FirstOrDefaultAsync();
            Assert.That(model1, Is.Not.Null);

            var model2 = await context.Notifications.Where(x => x.Id == notification2[0].Id).FirstOrDefaultAsync();
            Assert.That(model2, Is.Not.Null);

            Assert.That(model1!.ParentId, Is.Null);
            Assert.That(model2!.ParentId, Is.Null);
        });
    }

    /// <summary>
    /// Tests parentId explicitly forces children under that parent.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddNotification_WithParentId_AddsChildToParent()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        var parent = await this.testee.AddNotification(
            title: "Parent",
            message: "P",
            userId: userId);

        // Execute
        _ = await this.testee.AddNotification(
            title: "Child",
            message: "C",
            userId: userId,
            parentId: parent[0].Id);

        // Assert
        using var context = new KapitelShelfDBContext(this.dbOptions);

        var models = await context.Notifications.AsNoTracking().Where(x => x.Id == parent[0].Id || x.ParentId == parent[0].Id).ToListAsync();
        Assert.That(models, Has.Count.EqualTo(2));

        var parentModel = models.Single(x => x.ParentId == null);
        var childModel = models.Single(x => x.ParentId != null);

        Assert.That(childModel.ParentId, Is.EqualTo(parentModel.Id));
    }

    /// <summary>
    /// Tests GetByIdAsync returns only parent notifications for the user and includes children.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetByIdAsync_ReturnsParentWithChildren_WhenFound()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        var parent = await this.testee.AddNotification(
            title: "Parent",
            message: "P",
            userId: userId);

        _ = await this.testee.AddNotification(
            title: "Child1",
            message: "C1",
            userId: userId,
            parentId: parent[0].Id);

        // Execute
        var dto = await this.testee.GetByIdAsync(parent[0].Id, userId);

        // Assert
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto!.Id, Is.EqualTo(parent[0].Id));
            Assert.That(dto.Children, Has.Count.EqualTo(1));
        });
    }

    /// <summary>
    /// Tests GetByIdAsync returns null for child id (only parentId == null are returned).
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenIdIsChild()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        var parent = await this.testee.AddNotification("Parent", "P", userId: userId);
        var child = await this.testee.AddNotification("Child", "C", userId: userId, parentId: parent[0].Id);

        // Execute
        var dto = await this.testee.GetByIdAsync(child[0].Id, userId);

        // Assert
        Assert.That(dto, Is.Null);
    }

    /// <summary>
    /// Tests GetByUserIdAsync returns only parents ordered by Created desc and includes children.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetByUserIdAsync_ReturnsParents_OrderedByCreatedDesc()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        var first = await this.testee.AddNotification("First", "1", userId: userId);
        await Task.Delay(10); // ensure Created differs
        var second = await this.testee.AddNotification("Second", "2", userId: userId);

        _ = await this.testee.AddNotification("Child", "c", userId: userId, parentId: first[0].Id);

        // Execute
        var list = await this.testee.GetByUserIdAsync(userId);

        // Assert
        Assert.That(list, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(list[0].Title, Is.EqualTo(second[0].Title));
            Assert.That(list[1].Title, Is.EqualTo(first[0].Title));
            Assert.That(list[1].Children, Has.Count.EqualTo(1));
        });
    }

    /// <summary>
    /// Tests GetStatsAsync counts unread when parent unread OR any child unread.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStatsAsync_CountsUnread_WhenChildUnread()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        // parent read, child unread -> should count as unread
        var parent = await this.testee.AddNotification("Parent", "P", userId: userId);
        _ = await this.testee.AddNotification("Child", "C", userId: userId, parentId: parent[0].Id);

        // mark parent as read directly in db (children stay unread)
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var parentModel = await context.Notifications.SingleAsync(x => x.Id == parent[0].Id);
            parentModel.IsRead = true;
            await context.SaveChangesAsync();
        }

        // Execute
        var stats = await this.testee.GetStatsAsync(userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(stats.TotalMessages, Is.EqualTo(1));
            Assert.That(stats.UnreadCount, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Tests GetStatsAsync unreadHasCritical checks severity on parent records.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStatsAsync_UnreadHasCritical_IsTrue_WhenUnreadCriticalParentExists()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        _ = await this.testee.AddNotification(
            title: "Critical",
            message: "!",
            severity: NotificationSeverityDto.Critical,
            userId: userId);

        // Execute
        var stats = await this.testee.GetStatsAsync(userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(stats.TotalMessages, Is.EqualTo(1));
            Assert.That(stats.UnreadCount, Is.EqualTo(1));
            Assert.That(stats.UnreadHasCritical, Is.True);
        });
    }

    /// <summary>
    /// Tests MarkAllAsReadAsync marks unread parents as read and returns true when something changed.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task MarkAllAsReadAsync_MarksUnread_AsRead()
    {
        // Setup
        var userId = await this.InsertUserAsync();
        _ = await this.testee.AddNotification("A", "a", userId: userId);

        // Execute
        var changed = await this.testee.MarkAllAsReadAsync(userId);

        // Assert
        Assert.That(changed, Is.True);

        using var context = new KapitelShelfDBContext(this.dbOptions);

        var models = await context.Notifications.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
        Assert.That(models.All(x => x.IsRead), Is.True);
    }

    /// <summary>
    /// Tests MarkAllAsReadAsync returns false when nothing to update.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task MarkAllAsReadAsync_ReturnsFalse_WhenNothingUnread()
    {
        // Setup
        var userId = await this.InsertUserAsync();
        _ = await this.testee.AddNotification("A", "a", userId: userId);
        _ = await this.testee.MarkAllAsReadAsync(userId);

        // Execute
        var changed = await this.testee.MarkAllAsReadAsync(userId);

        // Assert
        Assert.That(changed, Is.False);
    }

    /// <summary>
    /// Tests MarkAsReadAsync marks one notification as read and returns true when exactly one row is affected.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task MarkAsReadAsync_ReturnsTrue_WhenOnlyParentIsUpdated()
    {
        // Setup
        var userId = await this.InsertUserAsync();
        var parent = await this.testee.AddNotification("Parent", "P", userId: userId);

        // Execute
        var ok = await this.testee.MarkAsReadAsync(parent[0].Id, userId);

        // Assert
        Assert.That(ok, Is.True);
    }

    /// <summary>
    /// Tests MarkAsReadAsync marks parent + children but returns false when more than one row is affected (current implementation).
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task MarkAsReadAsync_ReturnsFalse_WhenParentAndChildAreUpdated()
    {
        // Setup
        var userId = await this.InsertUserAsync();
        var parent = await this.testee.AddNotification("Parent", "P", userId: userId);
        _ = await this.testee.AddNotification("Child", "C", userId: userId, parentId: parent[0].Id);

        // Execute
        var ok = await this.testee.MarkAsReadAsync(parent[0].Id, userId);

        // Assert
        Assert.That(ok, Is.False);

        using var context = new KapitelShelfDBContext(this.dbOptions);

        var models = await context.Notifications.AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        Assert.That(models.All(x => x.IsRead), Is.True);
    }

    /// <summary>
    /// Tests MarkAsReadAsync returns false when notification does not exist for that user.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task MarkAsReadAsync_ReturnsFalse_WhenNotFound()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        // Execute
        var ok = await this.testee.MarkAsReadAsync(Guid.NewGuid(), userId);

        // Assert
        Assert.That(ok, Is.False);
    }

    /// <summary>
    /// Tests DeleteExpiredNotificationsAsync removes expired notifications.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteExpiredNotificationsAsync_DeletesExpired()
    {
        // Setup
        var userId = await this.InsertUserAsync();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Notifications.Add(new NotificationModel
            {
                Title = "Expired",
                Message = "x",
                Type = NotificationType.Info,
                Severity = NotificationSeverity.Low,
                Created = DateTime.UtcNow.AddDays(-10),
                Expires = DateTime.UtcNow.AddMinutes(-1),
                IsRead = false,
                Source = "UnitTest",
                UserId = userId,
            });

            context.Notifications.Add(new NotificationModel
            {
                Title = "Active",
                Message = "y",
                Type = NotificationType.Info,
                Severity = NotificationSeverity.Low,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(5),
                IsRead = false,
                Source = "UnitTest",
                UserId = userId,
            });

            await context.SaveChangesAsync();
        }

        // Execute
        await this.testee.DeleteExpiredNotificationsAsync();

        // Assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var expiredCount = await context.Notifications.AsNoTracking().Where(x => x.Expires <= DateTime.UtcNow).CountAsync();
            Assert.That(expiredCount, Is.EqualTo(0));
        }
    }

    private async Task<Guid> InsertUserAsync()
    {
        var id = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.Add(new UserModel
            {
                Id = id,
                Username = $"User_{Guid.NewGuid()}",
                Image = ProfileImageType.MonsieurRead,
                Color = "#33ffff",
            });

            await context.SaveChangesAsync();
        }

        return id;
    }
}
