// <copyright file="SettingsLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Settings;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Quartz;
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
    private Mapper mapper;
    private ISchedulerFactory schedulerFactory;
    private IDynamicSettingsManager settingsManager;
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
        this.schedulerFactory = Substitute.For<ISchedulerFactory>();
        this.settingsManager = Substitute.For<IDynamicSettingsManager>();

        this.testee = new SettingsLogic(this.dbContextFactory, this.mapper, this.schedulerFactory, this.settingsManager);
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
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => this.testee.UpdateSettingAsync(model.Id, "not-bool"));

        Assert.That(ex!.Message, Is.EqualTo(StaticConstants.InvalidSettingValueType));
    }

    /// <summary>
    /// Tests <see cref="SettingsLogic.UpdateSettingAsync"/> triggers side effects when the updated key is in the AI provider side-effect list.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateSettingAsync_TriggersAiProviderSideEffects_WhenKeyIsAiProviderKey()
    {
        // Setup
        var context = await this.dbContextFactory.CreateDbContextAsync();

        var setting = new SettingsModel
        {
            Id = Guid.NewGuid(),
            Key = StaticConstants.DynamicSettingAiProvider,
            Value = "\"ollama\"",
            Type = SettingsValueType.TString,
        };

        await context.Settings.AddAsync(setting);
        await context.SaveChangesAsync();

        var scheduler = Substitute.For<IScheduler>();
        this.schedulerFactory.GetScheduler().Returns(Task.FromResult(scheduler));

        // Execute
        _ = await this.testee.UpdateSettingAsync(setting.Id, "openai");

        // Assert
        await this.settingsManager.Received(1).SetAsync(StaticConstants.DynamicSettingAiProviderConfigured, false);
        _ = await this.schedulerFactory.Received(1).GetScheduler();
    }

    /// <summary>
    /// Tests <see cref="SettingsLogic.UpdateSettingAsync"/> does not trigger side effects when the updated key is not in the AI provider side-effect list.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateSettingAsync_DoesNotTriggerSideEffects_WhenKeyIsNotAiProviderKey()
    {
        // Setup
        var context = await this.dbContextFactory.CreateDbContextAsync();

        var setting = new SettingsModel
        {
            Id = Guid.NewGuid(),
            Key = "some-other-key".Unique(),
            Value = "\"x\"",
            Type = SettingsValueType.TString,
        };

        await context.Settings.AddAsync(setting);
        await context.SaveChangesAsync();

        // Execute
        _ = await this.testee.UpdateSettingAsync(setting.Id, "y");

        // Assert
        await this.settingsManager.DidNotReceiveWithAnyArgs().SetAsync<string>(default!, default!);
        _ = await this.schedulerFactory.DidNotReceive().GetScheduler();
    }

    /// <summary>
    /// Tests <see cref="SettingsLogic.SettingUpdateSideEffectsAsync"/> triggers AI provider configuration when the key is in the side-effect list.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task SettingUpdateSideEffectsAsync_TriggersAiProviderConfiguration_WhenKeyIsAiProviderKey()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        this.schedulerFactory.GetScheduler().Returns(Task.FromResult(scheduler));

        var settingDto = new SettingsDTO<object>
        {
            Id = Guid.NewGuid(),
            Key = StaticConstants.DynamicSettingAiOllamaModel,
            Value = "llama3.1",
        };

        // Execute
        await this.testee.SettingUpdateSideEffectsAsync(settingDto);

        // Assert
        await this.settingsManager.Received(1).SetAsync(StaticConstants.DynamicSettingAiProviderConfigured, false);
        _ = await this.schedulerFactory.Received(1).GetScheduler();
    }

    /// <summary>
    /// Tests <see cref="SettingsLogic.SettingUpdateSideEffectsAsync"/> does nothing when the key is not in the side-effect list.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task SettingUpdateSideEffectsAsync_DoesNothing_WhenKeyIsNotAiProviderKey()
    {
        // Setup
        var settingDto = new SettingsDTO<object>
        {
            Id = Guid.NewGuid(),
            Key = "not-ai-related",
            Value = "x",
        };

        // Execute
        await this.testee.SettingUpdateSideEffectsAsync(settingDto);

        // Assert
        await this.settingsManager.DidNotReceiveWithAnyArgs().SetAsync<string>(default!, default!);
        _ = await this.schedulerFactory.DidNotReceive().GetScheduler();
    }

    /// <summary>
    /// Tests <see cref="SettingsLogic.SettingUpdateSideEffectsAsync"/> still sets provider configured false even if scheduling throws.
    /// </summary>
    [Test]
    public void SettingUpdateSideEffectsAsync_SetsConfiguredFalse_EvenIfSchedulingThrows()
    {
        // Setup
        this.schedulerFactory.GetScheduler().Throws(new InvalidOperationException("scheduler failed"));

        var settingDto = new SettingsDTO<object>
        {
            Id = Guid.NewGuid(),
            Key = StaticConstants.DynamicSettingAiProvider,
            Value = "ollama",
        };

        // Execute / Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => this.testee.SettingUpdateSideEffectsAsync(settingDto));

        this.settingsManager.Received(1).SetAsync(StaticConstants.DynamicSettingAiProviderConfigured, false);
    }
}
