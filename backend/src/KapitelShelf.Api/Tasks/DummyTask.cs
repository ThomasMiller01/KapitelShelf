// <copyright file="DummyTask.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Quartz;

namespace KapitelShelf.Api.Tasks;

/// <summary>
/// A dummy task for developemnt.
/// </summary>
/// <remarks>REMOVE BEFORE NEXT RELEASE!.</remarks>
[Obsolete("REMOVE BEFORE NEXT RELEASE!")]
public class DummyTask : IJob
{
    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context) => await Task.CompletedTask;
}
