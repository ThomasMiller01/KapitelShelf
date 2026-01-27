// <copyright file="KapitelShelfDBContext.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.PostgreSQL;
using KapitelShelf.Data.Extensions;
using KapitelShelf.Data.Models;
using KapitelShelf.Data.Models.CloudStorage;
using KapitelShelf.Data.Models.Notifications;
using KapitelShelf.Data.Models.User;
using KapitelShelf.Data.Models.Watchlists;
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
    /// Gets the user settings table.
    /// </summary>
    public DbSet<UserSettingModel> UserSettings => Set<UserSettingModel>();

    /// <summary>
    /// Gets the user book metadata table.
    /// </summary>
    public DbSet<UserBookMetadataModel> UserBookMetadata => Set<UserBookMetadataModel>();

    /// <summary>
    /// Gets the visited books table.
    /// </summary>
    public DbSet<VisitedBooksModel> VisitedBooks => Set<VisitedBooksModel>();

    /// <summary>
    /// Gets the visited books table.
    /// </summary>
    public DbSet<CloudConfigurationModel> CloudConfiguration => Set<CloudConfigurationModel>();

    /// <summary>
    /// Gets the visited books table.
    /// </summary>
    public DbSet<CloudStorageModel> CloudStorages => Set<CloudStorageModel>();

    /// <summary>
    /// Gets the failed cloud file imports table.
    /// </summary>
    public DbSet<FailedCloudFileImportModel> FailedCloudFileImports => Set<FailedCloudFileImportModel>();

    /// <summary>
    /// Gets the book search view.
    /// </summary>
    public DbSet<BookSearchView> BookSearchView => Set<BookSearchView>();

    /// <summary>
    /// Gets the settings table.
    /// </summary>
    public DbSet<SettingsModel> Settings => Set<SettingsModel>();

    /// <summary>
    /// Gets the series watchlist table.
    /// </summary>
    public DbSet<WatchlistModel> Watchlist => Set<WatchlistModel>();

    /// <summary>
    /// Gets the series watchlist results table.
    /// </summary>
    public DbSet<WatchlistResultModel> WatchlistResults => Set<WatchlistResultModel>();

    /// <summary>
    /// Gets the notifications table.
    /// </summary>
    public DbSet<NotificationModel> Notifications => Set<NotificationModel>();

    /// <summary>
    /// On model creating.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
#pragma warning disable CA1062 // Validate arguments of public methods
        // Books
        modelBuilder.Entity<BookModel>()
            .HasOne(x => x.Author)
            .WithMany(x => x.Books)
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<BookModel>()
            .HasOne(x => x.Series)
            .WithMany(x => x.Books)
            .HasForeignKey(x => x.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        // Book Search View
        modelBuilder.Entity<BookSearchView>()
            .HasNoKey()
            .ToView("BookSearchView")

            .HasOne(x => x.BookModel)
            .WithMany()
            .HasForeignKey(x => x.Id)
            .HasPrincipalKey(x => x.Id);

        modelBuilder.Entity<BookSearchView>()
            .HasKey(x => x.Id);

        // Categories
        modelBuilder.Entity<BookCategoryModel>()
            .HasKey(x => new { x.BookId, x.CategoryId });

        modelBuilder.Entity<BookCategoryModel>()
            .HasOne(x => x.Book)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookCategoryModel>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Books)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CategoryModel>()
            .HasIndex(x => new { x.Name })
            .IsUnique();

        // Tags
        modelBuilder.Entity<BookTagModel>()
            .HasKey(x => new { x.BookId, x.TagId });

        modelBuilder.Entity<BookTagModel>()
            .HasOne(x => x.Book)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookTagModel>()
            .HasOne(x => x.Tag)
            .WithMany(x => x.Books)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TagModel>()
            .HasIndex(x => new { x.Name })
            .IsUnique();

        // Users
        modelBuilder.Entity<UserSettingModel>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSettingModel>()
            .HasIndex(x => new { x.UserId, x.Key })
            .IsUnique();

        modelBuilder.Entity<UserBookMetadataModel>()
            .HasOne(x => x.Book)
            .WithMany()
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserBookMetadataModel>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Author
        modelBuilder.Entity<AuthorModel>()
            .HasIndex(x => new { x.FirstName, x.LastName })
            .IsUnique();

        // Series
        modelBuilder.Entity<SeriesModel>()
            .HasIndex(x => new { x.Name })
            .IsUnique();

        // Visited Books
        modelBuilder.Entity<VisitedBooksModel>()
            .HasOne(x => x.Book)
            .WithMany()
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cloud Configuration
        modelBuilder.Entity<CloudConfigurationModel>()
            .HasIndex(x => new { x.Type })
            .IsUnique();

        // Cloud Storages
        modelBuilder.Entity<CloudStorageModel>()
            .HasIndex(x => new { x.Type, x.CloudOwnerEmail })
            .IsUnique();

        // Failed Cloud File Imports
        modelBuilder.Entity<FailedCloudFileImportModel>()
            .HasOne(x => x.Storage)
            .WithMany()
            .HasForeignKey(x => x.StorageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FailedCloudFileImportModel>()
            .HasOne(x => x.FileInfo)
            .WithMany()
            .HasForeignKey(x => x.FileInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Settings
        modelBuilder.Entity<SettingsModel>()
            .HasIndex(x => new { x.Key })
            .IsUnique();

        // Series Watchlist
        modelBuilder.Entity<WatchlistModel>()
            .HasKey(x => new { x.SeriesId, x.UserId });

        modelBuilder.Entity<WatchlistModel>()
            .HasOne(x => x.Series)
            .WithMany()
            .HasForeignKey(x => x.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WatchlistModel>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WatchlistResultModel>()
            .HasKey(x => new { x.SeriesId, x.Volume });

        modelBuilder.Entity<WatchlistResultModel>()
            .HasOne(x => x.Series)
            .WithMany()
            .HasForeignKey(x => x.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        // Notifications
        modelBuilder.Entity<NotificationModel>()
            .HasKey(x => new { x.Id });

        modelBuilder.Entity<NotificationModel>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NotificationModel>()
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

#pragma warning restore CA1062 // Validate arguments of public methods

        base.OnModelCreating(modelBuilder);

        // register extension methods
        modelBuilder.HasDbFunction(() => PgTrgmExtensions.Similarity(default!, default!));

        // background tasks migration for Quartz.NET
        modelBuilder.AddQuartz(builder => builder.UsePostgreSql());
    }
}
