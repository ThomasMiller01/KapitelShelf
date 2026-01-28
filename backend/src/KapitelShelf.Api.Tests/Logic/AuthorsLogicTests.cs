// <copyright file="AuthorsLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Author;
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
/// Unit tests for the AuthorsLogic class.
/// </summary>
[TestFixture]
public class AuthorsLogicTests
{
    private PostgreSqlContainer postgres;

    private DbContextOptions<KapitelShelfDBContext> dbOptions;
    private IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private Mapper mapper;
    private AuthorsLogic testee;

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
        this.testee = new AuthorsLogic(this.dbContextFactory, this.mapper);
    }

    /// <summary>
    /// Tests GetAuthorsAsync returns paged results.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetAuthorsAsync_ReturnsPagedResult()
    {
        // setup
        var suffix = Guid.NewGuid().ToString("N");

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            for (var i = 1; i <= 15; i++)
            {
                context.Authors.Add(new AuthorModel
                {
                    Id = Guid.NewGuid(),
                    FirstName = $"First_{i:000}_{suffix}",
                    LastName = $"Last_{i:000}_{suffix}",
                    Books = [],
                });
            }

            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.GetAuthorsAsync(
            page: 1,
            pageSize: 10,
            sortBy: AuthorSortByDTO.Default,
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
    /// Tests GetAuthorsAsync applies filter and returns only matching results.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetAuthorsAsync_AppliesFilter()
    {
        // setup
        var token = $"Match_{Guid.NewGuid():N}";

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            // matching
            context.Authors.Add(new AuthorModel
            {
                Id = Guid.NewGuid(),
                FirstName = token,
                LastName = "Doe",
                Books = [],
            });

            // non-matching
            context.Authors.Add(new AuthorModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Other",
                LastName = $"Last_{Guid.NewGuid():N}",
                Books = [],
            });

            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.GetAuthorsAsync(
            page: 1,
            pageSize: 10,
            sortBy: AuthorSortByDTO.Default,
            sortDir: SortDirectionDTO.Asc,
            filter: token);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].FirstName, Is.EqualTo(token));
        });
    }

    /// <summary>
    /// Tests DeleteAuthorsAsync deletes only the specified authors (and keeps other authors in db).
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteAuthorsAsync_DeletesOnlySpecifiedAuthors()
    {
        // setup
        var deleteId1 = Guid.NewGuid();
        var deleteId2 = Guid.NewGuid();
        var keepId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Authors.AddRange(
                new AuthorModel
                {
                    Id = deleteId1,
                    FirstName = "Del".Unique(),
                    LastName = "One".Unique(),
                    Books = [],
                },
                new AuthorModel
                {
                    Id = deleteId2,
                    FirstName = "Del".Unique(),
                    LastName = "Two".Unique(),
                    Books = [],
                },
                new AuthorModel
                {
                    Id = keepId,
                    FirstName = "Keep".Unique(),
                    LastName = "Me".Unique(),
                    Books = [],
                });

            await context.SaveChangesAsync();
        }

        // execute
        await this.testee.DeleteAuthorsAsync([deleteId1, deleteId2]);

        // assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.Multiple(() =>
            {
                Assert.That(context.Authors.FirstOrDefault(x => x.Id == deleteId1), Is.Null);
                Assert.That(context.Authors.FirstOrDefault(x => x.Id == deleteId2), Is.Null);
                Assert.That(context.Authors.FirstOrDefault(x => x.Id == keepId), Is.Not.Null);
            });
        }
    }

    /// <summary>
    /// Tests DeleteAuthorsAsync is safe when some ids do not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DeleteAuthorsAsync_IgnoresUnknownIds()
    {
        // setup
        var existingId = Guid.NewGuid();
        var unknownId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Authors.Add(new AuthorModel
            {
                Id = existingId,
                FirstName = "Existing".Unique(),
                LastName = "Author".Unique(),
                Books = [],
            });
            await context.SaveChangesAsync();
        }

        // execute
        await this.testee.DeleteAuthorsAsync([unknownId]);

        // assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            Assert.That(context.Authors.FirstOrDefault(x => x.Id == existingId), Is.Not.Null);
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
    /// Tests AutocompleteAsync returns matching authors and respects the 5 item limit.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task AutocompleteAsync_ReturnsMatches_AndLimitsTo5()
    {
        // setup
        var prefix = "AutoAuthor".Unique();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            // 12 matching
            for (int i = 0; i < 12; i++)
            {
                context.Authors.Add(new AuthorModel
                {
                    Id = Guid.NewGuid(),
                    FirstName = prefix,
                    LastName = $"Last{i}".Unique(),
                    Books = [],
                });
            }

            // non-matching
            for (int i = 0; i < 3; i++)
            {
                context.Authors.Add(new AuthorModel
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Other".Unique(),
                    LastName = "Person".Unique(),
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
        Assert.Multiple(() =>
        {
            Assert.That(result.All(x => x.Contains(prefix, StringComparison.OrdinalIgnoreCase)), Is.True);
            Assert.That(result.All(x => x.Contains(' ')), Is.True);
        });
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
            context.Authors.Add(new AuthorModel
            {
                Id = Guid.NewGuid(),
                FirstName = "NoMatch".Unique(),
                LastName = "Author".Unique(),
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
    /// Tests UpdateAuthorAsync throws if duplicate exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateAuthorAsync_ThrowsOnDuplicate()
    {
        // setup
        var existing = new AuthorModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane".Unique(),
            LastName = "Doe".Unique(),
            Books = [],
        };

        var toUpdateId = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Authors.Add(existing);
            context.Authors.Add(new AuthorModel
            {
                Id = toUpdateId,
                FirstName = "X".Unique(),
                LastName = "Y".Unique(),
                Books = [],
            });
            await context.SaveChangesAsync();
        }

        var dto = new AuthorDTO
        {
            Id = toUpdateId,
            FirstName = existing.FirstName,
            LastName = existing.LastName,
        };

        // execute
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            _ = await this.testee.UpdateAuthorAsync(toUpdateId, dto);
        });

        // assert
        Assert.That(ex!.Message, Is.EqualTo(StaticConstants.DuplicateExceptionKey));
    }

    /// <summary>
    /// Tests UpdateAuthorAsync returns null when authorDto is null.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateAuthorAsync_ReturnsNull_WhenDtoIsNull()
    {
        // execute
        var result = await this.testee.UpdateAuthorAsync(Guid.NewGuid(), null!);

        // assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests UpdateAuthorAsync returns null when author is not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateAuthorAsync_ReturnsNull_WhenNotFound()
    {
        // setup
        var dto = new AuthorDTO
        {
            Id = Guid.NewGuid(),
            FirstName = "Non".Unique(),
            LastName = "Existent".Unique(),
        };

        // execute
        var result = await this.testee.UpdateAuthorAsync(dto.Id, dto);

        // assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests UpdateAuthorAsync updates the entity and returns DTO.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task UpdateAuthorAsync_UpdatesAndReturnsDTO()
    {
        // setup
        var id = Guid.NewGuid();
        var author = new AuthorModel
        {
            Id = id,
            FirstName = "Original".Unique(),
            LastName = "Name".Unique(),
            Books = [],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Authors.Add(author);
            await context.SaveChangesAsync();
        }

        var updatedDto = new AuthorDTO
        {
            Id = id,
            FirstName = "Updated".Unique(),
            LastName = "Author".Unique(),
        };

        // execute
        var result = await this.testee.UpdateAuthorAsync(id, updatedDto);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.FirstName, Is.EqualTo(updatedDto.FirstName));
            Assert.That(result.LastName, Is.EqualTo(updatedDto.LastName));

            using var context = new KapitelShelfDBContext(this.dbOptions);
            var dbAuthor = context.Authors.Single(x => x.Id == id);
            Assert.That(dbAuthor.FirstName, Is.EqualTo(updatedDto.FirstName));
            Assert.That(dbAuthor.LastName, Is.EqualTo(updatedDto.LastName));
        });
    }

    /// <summary>
    /// Tests MergeAuthors moves all books from source to target and deletes source authors.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task MergeAuthors_MovesBooksAndDeletesSourceAuthors()
    {
        // setup
        var seriesId = Guid.NewGuid();

        var targetId = Guid.NewGuid();
        var sourceId1 = Guid.NewGuid();
        var sourceId2 = Guid.NewGuid();

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
            AuthorId = sourceId1,
            SeriesId = seriesId,
        };
        var book2 = new BookModel
        {
            Id = Guid.NewGuid(),
            Title = "Book2".Unique(),
            Description = "Description",
            AuthorId = sourceId2,
            SeriesId = seriesId,
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);

            // authors
            context.Authors.AddRange(
                new AuthorModel
                {
                    Id = targetId,
                    FirstName = "Target".Unique(),
                    LastName = "Author".Unique(),
                    Books = [],
                },
                new AuthorModel
                {
                    Id = sourceId1,
                    FirstName = "Source".Unique(),
                    LastName = "One".Unique(),
                    Books = [],
                },
                new AuthorModel
                {
                    Id = sourceId2,
                    FirstName = "Source".Unique(),
                    LastName = "Two".Unique(),
                    Books = [],
                });

            // books
            context.Books.AddRange(book1, book2);

            await context.SaveChangesAsync();
        }

        // execute
        await this.testee.MergeAuthors(targetId, [sourceId1, sourceId2]);

        // assert
        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            var b1 = await context.Books.FindAsync(book1.Id);
            var b2 = await context.Books.FindAsync(book2.Id);

            Assert.Multiple(() =>
            {
                Assert.That(b1, Is.Not.Null);
                Assert.That(b2, Is.Not.Null);
                Assert.That(b1!.AuthorId, Is.EqualTo(targetId));
                Assert.That(b2!.AuthorId, Is.EqualTo(targetId));
            });

            Assert.Multiple(async () =>
            {
                Assert.That(await context.Authors.FindAsync(sourceId1), Is.Null);
                Assert.That(await context.Authors.FindAsync(sourceId2), Is.Null);
                Assert.That(await context.Authors.FindAsync(targetId), Is.Not.Null);
            });
        }
    }

    /// <summary>
    /// Tests MergeAuthors throws if target author does not exist.
    /// </summary>
    [Test]
    public void MergeAuthors_ThrowsOnUnknownTarget()
    {
        // setup
        var seriesId = Guid.NewGuid();

        var sourceId = Guid.NewGuid();
        var unknownTarget = Guid.NewGuid();

        var series = new SeriesModel
        {
            Id = seriesId,
            Name = "Series".Unique(),
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Series.Add(series);

            context.Authors.Add(new AuthorModel
            {
                Id = sourceId,
                FirstName = "Source".Unique(),
                LastName = "Author".Unique(),
                Books = [],
            });

            context.Books.Add(new BookModel
            {
                Id = Guid.NewGuid(),
                Title = "Book".Unique(),
                Description = "Description",
                AuthorId = sourceId,
                SeriesId = seriesId,
            });

            context.SaveChanges();
        }

        // execute, assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await this.testee.MergeAuthors(unknownTarget, [sourceId]);
        });

        Assert.That(ex!.Message, Does.Contain("Unknown target author id"));
    }

    /// <summary>
    /// Tests MergeAuthors throws if no source authors exist.
    /// </summary>
    [Test]
    public void MergeAuthors_ThrowsOnUnknownSources()
    {
        // setup
        var targetId = Guid.NewGuid();
        var unknownSource = Guid.NewGuid();

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Authors.Add(new AuthorModel
            {
                Id = targetId,
                FirstName = "Target".Unique(),
                LastName = "Author".Unique(),
                Books = [],
            });

            context.SaveChanges();
        }

        // execute, assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await this.testee.MergeAuthors(targetId, [unknownSource]);
        });

        Assert.That(ex!.Message, Does.Contain("Unknown source author ids"));
    }

    /// <summary>
    /// Tests GetDuplicatesAsync returns matching authors by first and last name.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetDuplicatesAsync_ReturnsMatchingAuthors()
    {
        // setup
        var first = "First".Unique();
        var last = "Last".Unique();

        var match = new AuthorModel
        {
            Id = Guid.NewGuid(),
            FirstName = first,
            LastName = last,
            Books = [],
        };

        using (var context = new KapitelShelfDBContext(this.dbOptions))
        {
            context.Authors.Add(match);
            await context.SaveChangesAsync();
        }

        // execute
        var result = await this.testee.GetDuplicatesAsync(first, last);

        // assert
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(result.Any(x => x.FirstName == first && x.LastName == last), Is.True);
    }

    /// <summary>
    /// Tests GetDuplicatesAsync returns empty if no match.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetDuplicatesAsync_ReturnsEmpty_IfNoMatch()
    {
        // execute
        var result = await this.testee.GetDuplicatesAsync("No".Unique(), "Match".Unique());

        // assert
        Assert.That(result, Is.Empty);
    }
}
