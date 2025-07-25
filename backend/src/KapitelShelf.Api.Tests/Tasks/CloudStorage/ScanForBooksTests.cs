// <copyright file="ScanForBooksTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Tasks;
using KapitelShelf.Api.Tasks.CloudStorage;
using KapitelShelf.Data.Models.CloudStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Quartz;

namespace KapitelShelf.Api.Tests.Tasks.CloudStorage;

/// <summary>
/// Unit tests for ScanForBooks.
/// </summary>
[TestFixture]
public class ScanForBooksTests
{
    private ScanForBooks testee;
    private ITaskRuntimeDataStore dataStore;
    private ILogger<TaskBase> logger;
    private ICloudStorage fileStorage;
    private ICloudStoragesLogic logic;
    private IMapper mapper;
    private IBooksLogic booksLogic;
    private IBookParserManager bookParserManager;
    private IJobExecutionContext context;
    private CloudStorageDTO storage;
    private CloudStorageModel storageModel;

    /// <summary>
    /// Sets up testee and dependencies.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.dataStore = Substitute.For<ITaskRuntimeDataStore>();
        this.logger = Substitute.For<ILogger<TaskBase>>();
        this.fileStorage = Substitute.For<ICloudStorage>();
        this.logic = Substitute.For<ICloudStoragesLogic>();
        this.mapper = Substitute.For<IMapper>();
        this.booksLogic = Substitute.For<IBooksLogic>();
        this.bookParserManager = Substitute.For<IBookParserManager>();
        this.context = Substitute.For<IJobExecutionContext>();

        var jobKey = new JobKey("ScanBooks", "Test");
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(jobKey);
        this.context.JobDetail.Returns(jobDetail);

        this.storage = new CloudStorageDTO
        {
            Id = Guid.NewGuid(),
            CloudDirectory = "TestDir",
            Type = CloudTypeDTO.OneDrive,
        };

        this.storageModel = new CloudStorageModel
        {
            Id = storage.Id,
            CloudDirectory = storage.CloudDirectory,
            Type = CloudType.OneDrive,
        };

        this.testee = new ScanForBooks(
            this.dataStore,
            this.logger,
            this.fileStorage,
            this.logic,
            this.mapper,
            this.booksLogic,
            this.bookParserManager);
    }

    /// <summary>
    /// Tests ExecuteTask returns when no storages are downloaded.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_Returns_IfNoDownloadedStorages()
    {
        // Setup
        this.logic.GetDownloadedStorageModels().Returns([]);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.booksLogic.DidNotReceive().ImportBookAsync(Arg.Any<IFormFile>());
    }

    /// <summary>
    /// Tests ExecuteTask imports all new books found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_ImportsAllNewBooks()
    {
        // Setup
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var bookFile = Path.Combine(tempDir, "book.epub");
        File.WriteAllText(bookFile, "test");

        this.logic.GetDownloadedStorageModels().Returns([this.storageModel]);
        this.mapper.Map<CloudStorageDTO>(this.storageModel).Returns(this.storage);
        this.fileStorage.FullPath(this.storage, Arg.Any<string>()).Returns(tempDir);
        this.bookParserManager.SupportedFileEndings().Returns(["epub"]);

        this.booksLogic.BookFileExists(Arg.Any<IFormFile>()).Returns(false);
        this.logic.CloudFileImportFailed(this.storage, Arg.Any<IFormFile>()).Returns(false);
        this.booksLogic.ImportBookAsync(Arg.Any<IFormFile>()).Returns(Task.FromResult(new ImportResultDTO()));

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.booksLogic.Received().ImportBookAsync(Arg.Any<IFormFile>());
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Tests ExecuteTask skips files already imported or failed.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_Skips_ImportedOrFailedFiles()
    {
        // Setup
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var bookFile = Path.Combine(tempDir, "book.epub");
        File.WriteAllText(bookFile, "test");

        this.logic.GetDownloadedStorageModels().Returns([this.storageModel]);
        this.mapper.Map<CloudStorageDTO>(this.storageModel).Returns(this.storage);
        this.fileStorage.FullPath(this.storage, Arg.Any<string>()).Returns(tempDir);
        this.bookParserManager.SupportedFileEndings().Returns(["epub"]);

        this.booksLogic.BookFileExists(Arg.Any<IFormFile>()).Returns(true);
        this.logic.CloudFileImportFailed(this.storage, Arg.Any<IFormFile>()).Returns(true);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.booksLogic.DidNotReceive().ImportBookAsync(Arg.Any<IFormFile>());
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Tests ExecuteTask logs error and adds import fail if ImportBookAsync throws.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_LogsErrorAndAddsImportFail_IfImportThrows()
    {
        // Setup
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var bookFile = Path.Combine(tempDir, "book.epub");
        File.WriteAllText(bookFile, "test content");

        this.logic.GetDownloadedStorageModels().Returns([this.storageModel]);
        this.mapper.Map<CloudStorageDTO>(this.storageModel).Returns(this.storage);
        this.fileStorage.FullPath(this.storage, Arg.Any<string>()).Returns(tempDir);
        this.bookParserManager.SupportedFileEndings().Returns(["epub"]);

        this.booksLogic.BookFileExists(Arg.Any<IFormFile>()).Returns(false);
        this.logic.CloudFileImportFailed(this.storage, Arg.Any<IFormFile>()).Returns(false);
        this.booksLogic.ImportBookAsync(Arg.Any<IFormFile>()).ThrowsAsync(new InvalidOperationException("fail!"));

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.Received().AddCloudFileImportFail(this.storage, Arg.Any<FileInfoDTO>(), Arg.Any<string>());
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Tests Schedule creates and schedules the job.
    /// </summary>
    /// <returns>A task that returns the job key string.</returns>
    [Test]
    public async Task Schedule_CreatesAndSchedulesJob_ReturnsJobKey()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();

        // Execute
        var jobKey = await ScanForBooks.Schedule(scheduler);

        // Assert
        Assert.That(jobKey, Is.Not.Null.And.Contains("Scan Cloud Storages for Books"));
        await scheduler.Received().ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<IReadOnlyCollection<ITrigger>>(), true);
    }
}
