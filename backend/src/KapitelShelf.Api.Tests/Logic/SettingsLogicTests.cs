// <copyright file="SettingsLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Tests for <see cref="SettingsLogic"/>.
/// </summary>
public class SettingsLogicTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private IMapper mapper;
    private SettingsLogic testee;

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

        this.testee = new SettingsLogic(this.dbContextFactory, this.mapper);
    }

    /// <summary>
    /// Tests <see cref="SettingsLogic.GetSettingsAsync"/> returns mapped settings.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetSettingsAsync_ReturnsAllSettings()
    {
        // Setup
        var context = await this.dbContextFactory.CreateDbContextAsync();
        var model = new SettingsModel
        {
            Id = Guid.NewGuid(),
            Key = "test-key".Unique(),
            Value = "True",
            Type = SettingsValueType.TBoolean,
        };
        await context.Settings.AddAsync(model);
        await context.SaveChangesAsync();

        // Execute
        var result = await this.testee.GetSettingsAsync();
        var setting = await context.Settings.FirstOrDefaultAsync(x => x.Key == model.Key);

        // Assert
        Assert.That(setting, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(setting!.Key, Is.EqualTo(model.Key));
            Assert.That(setting.Value, Is.EqualTo(model.Value));
            Assert.That(setting.Type, Is.EqualTo(model.Type));
        });
    }

    /// <summary>
    /// Tests <see cref="SettingsLogic.UpdateSettingAsync"/> returns null if value is null.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateSettingAsync_ReturnsNull_WhenValueIsNull()
    {
        // Setup
        var id = Guid.NewGuid();

        // Execute
        var result = await this.testee.UpdateSettingAsync(id, null!);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests <see cref="SettingsLogic.UpdateSettingAsync"/> returns null if setting not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateSettingAsync_ReturnsNull_WhenNotFound()
    {
        // Setup
        var id = Guid.NewGuid();

        // Execute
        var result = await this.testee.UpdateSettingAsync(id, true);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests <see cref="SettingsLogic.UpdateSettingAsync"/> updates setting if value is valid.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateSettingAsync_UpdatesValue_WhenValid()
    {
        // Setup
        var context = await this.dbContextFactory.CreateDbContextAsync();
        var model = new SettingsModel
        {
            Id = Guid.NewGuid(),
            Key = "test-key".Unique(),
            Value = "False",
            Type = SettingsValueType.TBoolean,
        };
        await context.Settings.AddAsync(model);
        await context.SaveChangesAsync();

        // Execute
        var result = await this.testee.UpdateSettingAsync(model.Id, true);

        // Assert
        var updated = await context.Settings.FirstAsync(x => x.Id == model.Id);
        Assert.That(updated.Value, Is.EqualTo("False"));
    }

    /// <summary>
    /// Tests <see cref="SettingsLogic.UpdateSettingAsync"/> throws if value type is invalid.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateSettingAsync_Throws_WhenInvalidValue()
    {
        // Setup
        var context = await this.dbContextFactory.CreateDbContextAsync();
        var model = new SettingsModel
        {
            Id = Guid.NewGuid(),
            Key = "test-key".Unique(),
            Value = "False",
            Type = SettingsValueType.TBoolean,
        };
        await context.Settings.AddAsync(model);
        await context.SaveChangesAsync();

        // Execute / Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => this.testee.UpdateSettingAsync(model.Id, "not-bool"));

        Assert.That(ex!.Message, Is.EqualTo(StaticConstants.InvalidSettingValueType));
    }
}
