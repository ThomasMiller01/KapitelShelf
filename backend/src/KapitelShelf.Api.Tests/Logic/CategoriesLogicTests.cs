// <copyright file="CategoriesLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the CategoriesLogic class.
/// </summary>
[TestFixture]
public class CategoriesLogicTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private Mapper mapper;
    private CategoriesLogic testee;

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
        this.testee = new CategoriesLogic(this.dbContextFactory, this.mapper);
    }

    /// <summary>
    /// Tests GetCategoriesAsync returns paged results.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetCategoriesAsync_ReturnsPagedResult()
    {
        // setup
        var suffix = Guid.NewGuid().ToString("N");

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            for (var i = 1; i <= 15; i++)
            {
                context.Categories.Add(new CategoryModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"Category_{i:000}_{suffix}",
                    Books = [],
                });
            }

            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.GetCategoriesAsync(
            page: 1,
            pageSize: 10,
            sortBy: CategorySortByDTO.Default,
            sortDir: SortDirectionDTO.Asc,
            filter: null);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.GreaterThanOrEqualTo(15));
            Assert.That(result.Items, Has.Count.EqualTo(10));
        });
    }

    /// <summary>
    /// Tests GetCategoriesAsync applies filter and returns only matching results.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetCategoriesAsync_AppliesFilter()
    {
        // setup
        var token = $"Match_{Guid.NewGuid():N}";

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.Add(new CategoryModel
            {
                Id = Guid.NewGuid(),
                Name = token,
                Books = [],
            });

            context.Categories.Add(new CategoryModel
            {
                Id = Guid.NewGuid(),
                Name = $"Other_{Guid.NewGuid():N}",
                Books = [],
            });

            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.GetCategoriesAsync(
            page: 1,
            pageSize: 10,
            sortBy: CategorySortByDTO.Default,
            sortDir: SortDirectionDTO.Asc,
            filter: token);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Name, Is.EqualTo(token));
        });
    }

    /// <summary>
    /// Tests DeleteCategoriesAsync deletes only the specified categories (and keeps other categories in db).
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteCategoriesAsync_DeletesOnlySpecifiedCategories()
    {
        // setup
        var deleteId1 = Guid.NewGuid();
        var deleteId2 = Guid.NewGuid();
        var keepId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.AddRange(
                new CategoryModel
                {
                    Id = deleteId1,
                    Name = "Del".Unique(),
                    Books = [],
                },
                new CategoryModel
                {
                    Id = deleteId2,
                    Name = "Del".Unique(),
                    Books = [],
                },
                new CategoryModel
                {
                    Id = keepId,
                    Name = "Keep".Unique(),
                    Books = [],
                });

            await context.SaveChangesAsync();
        }

        // execute
        await this.testee.DeleteCategoriesAsync([deleteId1, deleteId2]);

        // assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.Multiple(() =>
            {
                Assert.That(context.Categories.FirstOrDefault(x => x.Id == deleteId1), Is.Null);
                Assert.That(context.Categories.FirstOrDefault(x => x.Id == deleteId2), Is.Null);
                Assert.That(context.Categories.FirstOrDefault(x => x.Id == keepId), Is.Not.Null);
            });
        }
    }

    /// <summary>
    /// Tests DeleteCategoriesAsync is safe when some ids do not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteCategoriesAsync_IgnoresUnknownIds()
    {
        // setup
        var existingId = Guid.NewGuid();
        var unknownId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.Add(new CategoryModel
            {
                Id = existingId,
                Name = "Existing".Unique(),
                Books = [],
            });
            await context.SaveChangesAsync();
        }

        // execute
        await this.testee.DeleteCategoriesAsync([unknownId]);

        // assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Categories.FirstOrDefault(x => x.Id == existingId), Is.Not.Null);
        }
    }

    /// <summary>
    /// Tests AutocompleteAsync returns empty when input is null/whitespace.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A task.</returns>
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task AutocompleteAsync_ReturnsEmpty_WhenInputIsNullOrWhitespace(string? input)
    {
        // execute
        var result = await this.testee.AutocompleteAsync(input);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Tests AutocompleteAsync returns matching categories and respects the 5 item limit.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AutocompleteAsync_ReturnsMatches_AndLimitsTo5()
    {
        // setup
        var prefix = "AutoCategory".Unique();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            // 12 matching
            for (int i = 0; i < 12; i++)
            {
                context.Categories.Add(new CategoryModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"{prefix} {i}".Unique(),
                    Books = [],
                });
            }

            // non-matching
            for (int i = 0; i < 3; i++)
            {
                context.Categories.Add(new CategoryModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"OtherCategory {i}".Unique(),
                    Books = [],
                });
            }

            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.AutocompleteAsync(prefix);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(5));
        Assert.That(result.All(x => x.Contains(prefix, StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    /// <summary>
    /// Tests AutocompleteAsync returns empty when nothing matches.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AutocompleteAsync_ReturnsEmpty_WhenNoMatches()
    {
        // setup
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.Add(new CategoryModel
            {
                Id = Guid.NewGuid(),
                Name = "NoMatchCategory".Unique(),
                Books = [],
            });
            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.AutocompleteAsync("definitely-does-not-match".Unique());

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Tests UpdateCategoryAsync throws if duplicate exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateCategoryAsync_ThrowsOnDuplicate()
    {
        // setup
        var existing = new CategoryModel
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate".Unique(),
            Books = [],
        };

        var toUpdateId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.Add(existing);
            context.Categories.Add(new CategoryModel
            {
                Id = toUpdateId,
                Name = "Other".Unique(),
                Books = [],
            });

            await context.SaveChangesAsync();
        }

        var dto = new CategoryDTO
        {
            Id = toUpdateId,
            Name = existing.Name,
        };

        // execute
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            _ = await this.testee.UpdateCategoryAsync(toUpdateId, dto);
        });

        // assert
        Assert.That(ex!.Message, Is.EqualTo(StaticConstants.DuplicateExceptionKey));
    }

    /// <summary>
    /// Tests UpdateCategoryAsync returns null when categoryDto is null.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateCategoryAsync_ReturnsNull_WhenDtoIsNull()
    {
        // execute
        var result = await this.testee.UpdateCategoryAsync(Guid.NewGuid(), null!);

        // assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests UpdateCategoryAsync returns null when category is not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateCategoryAsync_ReturnsNull_WhenNotFound()
    {
        // setup
        var dto = new CategoryDTO
        {
            Id = Guid.NewGuid(),
            Name = "Non".Unique(),
        };

        // execute
        var result = await this.testee.UpdateCategoryAsync(dto.Id, dto);

        // assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests UpdateCategoryAsync updates the entity and returns DTO.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateCategoryAsync_UpdatesAndReturnsDTO()
    {
        // setup
        var id = Guid.NewGuid();
        var category = new CategoryModel
        {
            Id = id,
            Name = "Original".Unique(),
            Books = [],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }

        var updatedDto = new CategoryDTO
        {
            Id = id,
            Name = "Updated".Unique(),
        };

        // execute
        var result = await this.testee.UpdateCategoryAsync(id, updatedDto);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Name, Is.EqualTo(updatedDto.Name));

            using var context = new KapitelShelfDBContext(this.dbOptions);
            var dbCategory = context.Categories.Single(x => x.Id == id);
            Assert.That(dbCategory.Name, Is.EqualTo(updatedDto.Name));
        });
    }

    /// <summary>
    /// Tests MergeCategories moves all book-category relations from source categories to target and deletes source categories.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task MergeCategories_MovesRelationsAndDeletesSourceCategories()
    {
        // setup
        var seriesId = Guid.NewGuid();

        var targetCategoryId = Guid.NewGuid();
        var sourceCategoryId1 = Guid.NewGuid();
        var sourceCategoryId2 = Guid.NewGuid();

        var series = new SeriesModel
        {
            Id = seriesId,
            Name = "Series".Unique(),
        };

        var book1 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book1".Unique(),
            Description = "Description",
            SeriesId = seriesId,
        };
        var book2 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book2".Unique(),
            Description = "Description",
            SeriesId = seriesId,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);

            // categories
            context.Categories.AddRange(
                new CategoryModel
                {
                    Id = targetCategoryId,
                    Name = "Target".Unique(),
                    Books = [],
                },
                new CategoryModel
                {
                    Id = sourceCategoryId1,
                    Name = "Source".Unique(),
                    Books = [],
                },
                new CategoryModel
                {
                    Id = sourceCategoryId2,
                    Name = "Source".Unique(),
                    Books = [],
                });

            // books
            context.Books.AddRange(book1, book2);

            // relations (book-category join)
            context.BookCategories.AddRange(
                new BookCategoryModel
                {
                    BookId = book1.Id,
                    CategoryId = sourceCategoryId1,
                },
                new BookCategoryModel
                {
                    BookId = book2.Id,
                    CategoryId = sourceCategoryId2,
                });

            await context.SaveChangesAsync();
        }

        // execute
        await this.testee.MergeCategories(targetCategoryId, [sourceCategoryId1, sourceCategoryId2]);

        // assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var b1TargetRelation = await context.BookCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BookId == book1.Id && x.CategoryId == targetCategoryId);

            var b2TargetRelation = await context.BookCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BookId == book2.Id && x.CategoryId == targetCategoryId);

            Assert.Multiple(() =>
            {
                Assert.That(b1TargetRelation, Is.Not.Null);
                Assert.That(b2TargetRelation, Is.Not.Null);
            });

            Assert.Multiple(async () =>
            {
                Assert.That(await context.Categories.FindAsync(sourceCategoryId1), Is.Null);
                Assert.That(await context.Categories.FindAsync(sourceCategoryId2), Is.Null);
                Assert.That(await context.Categories.FindAsync(targetCategoryId), Is.Not.Null);
            });
        }
    }

    /// <summary>
    /// Tests MergeCategories throws if target category does not exist.
    /// </summary>
    [Test]
    public void MergeCategories_ThrowsOnUnknownTarget()
    {
        // setup
        var seriesId = Guid.NewGuid();

        var sourceCategoryId = Guid.NewGuid();
        var unknownTarget = Guid.NewGuid();

        var series = new SeriesModel
        {
            Id = seriesId,
            Name = "Series".Unique(),
        };

        var book = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book".Unique(),
            Description = "Description",
            SeriesId = seriesId,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);

            context.Categories.Add(new CategoryModel
            {
                Id = sourceCategoryId,
                Name = "Source".Unique(),
                Books = [],
            });

            context.Books.Add(book);

            context.BookCategories.Add(new BookCategoryModel
            {
                BookId = book.Id,
                CategoryId = sourceCategoryId,
            });

            context.SaveChanges();
        }

        // execute, assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await this.testee.MergeCategories(unknownTarget, [sourceCategoryId]);
        });

        Assert.That(ex!.Message, Does.Contain("Unknown target category id"));
    }

    /// <summary>
    /// Tests MergeCategories throws if no source category relations exist for the provided source ids.
    /// </summary>
    [Test]
    public void MergeCategories_ThrowsOnUnknownSources()
    {
        // setup
        var targetCategoryId = Guid.NewGuid();
        var unknownSource = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.Add(new CategoryModel
            {
                Id = targetCategoryId,
                Name = "Target".Unique(),
                Books = [],
            });

            context.SaveChanges();
        }

        // execute, assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await this.testee.MergeCategories(targetCategoryId, [unknownSource]);
        });

        Assert.That(ex!.Message, Does.Contain("Unknown source category ids"));
    }

    /// <summary>
    /// Tests GetDuplicatesAsync returns matching categories by name.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetDuplicatesAsync_ReturnsMatchingCategories()
    {
        // setup
        var name = "Name".Unique();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Categories.Add(new CategoryModel
            {
                Id = Guid.NewGuid(),
                Name = name,
                Books = [],
            });

            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.GetDuplicatesAsync(name);

        // assert
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(result.Any(x => x.Name == name), Is.True);
    }

    /// <summary>
    /// Tests GetDuplicatesAsync returns empty if no match.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetDuplicatesAsync_ReturnsEmpty_IfNoMatch()
    {
        // execute
        var result = await this.testee.GetDuplicatesAsync("NoSuchCategory".Unique());

        // assert
        Assert.That(result, Is.Empty);
    }
}
