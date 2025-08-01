﻿// <auto-generated />
using System;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace KapitelShelf.Data.Migrations.Migrations
{
    [DbContext(typeof(KapitelShelfDBContext))]
    [Migration("20250702181910_AddVisitedBookTime")]
    partial class AddVisitedBookTime
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("KapitelShelf.Data.Models.AuthorModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FirstName", "LastName")
                        .IsUnique();

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.BookCategoryModel", b =>
                {
                    b.Property<Guid>("BookId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uuid");

                    b.HasKey("BookId", "CategoryId");

                    b.HasIndex("CategoryId");

                    b.ToTable("BookCategories");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.BookModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AuthorId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CoverId")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("LocationId")
                        .HasColumnType("uuid");

                    b.Property<int?>("PageNumber")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("SeriesId")
                        .HasColumnType("uuid");

                    b.Property<int>("SeriesNumber")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("CoverId");

                    b.HasIndex("LocationId");

                    b.HasIndex("SeriesId");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.BookSearchView", b =>
                {
                    b.Property<string>("AuthorNames")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CategoryNames")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("SearchText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<NpgsqlTsVector>("SearchVector")
                        .IsRequired()
                        .HasColumnType("tsvector");

                    b.Property<string>("SeriesName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TagNames")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasIndex("Id");

                    b.ToTable((string)null);

                    b.ToView("BookSearchView", (string)null);
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.BookTagModel", b =>
                {
                    b.Property<Guid>("BookId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TagId")
                        .HasColumnType("uuid");

                    b.HasKey("BookId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("BookTags");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.CategoryModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.FileInfoModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("FileSizeBytes")
                        .HasColumnType("bigint");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Sha256")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("FileInfos");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.LocationModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("FileInfoId")
                        .HasColumnType("uuid");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FileInfoId");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.SeriesModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Series");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.TagModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.UserBookMetadataModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("BookId")
                        .HasColumnType("uuid");

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<bool?>("Favourite")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastReadDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("Rating")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("UserModelId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.HasIndex("UserId");

                    b.HasIndex("UserModelId");

                    b.ToTable("UserBookMetadata");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.UserModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.VisitedBooksModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("BookId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("VisitedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.HasIndex("UserId");

                    b.ToTable("VisitedBooks");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.BookCategoryModel", b =>
                {
                    b.HasOne("KapitelShelf.Data.Models.BookModel", "Book")
                        .WithMany("Categories")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KapitelShelf.Data.Models.CategoryModel", "Category")
                        .WithMany("Books")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.BookModel", b =>
                {
                    b.HasOne("KapitelShelf.Data.Models.AuthorModel", "Author")
                        .WithMany("Books")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("KapitelShelf.Data.Models.FileInfoModel", "Cover")
                        .WithMany()
                        .HasForeignKey("CoverId");

                    b.HasOne("KapitelShelf.Data.Models.LocationModel", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId");

                    b.HasOne("KapitelShelf.Data.Models.SeriesModel", "Series")
                        .WithMany("Books")
                        .HasForeignKey("SeriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("Cover");

                    b.Navigation("Location");

                    b.Navigation("Series");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.BookSearchView", b =>
                {
                    b.HasOne("KapitelShelf.Data.Models.BookModel", "BookModel")
                        .WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BookModel");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.BookTagModel", b =>
                {
                    b.HasOne("KapitelShelf.Data.Models.BookModel", "Book")
                        .WithMany("Tags")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KapitelShelf.Data.Models.TagModel", "Tag")
                        .WithMany("Books")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.LocationModel", b =>
                {
                    b.HasOne("KapitelShelf.Data.Models.FileInfoModel", "FileInfo")
                        .WithMany()
                        .HasForeignKey("FileInfoId");

                    b.Navigation("FileInfo");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.UserBookMetadataModel", b =>
                {
                    b.HasOne("KapitelShelf.Data.Models.BookModel", "Book")
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KapitelShelf.Data.Models.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KapitelShelf.Data.Models.UserModel", null)
                        .WithMany("Books")
                        .HasForeignKey("UserModelId");

                    b.Navigation("Book");

                    b.Navigation("User");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.VisitedBooksModel", b =>
                {
                    b.HasOne("KapitelShelf.Data.Models.BookModel", "Book")
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KapitelShelf.Data.Models.UserModel", "User")
                        .WithMany("VisitedBooks")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");

                    b.Navigation("User");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.AuthorModel", b =>
                {
                    b.Navigation("Books");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.BookModel", b =>
                {
                    b.Navigation("Categories");

                    b.Navigation("Tags");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.CategoryModel", b =>
                {
                    b.Navigation("Books");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.SeriesModel", b =>
                {
                    b.Navigation("Books");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.TagModel", b =>
                {
                    b.Navigation("Books");
                });

            modelBuilder.Entity("KapitelShelf.Data.Models.UserModel", b =>
                {
                    b.Navigation("Books");

                    b.Navigation("VisitedBooks");
                });
#pragma warning restore 612, 618
        }
    }
}
