// <copyright file="TasksController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="TasksController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The tasks logic.</param>
[ApiController]
[Route("tasks")]
public class TasksController(ILogger<TasksController> logger, ITasksLogic logic) : ControllerBase
{
    private readonly ILogger<TasksController> logger = logger;

    private readonly ITasksLogic logic = logic;

    /// <summary>
    /// Fetch all tasks.
    /// </summary>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<ActionResult<IList<TaskDTO>>> GetTasks()
    {
        try
        {
            var tasks = await this.logic.GetTasks();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching tasks");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
