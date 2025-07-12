// <copyright file="KapitelShelfDBContext.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.PostgreSQL;
using KapitelShelf.Data.Extensions;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Data;

/// <summary>
/// The KapiteShelf db context.
/// </summary>
/// <param name="options">The db context options.</param>
public class KapitelShelfDBContext(DbContextOptions<KapitelShelfDBContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the books table.
    /// </summary>
    public DbSet<BookModel> Books => Set<BookModel>();

    /// <summary>
    /// Gets the fileinfos table.
    /// </summary>
    public DbSet<FileInfoModel> FileInfos => Set<FileInfoModel>();

    /// <summary>
    /// Gets the series table.
    /// </summary>
    public DbSet<SeriesModel> Series => Set<SeriesModel>();

    /// <summary>
    /// Gets the authors table.
    /// </summary>
    public DbSet<AuthorModel> Authors => Set<AuthorModel>();

    /// <summary>
    /// Gets the locations table.
    /// </summary>
    public DbSet<LocationModel> Locations => Set<LocationModel>();

    /// <summary>
    /// Gets the tags table.
    /// </summary>
    public DbSet<TagModel> Tags => Set<TagModel>();

    /// <summary>
    /// Gets the booktags table.
    /// </summary>
    public DbSet<BookTagModel> BookTags => Set<BookTagModel>();

    /// <summary>
    /// Gets the categories table.
    /// </summary>
    public DbSet<CategoryModel> Categories => Set<CategoryModel>();

    /// <summary>
    /// Gets the bookcategories table.
    /// </summary>
    public DbSet<BookCategoryModel> BookCategories => Set<BookCategoryModel>();

    /// <summary>
    /// Gets the users table.
    /// </summary>
    public DbSet<UserModel> Users => Set<UserModel>();

    /// <summary>
    /// Gets the user book metadata table.
    /// </summary>
    public DbSet<UserBookMetadataModel> UserBookMetadata => Set<UserBookMetadataModel>();

    /// <summary>
    /// Gets the visited books table.
    /// </summary>
    public DbSet<VisitedBooksModel> VisitedBooks => Set<VisitedBooksModel>();

    /// <summary>
    /// Gets the book search view.
    /// </summary>
    public DbSet<BookSearchView> BookSearchView => Set<BookSearchView>();

    /// <summary>
    /// On model creating.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
#pragma warning disable CA1062 // Validate arguments of public methods
        // Books
        modelBuilder.Entity<BookModel>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookModel>()
            .HasOne(b => b.Series)
            .WithMany(s => s.Books)
            .HasForeignKey(b => b.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        // Book Search View
        modelBuilder.Entity<BookSearchView>()
            .HasNoKey()
            .ToView("BookSearchView")

            .HasOne(v => v.BookModel)
            .WithMany()
            .HasForeignKey(v => v.Id)
            .HasPrincipalKey(b => b.Id);

        // Categories
        modelBuilder.Entity<BookCategoryModel>()
            .HasKey(bt => new { bt.BookId, bt.CategoryId });

        modelBuilder.Entity<BookCategoryModel>()
            .HasOne(bc => bc.Book)
            .WithMany(b => b.Categories)
            .HasForeignKey(bc => bc.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookCategoryModel>()
            .HasOne(bc => bc.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(bc => bc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CategoryModel>()
            .HasIndex(c => new { c.Name })
            .IsUnique();

        // Tags
        modelBuilder.Entity<BookTagModel>()
            .HasKey(bt => new { bt.BookId, bt.TagId });

        modelBuilder.Entity<BookTagModel>()
            .HasOne(bt => bt.Book)
            .WithMany(b => b.Tags)
            .HasForeignKey(bt => bt.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookTagModel>()
            .HasOne(bt => bt.Tag)
            .WithMany(t => t.Books)
            .HasForeignKey(bt => bt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TagModel>()
            .HasIndex(t => new { t.Name })
            .IsUnique();

        // Users
        modelBuilder.Entity<UserBookMetadataModel>()
            .HasOne(ub => ub.Book)
            .WithMany()
            .HasForeignKey(ub => ub.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserBookMetadataModel>()
            .HasOne(ub => ub.User)
            .WithMany()
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Author
        modelBuilder.Entity<AuthorModel>()
            .HasIndex(a => new { a.FirstName, a.LastName })
            .IsUnique();

        // Series
        modelBuilder.Entity<SeriesModel>()
            .HasIndex(c => new { c.Name })
            .IsUnique();

        // Visited Books
        modelBuilder.Entity<VisitedBooksModel>()
            .HasOne(b => b.Book)
            .WithMany()
            .HasForeignKey(b => b.BookId)
            .OnDelete(DeleteBehavior.Cascade);
#pragma warning restore CA1062 // Validate arguments of public methods

        base.OnModelCreating(modelBuilder);

        // register extension methods
        modelBuilder.HasDbFunction(() => PgTrgmExtensions.Similarity(default!, default!));

        // background tasks migration for Quartz.NET
        modelBuilder.AddQuartz(builder => builder.UsePostgreSql());
    }
}
