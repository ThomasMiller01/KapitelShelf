// <copyright file="DynamicSettingsManagerTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Logic;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Tests for <see cref="DynamicSettingsManager"/>.
/// </summary>
public class DynamicSettingsManagerTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;

    private Mapper mapper;
    private DynamicSettingsManager testee;

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

        this.testee = new DynamicSettingsManager(this.dbContextFactory, this.mapper);
    }

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.AddIfNotExists{T}"/> adds a new setting when none exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddIfNotExists_AddsSetting_WhenNotExists()
    {
        // Setup
        var key = "test-key".Unique();
        var value = true;

        // Execute
        await this.testee.AddIfNotExists(key, value);

        // Assert
        var context = await this.dbContextFactory.CreateDbContextAsync();
        var setting = await context.Settings.FirstOrDefaultAsync(x => x.Key == key);

        Assert.That(setting, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(setting!.Key, Is.EqualTo(key));
            Assert.That(setting.Value, Is.EqualTo("True"));
            Assert.That(setting.Type, Is.EqualTo(SettingsValueType.TBoolean));
        });
    }

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.AddIfNotExists{T}"/> does not add duplicate settings.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AddIfNotExists_DoesNothing_WhenExists()
    {
        // Setup
        var key = "test-key".Unique();
        var context = await this.dbContextFactory.CreateDbContextAsync();
        await context.Settings.AddAsync(new SettingsModel
        {
            Key = key,
            Value = "False",
            Type = SettingsValueType.TBoolean,
        });
        await context.SaveChangesAsync();

        // Execute
        await this.testee.AddIfNotExists(key, true);

        // Assert
        var settings = await context.Settings.ToListAsync();
        var setting = settings.FirstOrDefault(x => x.Key == key);
        Assert.Multiple(() =>
        {
            Assert.That(setting, Is.Not.Null);
            Assert.That(setting?.Value, Is.EqualTo("False"));
        });
    }

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.GetAsync{T}"/> returns mapped DTO if setting exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetAsync_ReturnsMappedDto_WhenExists()
    {
        // Setup
        var key = "test-key".Unique();
        var context = await this.dbContextFactory.CreateDbContextAsync();
        var model = new SettingsModel
        {
            Key = key,
            Value = "True",
            Type = SettingsValueType.TBoolean,
        };
        await context.Settings.AddAsync(model);
        await context.SaveChangesAsync();

        // Execute
        var result = await this.testee.GetAsync<bool>(key);

        // Assert
        Assert.That(result.Value, Is.True);
    }

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.GetAsync{T}"/> throws if setting not found.
    /// </summary>
    [Test]
    public void GetAsync_Throws_WhenNotFound()
    {
        // Setup
        var key = "not-found".Unique();

        // Execute / Assert
        Assert.ThrowsAsync<KeyNotFoundException>(() => this.testee.GetAsync<bool>(key));
    }

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.MapTypeToValueType{T}"/> returns correct mapping.
    /// </summary>
    [Test]
    public void MapTypeToValueType_ReturnsTBoolean_ForBool()
    {
        // Execute
        var result = DynamicSettingsManager.MapTypeToValueType(true);

        // Assert
        Assert.That(result, Is.EqualTo(SettingsValueType.TBoolean));
    }

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.MapTypeToValueType{T}"/> throws for unsupported type.
    /// </summary>
    [Test]
    public void MapTypeToValueType_Throws_ForInvalidType() =>
        Assert.Throws<InvalidOperationException>(() => DynamicSettingsManager.MapTypeToValueType("string"));

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.ConvertSettingValue{T}"/> converts boolean value.
    /// </summary>
    [Test]
    public void ConvertSettingValue_ReturnsBool_WhenValid()
    {
        // Setup
        var setting = new SettingsModel
        {
            Key = "key",
            Value = "True",
            Type = SettingsValueType.TBoolean,
        };

        // Execute
        var result = DynamicSettingsManager.ConvertSettingValue<bool>(setting);

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.ConvertSettingValue{T}"/> throws for invalid type.
    /// </summary>
    [Test]
    public void ConvertSettingValue_Throws_ForInvalidType()
    {
        // Setup
        var setting = new SettingsModel
        {
            Key = "key",
            Value = "True",
            Type = (SettingsValueType)999,
        };

        // Execute / Assert
        Assert.Throws<InvalidOperationException>(() => DynamicSettingsManager.ConvertSettingValue<bool>(setting));
    }

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.ValidateValue"/> returns true for valid boolean string.
    /// </summary>
    [Test]
    public void ValidateValue_ReturnsTrue_ForValidBoolean()
    {
        // Setup
        var setting = new SettingsModel
        {
            Key = "key",
            Type = SettingsValueType.TBoolean,
        };

        // Execute
        var result = DynamicSettingsManager.ValidateValue(setting, "true");

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.ValidateValue"/> returns false for invalid boolean string.
    /// </summary>
    [Test]
    public void ValidateValue_ReturnsFalse_ForInvalidBoolean()
    {
        // Setup
        var setting = new SettingsModel
        {
            Key = "key",
            Type = SettingsValueType.TBoolean,
        };

        // Execute
        var result = DynamicSettingsManager.ValidateValue(setting, "not-bool");

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests <see cref="DynamicSettingsManager.ValidateValue"/> returns false if value is null.
    /// </summary>
    [Test]
    public void ValidateValue_ReturnsFalse_ForNullValue()
    {
        // Setup
        var setting = new SettingsModel
        {
            Key = "key",
            Type = SettingsValueType.TBoolean,
        };

        // Execute
        var result = DynamicSettingsManager.ValidateValue(setting, null);

        // Assert
        Assert.That(result, Is.False);
    }
}
