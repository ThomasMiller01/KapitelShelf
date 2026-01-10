// <copyright file="LocalizationProviderTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Localization;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace KapitelShelf.Api.Tests.Localization;

/// <summary>
/// Unit tests for the LocalizationProvider class.
/// </summary>
[TestFixture]
public class LocalizationProviderTests
{
    private IStringLocalizer<DummyResource> localizer;
    private LocalizationProvider<DummyResource> testee;

    /// <summary>
    /// Sets up the testee and fakes before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.localizer = Substitute.For<IStringLocalizer<DummyResource>>();
        this.testee = new LocalizationProvider<DummyResource>(this.localizer);
    }

    /// <summary>
    /// Tests Get returns localized value without args.
    /// </summary>
    [Test]
    public void Get_ReturnsLocalizedValue_WithoutArgs()
    {
        // Setup
        var key = "test.key";
        var localized = new LocalizedString(key, "localized value");

        this.localizer[key, Arg.Any<object[]>()]
            .Returns(localized);

        // Execute
        var result = this.testee.Get(key);

        // Assert
        Assert.That(result, Is.EqualTo("localized value"));
        _ = this.localizer.Received(1)[key, Arg.Any<object[]>()];
    }

    /// <summary>
    /// Tests Get returns localized value with args.
    /// </summary>
    [Test]
    public void Get_ReturnsLocalizedValue_WithArgs()
    {
        // Setup
        var key = "greeting";
        var args = new object[] { "World", 42 };
        var localized = new LocalizedString(key, "Hello World 42");

        this.localizer[key, args]
            .Returns(localized);

        // Execute
        var result = this.testee.Get(key, args);

        // Assert
        Assert.That(result, Is.EqualTo("Hello World 42"));
        _ = this.localizer.Received(1)[key, args];
    }

    /// <summary>
    /// Dummy resource type for the localization generic parameter.
    /// </summary>
    public sealed class DummyResource
    {
    }
}
