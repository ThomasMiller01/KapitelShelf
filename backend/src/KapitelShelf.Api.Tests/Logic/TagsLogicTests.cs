// <copyright file="TagsLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Tag;
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
/// Unit tests for the TagsLogic class.
/// </summary>
[TestFixture]
public class TagsLogicTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private Mapper mapper;
    private TagsLogic testee;

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
        this.testee = new TagsLogic(this.dbContextFactory, this.mapper);
    }

    /// <summary>
    /// Tests GetTagsAsync returns paged results.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetTagsAsync_ReturnsPagedResult()
    {
        // setup
        var suffix = Guid.NewGuid().ToString("N");

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            for (var i = 1; i <= 15; i++)
            {
                context.Tags.Add(new TagModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"Tag_{i:000}_{suffix}",
                    Books = [],
                });
            }

            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.GetTagsAsync(
            page: 1,
            pageSize: 10,
            sortBy: TagSortByDTO.Default,
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
    /// Tests GetTagsAsync applies filter and returns only matching results.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetTagsAsync_AppliesFilter()
    {
        // setup
        var token = $"Match_{Guid.NewGuid():N}";

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Tags.Add(new TagModel
            {
                Id = Guid.NewGuid(),
                Name = token,
                Books = [],
            });

            context.Tags.Add(new TagModel
            {
                Id = Guid.NewGuid(),
                Name = $"Other_{Guid.NewGuid():N}",
                Books = [],
            });

            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.GetTagsAsync(
            page: 1,
            pageSize: 10,
            sortBy: TagSortByDTO.Default,
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
    /// Tests DeleteTagsAsync deletes only the specified tags (and keeps other tags in db).
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteTagsAsync_DeletesOnlySpecifiedTags()
    {
        // setup
        var deleteId1 = Guid.NewGuid();
        var deleteId2 = Guid.NewGuid();
        var keepId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Tags.AddRange(
                new TagModel
                {
                    Id = deleteId1,
                    Name = "Del".Unique(),
                    Books = [],
                },
                new TagModel
                {
                    Id = deleteId2,
                    Name = "Del".Unique(),
                    Books = [],
                },
                new TagModel
                {
                    Id = keepId,
                    Name = "Keep".Unique(),
                    Books = [],
                });

            await context.SaveChangesAsync();
        }

        // execute
        await this.testee.DeleteTagsAsync([deleteId1, deleteId2]);

        // assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.Multiple(() =>
            {
                Assert.That(context.Tags.FirstOrDefault(x => x.Id == deleteId1), Is.Null);
                Assert.That(context.Tags.FirstOrDefault(x => x.Id == deleteId2), Is.Null);
                Assert.That(context.Tags.FirstOrDefault(x => x.Id == keepId), Is.Not.Null);
            });
        }
    }

    /// <summary>
    /// Tests DeleteTagsAsync is safe when some ids do not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteTagsAsync_IgnoresUnknownIds()
    {
        // setup
        var existingId = Guid.NewGuid();
        var unknownId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Tags.Add(new TagModel
            {
                Id = existingId,
                Name = "Existing".Unique(),
                Books = [],
            });
            await context.SaveChangesAsync();
        }

        // execute
        await this.testee.DeleteTagsAsync([unknownId]);

        // assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Tags.FirstOrDefault(x => x.Id == existingId), Is.Not.Null);
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
    /// Tests AutocompleteAsync returns matching tags and respects the 5 item limit.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AutocompleteAsync_ReturnsMatches_AndLimitsTo5()
    {
        // setup
        var prefix = "AutoTag".Unique();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            // 12 matching
            for (int i = 0; i < 12; i++)
            {
                context.Tags.Add(new TagModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"{prefix} {i}".Unique(),
                    Books = [],
                });
            }

            // non-matching
            for (int i = 0; i < 3; i++)
            {
                context.Tags.Add(new TagModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"OtherTag {i}".Unique(),
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
            context.Tags.Add(new TagModel
            {
                Id = Guid.NewGuid(),
                Name = "NoMatchTag".Unique(),
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
    /// Tests UpdateTagAsync throws if duplicate exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateTagAsync_ThrowsOnDuplicate()
    {
        // setup
        var existing = new TagModel
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate".Unique(),
            Books = [],
        };

        var toUpdateId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Tags.Add(existing);
            context.Tags.Add(new TagModel
            {
                Id = toUpdateId,
                Name = "Other".Unique(),
                Books = [],
            });

            await context.SaveChangesAsync();
        }

        var dto = new TagDTO
        {
            Id = toUpdateId,
            Name = existing.Name,
        };

        // execute
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            _ = await this.testee.UpdateTagAsync(toUpdateId, dto);
        });

        // assert
        Assert.That(ex!.Message, Is.EqualTo(StaticConstants.DuplicateExceptionKey));
    }

    /// <summary>
    /// Tests UpdateTagAsync returns null when tagDto is null.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateTagAsync_ReturnsNull_WhenDtoIsNull()
    {
        // execute
        var result = await this.testee.UpdateTagAsync(Guid.NewGuid(), null!);

        // assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests UpdateTagAsync returns null when tag is not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateTagAsync_ReturnsNull_WhenNotFound()
    {
        // setup
        var dto = new TagDTO
        {
            Id = Guid.NewGuid(),
            Name = "Non".Unique(),
        };

        // execute
        var result = await this.testee.UpdateTagAsync(dto.Id, dto);

        // assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests UpdateTagAsync updates the entity and returns DTO.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateTagAsync_UpdatesAndReturnsDTO()
    {
        // setup
        var id = Guid.NewGuid();
        var tag = new TagModel
        {
            Id = id,
            Name = "Original".Unique(),
            Books = [],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Tags.Add(tag);
            await context.SaveChangesAsync();
        }

        var updatedDto = new TagDTO
        {
            Id = id,
            Name = "Updated".Unique(),
        };

        // execute
        var result = await this.testee.UpdateTagAsync(id, updatedDto);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Name, Is.EqualTo(updatedDto.Name));

            using var context = new KapitelShelfDBContext(this.dbOptions);
            var dbTag = context.Tags.Single(x => x.Id == id);
            Assert.That(dbTag.Name, Is.EqualTo(updatedDto.Name));
        });
    }

    /// <summary>
    /// Tests MergeTags moves all book-tag relations from source tags to target and deletes source tags.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task MergeTags_MovesRelationsAndDeletesSourceTags()
    {
        // setup
        var seriesId = Guid.NewGuid();

        var targetTagId = Guid.NewGuid();
        var sourceTagId1 = Guid.NewGuid();
        var sourceTagId2 = Guid.NewGuid();

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

            // tags
            context.Tags.AddRange(
                new TagModel
                {
                    Id = targetTagId,
                    Name = "Target".Unique(),
                    Books = [],
                },
                new TagModel
                {
                    Id = sourceTagId1,
                    Name = "Source".Unique(),
                    Books = [],
                },
                new TagModel
                {
                    Id = sourceTagId2,
                    Name = "Source".Unique(),
                    Books = [],
                });

            // books
            context.Books.AddRange(book1, book2);

            // relations
            context.BookTags.AddRange(
                new BookTagModel
                {
                    BookId = book1.Id,
                    TagId = sourceTagId1,
                },
                new BookTagModel
                {
                    BookId = book2.Id,
                    TagId = sourceTagId2,
                });

            await context.SaveChangesAsync();
        }

        // execute
        await this.testee.MergeTags(targetTagId, [sourceTagId1, sourceTagId2]);

        // assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var b1TargetRelation = await context.BookTags
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BookId == book1.Id && x.TagId == targetTagId);

            var b2TargetRelation = await context.BookTags
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BookId == book2.Id && x.TagId == targetTagId);

            Assert.Multiple(() =>
            {
                Assert.That(b1TargetRelation, Is.Not.Null);
                Assert.That(b2TargetRelation, Is.Not.Null);
            });

            Assert.Multiple(async () =>
            {
                Assert.That(await context.Tags.FindAsync(sourceTagId1), Is.Null);
                Assert.That(await context.Tags.FindAsync(sourceTagId2), Is.Null);
                Assert.That(await context.Tags.FindAsync(targetTagId), Is.Not.Null);
            });
        }
    }

    /// <summary>
    /// Tests MergeTags throws if target tag does not exist.
    /// </summary>
    [Test]
    public void MergeTags_ThrowsOnUnknownTarget()
    {
        // setup
        var seriesId = Guid.NewGuid();

        var sourceTagId = Guid.NewGuid();
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

            context.Tags.Add(new TagModel
            {
                Id = sourceTagId,
                Name = "Source".Unique(),
                Books = [],
            });

            context.Books.Add(book);

            context.BookTags.Add(new BookTagModel
            {
                BookId = book.Id,
                TagId = sourceTagId,
            });

            context.SaveChanges();
        }

        // execute, assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await this.testee.MergeTags(unknownTarget, [sourceTagId]);
        });

        Assert.That(ex!.Message, Does.Contain("Unknown target tag id"));
    }

    /// <summary>
    /// Tests MergeTags throws if no source tag relations exist for the provided source ids.
    /// </summary>
    [Test]
    public void MergeTags_ThrowsOnUnknownSources()
    {
        // setup
        var targetTagId = Guid.NewGuid();
        var unknownSource = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Tags.Add(new TagModel
            {
                Id = targetTagId,
                Name = "Target".Unique(),
                Books = [],
            });

            context.SaveChanges();
        }

        // execute, assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await this.testee.MergeTags(targetTagId, [unknownSource]);
        });

        Assert.That(ex!.Message, Does.Contain("Unknown source tag ids"));
    }

    /// <summary>
    /// Tests GetDuplicatesAsync returns matching tags by name.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetDuplicatesAsync_ReturnsMatchingTags()
    {
        // setup
        var name = "Name".Unique();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Tags.Add(new TagModel
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
        var result = await this.testee.GetDuplicatesAsync("NoSuchTag".Unique());

        // assert
        Assert.That(result, Is.Empty);
    }
}
