using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Data;

public class KapitelShelfDBContext : DbContext
{
    public DbSet<BookModel> Books => Set<BookModel>();
    public DbSet<FileInfoModel> BookFileInfos => Set<FileInfoModel>();
    public DbSet<SeriesModel> Series => Set<SeriesModel>();
    public DbSet<AuthorModel> Authors => Set<AuthorModel>();
    public DbSet<LocationModel> Locations => Set<LocationModel>();
    public DbSet<TagModel> Tags => Set<TagModel>();
    public DbSet<BookTagModel> BookTags => Set<BookTagModel>();
    public DbSet<CategoryModel> Categories => Set<CategoryModel>();
    public DbSet<BookCategoryModel> BookCategories => Set<BookCategoryModel>();
    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<UserBookMetadata> UserBookMetadata => Set<UserBookMetadata>();

    public KapitelShelfDBContext(DbContextOptions<KapitelShelfDBContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        // Users
        modelBuilder.Entity<UserBookMetadata>()
            .HasOne(ub => ub.Book)
            .WithMany()
            .HasForeignKey(ub => ub.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserBookMetadata>()
            .HasOne(ub => ub.User)
            .WithMany()
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
