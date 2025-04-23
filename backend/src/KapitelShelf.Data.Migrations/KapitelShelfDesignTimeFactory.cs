// <copyright file="KapitelShelfDesignTimeFactory.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KapitelShelf.Data.Migrations;

/// <summary>
/// The KapitelShelf design time factory.
/// </summary>
public class KapitelShelfDesignTimeFactory : IDesignTimeDbContextFactory<KapitelShelfDBContext>
{
    /// <summary>
    /// Create a new db context.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>A db context.</returns>
    public KapitelShelfDBContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseNpgsql(
                "Host=localhost;Database=kapitelshelf;Username=kapitelshelf;Password=kapitelshelf",
                b => b.MigrationsAssembly("KapitelShelf.Data.Migrations"))
            .Options;

        return new KapitelShelfDBContext(options);
    }
}
