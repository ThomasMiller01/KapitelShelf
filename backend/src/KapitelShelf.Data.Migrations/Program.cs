// <copyright file="Program.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Data;
using KapitelShelf.Data.Migrations.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// configuration settings
var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

var settings = config.GetSection("KapitelShelf").Get<KapitelShelfSettings>()!;

var options = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseNpgsql(
                $"Host={settings.Database.Host};Database={StaticConstants.DatabaseName};Username={settings.Database.Username};Password={settings.Database.Password}",
                b => b.MigrationsAssembly("KapitelShelf.Data.Migrations"))
            .Options;

using var context = new KapitelShelfDBContext(options);

context.Database.Migrate();

Console.WriteLine("Migrations applied.");
