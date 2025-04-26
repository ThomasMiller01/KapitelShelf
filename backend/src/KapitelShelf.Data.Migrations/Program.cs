// <copyright file="Program.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text.Json;
using KapitelShelf.Data;
using KapitelShelf.Data.Migrations.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// configuration settings
var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

var settings = config.GetSection("KapitelShelf").Get<KapitelShelfSettings>()!;
#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
Console.WriteLine(JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances

var options = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseNpgsql(
                $"Host={settings.Database.Host};Database={StaticConstants.DatabaseName};Username={settings.Database.Username};Password={settings.Database.Password}",
                b => b.MigrationsAssembly("KapitelShelf.Data.Migrations"))
            .Options;

using var context = new KapitelShelfDBContext(options);

context.Database.Migrate();

Console.WriteLine("Migrations applied.");
