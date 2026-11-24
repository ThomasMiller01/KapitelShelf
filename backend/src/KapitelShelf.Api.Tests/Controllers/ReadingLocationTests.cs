// <copyright file="ReadingLocationTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using KapitelShelf.Api.DTOs.Reader;

namespace KapitelShelf.Api.Tests.Controllers;

/// <summary>
/// Tests validation of legacy and revisioned bookmark requests.
/// </summary>
[TestFixture]
public class ReadingLocationTests
{
    /// <summary>
    /// Verifies the locator format and its pairing with a write id.
    /// </summary>
    /// <param name="suffix">The anchor suffix, or null when no anchor is supplied.</param>
    /// <param name="guarded">Whether the client supplies a write id.</param>
    /// <param name="valid">Whether the request should be accepted.</param>
    [TestCase(null, false, true)]
    [TestCase(null, true, true)]
    [TestCase("0.t.32", true, true)]
    [TestCase("2.i.0", true, true)]
    [TestCase("0.t.32", false, false)]
    [TestCase("-1.t.32", true, false)]
    [TestCase("0.i.2147483648", true, false)]
    [TestCase("0.x.32", true, false)]
    public void ReadingLocation_ValidatesContentAnchor(string? suffix, bool guarded, bool valid)
    {
        var location = new ReadingLocationDTO
        {
            WriteId = guarded ? Guid.NewGuid() : null,
            ContentAnchor = suffix is null ? null : $"1.{new string('a', 64)}.{suffix}",
        };
        Assert.That(Validator.TryValidateObject(location, new ValidationContext(location), null, true), Is.EqualTo(valid));
    }
}
