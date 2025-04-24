// <copyright file="VersionController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="VersionController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("version")]
public class VersionController(ILogger<BooksController> logger) : ControllerBase
{
    private readonly ILogger<BooksController> logger = logger;

    /// <summary>
    /// Gets the backend version.
    /// </summary>
    /// <returns>The backend version.</returns>
    [HttpGet]
    public ActionResult<string> GetVersion()
    {
        try
        {
            var informationalVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            var version = informationalVersion?.Split("+")[0];
            return Ok(version);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error parsing version");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
