// <copyright file="UsersLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.DTOs.User;
using KapitelShelf.Api.Logic;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.User;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the UsersLogic class.
/// </summary>
[TestFixture]
public class UsersLogicTests
{
    private PostgreSqlContainer postgres;
    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private Mapper mapper;
    private UsersLogic testee;

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
    /// Sets up a new test database and fakes before each test.
    /// </summary>
    /// <returns>A task.</returns>
    [SetUp]
    public async Task SetUp()
    {
        this.dbOptions = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseNpgsql(this.postgres.GetConnectionString(), x => x.MigrationsAssembly("KapitelShelf.Data.Migrations"))
            .Options;

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            await context.Database.MigrateAsync();
        }

        this.dbContextFactory = Substitute.For<IDbContextFactory<KapitelShelfDBContext>>();
        this.dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(new KapitelShelfDBContext(this.dbOptions)));

        this.mapper = Testhelper.CreateMapper();
        this.testee = new UsersLogic(this.dbContextFactory, this.mapper);
    }

    /// <summary>
    /// Tests GetUsersAsync returns all users.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetUsersAsync_ReturnsAllUsers()
    {
        // Setup
        var user1 = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "user1".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#33ffff",
        };
        var user2 = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "user2".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#33ffff",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();
        }

        // Execute
        var users = await this.testee.GetUsersAsync();

        // Assert
        Assert.That(users, Is.Not.Null);
        Assert.That(users, Has.Count.GreaterThanOrEqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(users.Any(u => u.Username == user1.Username), Is.True);
            Assert.That(users.Any(u => u.Username == user2.Username), Is.True);
        });
    }

    /// <summary>
    /// Tests GetUserByIdAsync returns user when found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetUserByIdAsync_ReturnsUser_WhenFound()
    {
        // Setup
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "founduser".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#33ffff",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.GetUserByIdAsync(user.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo(user.Username));
    }

    /// <summary>
    /// Tests GetUserByIdAsync returns null when not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetUserByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Execute
        var result = await this.testee.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests CreateUserAsync creates and returns user.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CreateUserAsync_CreatesAndReturnsUser()
    {
        // Setup
        var createDto = new CreateUserDTO
        {
            Username = "newuser".Unique(),
            Image = ProfileImageTypeDTO.MonsieurRead,
            Color = "#33ffff",
        };

        // Execute
        var result = await this.testee.CreateUserAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo(createDto.Username));
        using var context = new KapitelShelfDBContext(this.dbOptions);
        Assert.That(context.Users.Any(u => u.Username == createDto.Username), Is.True);
    }

    /// <summary>
    /// Tests CreateUserAsync returns null if input is null.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CreateUserAsync_ReturnsNull_IfInputNull()
    {
        // Execute
        var result = await this.testee.CreateUserAsync(null!);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests UpdateUserAsync updates and returns user.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateUserAsync_UpdatesAndReturnsUser()
    {
        // Setup
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "beforeupdate".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#33ffff",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var updatedDto = new UserDTO
        {
            Id = user.Id,
            Username = "afterupdate".Unique(),
            Image = ProfileImageTypeDTO.MonsieurRead,
            Color = "#33ffff",
        };

        // Execute
        var result = await this.testee.UpdateUserAsync(user.Id, updatedDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo(updatedDto.Username));
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Users.Single(u => u.Id == user.Id).Username, Is.EqualTo(updatedDto.Username));
        }
    }

    /// <summary>
    /// Tests UpdateUserAsync returns null if user not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateUserAsync_ReturnsNull_IfUserNotFound()
    {
        // Setup
        var updatedDto = new UserDTO
        {
            Id = Guid.NewGuid(),
            Username = "notfound".Unique(),
            Image = ProfileImageTypeDTO.MonsieurRead,
            Color = "#33ffff",
        };

        // Execute
        var result = await this.testee.UpdateUserAsync(updatedDto.Id, updatedDto);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests UpdateUserAsync returns null if input is null.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateUserAsync_ReturnsNull_IfInputNull()
    {
        // Setup
        var id = Guid.NewGuid();

        // Execute
        var result = await this.testee.UpdateUserAsync(id, null!);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests DeleteUserAsync removes user and returns DTO.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteUserAsync_RemovesUserAndReturnsDto()
    {
        // Setup
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "deleteuser".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#33ffff",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.DeleteUserAsync(user.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo(user.Username));
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Users.FirstOrDefault(u => u.Id == user.Id), Is.Null);
        }
    }

    /// <summary>
    /// Tests DeleteUserAsync returns null if user not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteUserAsync_ReturnsNull_IfUserNotFound()
    {
        // Execute
        var result = await this.testee.DeleteUserAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests GetLastVisitedBooksAsync returns paginated, ordered results.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetLastVisitedBooksAsync_ReturnsPaginatedOrderedBooks()
    {
        // Setup
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "visiteduser".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#abcdef",
        };

        var series = new SeriesModel
        {
            Id = Guid.NewGuid(),
            Name = "Series 1".Unique(),
        };

        var book1 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book 1",
            Description = "Desc",
            Author = new AuthorModel
            {
                FirstName = "John",
                LastName = "Doe",
            },
            Series = series,
        };

        var book2 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book 2",
            Description = "Desc",
            Author = new AuthorModel
            {
                FirstName = "Jane",
                LastName = "Smith",
            },
            Series = series,
        };

        var visit1 = new VisitedBooksModel
        {
            UserId = user.Id,
            BookId = book1.Id,
            VisitedAt = DateTime.UtcNow.AddMinutes(-10),
            Book = book1,
        };

        var visit2 = new VisitedBooksModel
        {
            UserId = user.Id,
            BookId = book2.Id,
            VisitedAt = DateTime.UtcNow.AddMinutes(-5),
            Book = book2,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.Add(user);
            context.Series.Add(series);
            context.Books.AddRange(book1, book2);
            context.VisitedBooks.AddRange(visit1, visit2);
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.GetLastVisitedBooksAsync(user.Id, page: 1, pageSize: 10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(2));
            Assert.That(result.TotalCount, Is.EqualTo(2));
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Items[0].Title, Is.EqualTo("Book 2")); // newest first
            Assert.That(result.Items[1].Title, Is.EqualTo("Book 1"));
        });
    }

    /// <summary>
    /// Tests GetSettingsByUserIdAsync returns user settings.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetSettingsByUserIdAsync_ReturnsSettings()
    {
        // Setup
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "user_with_settings".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#33ffff",
        };
        var settings = new List<UserSettingModel>
        {
            new() { Key = "test1", Value = "abc", Type = UserSettingValueType.TString, UserId = user.Id },
            new() { Key = "test2", Value = "123", Type = UserSettingValueType.TInteger, UserId = user.Id },
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.Add(user);
            context.UserSettings.AddRange(settings);
            await context.SaveChangesAsync();
        }

        // Execute
        var result = await this.testee.GetSettingsByUserIdAsync(user.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(result.Any(x => x.Key == "test1" && x.Value == "abc"), Is.True);
            Assert.That(result.Any(x => x.Key == "test2" && x.Value == "123"), Is.True);
        });
    }

    /// <summary>
    /// Tests UpdateSettingAsync adds a new setting.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateSettingAsync_AddsNewSetting()
    {
        // Setup
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "user_add_setting".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#33ffff",
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var setting = new UserSettingDTO
        {
            Key = "test_add",
            Value = "true",
            Type = UserSettingValueTypeDTO.TBoolean,
        };

        // Execute
        await this.testee.UpdateSettingAsync(user.Id, setting);

        // Assert
        using var context2 = new KapitelShelfDBContext(this.dbOptions);
        var saved = await context2.UserSettings.FirstOrDefaultAsync(s => s.UserId == user.Id && s.Key == "test_add");
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved!.Value, Is.EqualTo("true"));
            Assert.That(saved.Type, Is.EqualTo(UserSettingValueType.TBoolean));
        });
    }

    /// <summary>
    /// Tests UpdateSettingAsync updates an existing setting.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateSettingAsync_UpdatesExistingSetting()
    {
        // Setup
        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Username = "user_update_settings".Unique(),
            Image = ProfileImageType.MonsieurRead,
            Color = "#33ffff",
        };
        var settings = new List<UserSettingModel>
        {
            new() { Key = "test_update", Value = "false", Type = UserSettingValueType.TBoolean, UserId = user.Id },
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Users.Add(user);
            context.UserSettings.AddRange(settings);
            await context.SaveChangesAsync();
        }

        var updatedSetting = new UserSettingDTO
        {
            Key = "test_update",
            Value = "true",
            Type = UserSettingValueTypeDTO.TBoolean,
        };

        // Execute
        await this.testee.UpdateSettingAsync(user.Id, updatedSetting);

        // Assert
        using var context2 = new KapitelShelfDBContext(this.dbOptions);
        var saved = await context2.UserSettings.FirstOrDefaultAsync(s => s.UserId == user.Id && s.Key == "test_update");
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved!.Value, Is.EqualTo("true"));
            Assert.That(saved.Type, Is.EqualTo(UserSettingValueType.TBoolean));
        });
    }
}
