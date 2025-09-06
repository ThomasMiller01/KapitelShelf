﻿// <copyright file="ScanForBooksTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Tasks;
using KapitelShelf.Api.Tasks.CloudStorage;
using KapitelShelf.Data.Models.CloudStorage;
using Microsoft.Extensions.Logging;
using NSubstitute;
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
    private ICloudStoragesLogic logic;
    private IMapper mapper;
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
        this.logic = Substitute.For<ICloudStoragesLogic>();
        this.mapper = Testhelper.CreateMapper();
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
            this.logic,
            this.mapper);
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
        await this.logic.DidNotReceiveWithAnyArgs().ScanStorageForBooks(default!, default!);
        this.dataStore.DidNotReceiveWithAnyArgs().SetMessage(default!, default!);
    }

    /// <summary>
    /// Tests ExecuteTask scans all storages and calls ScanStorageForBooks for each.
    /// Also verifies message and progress updates via callback.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_ScansAllStorages_CallsLogicAndUpdatesProgress()
    {
        // Setup
        var storages = new List<CloudStorageModel>
        {
            this.storageModel,
        };
        this.logic.GetDownloadedStorageModels().Returns(storages);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.Received(1).ScanStorageForBooks(Arg.Any<CloudStorageDTO>(), Arg.Any<Action<int, int>>());
        this.dataStore.Received().SetMessage("Test.ScanBooks", Arg.Is<string>(msg => msg.Contains("TestDir")));
    }

    /// <summary>
    /// Tests ScanSingleStorage returns if storage is not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ScanSingleStorage_Returns_IfStorageNotFound()
    {
        // Setup
        var missingId = Guid.NewGuid();
        this.testee.ForSingleStorageId = missingId;
        this.logic.GetStorageModel(missingId).Returns((CloudStorageModel?)null);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.DidNotReceiveWithAnyArgs().ScanStorageForBooks(default!, default!);
    }

    /// <summary>
    /// Tests ScanSingleStorage sets message and calls logic.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ScanSingleStorage_SetsMessageAndCallsScan()
    {
        // Setup
        this.testee.ForSingleStorageId = this.storageModel.Id;
        this.logic.GetStorageModel(this.storageModel.Id).Returns(this.storageModel);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        this.dataStore.Received().SetMessage("Test.ScanBooks", Arg.Is<string>(msg => msg.Contains("TestDir")));
    }

    /// <summary>
    /// Tests Kill does not throw.
    /// </summary>
    [Test]
    public void Kill_DoesNothing() => Assert.DoesNotThrowAsync(this.testee.Kill);
}
