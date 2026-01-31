// <copyright file="AiManagerTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Net;
using System.Text;
using KapitelShelf.Api.DTOs.Ai;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.DTOs.Settings;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Resources;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Tests for <see cref="AiManager"/>.
/// </summary>
[TestFixture]
public class AiManagerTests
{
    private IHttpClientFactory httpClientFactory;
    private IDynamicSettingsManager settingsManager;
    private ILogger<AiManager> logger;
    private INotificationsLogic notifications;

    private AiManager testee;

    /// <summary>
    /// Setup test fakes.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.httpClientFactory = Substitute.For<IHttpClientFactory>();
        this.settingsManager = Substitute.For<IDynamicSettingsManager>();
        this.logger = Substitute.For<ILogger<AiManager>>();
        this.notifications = Substitute.For<INotificationsLogic>();

        this.testee = new AiManager(
            this.httpClientFactory,
            this.settingsManager,
            this.logger,
            this.notifications);
    }

    /// <summary>
    /// Tests <see cref="AiManager.GetAsync"/> returns null when provider is not configured.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetAsync_ReturnsNull_WhenProviderNotConfigured()
    {
        // setup
        this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured)
            .Returns(new SettingsDTO<bool> { Value = false });

        // Execute
        var result = await this.testee.GetAsync();

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests <see cref="AiManager.GetAsync"/> returns null when provider enum is invalid.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetAsync_ReturnsNull_WhenProviderInvalid()
    {
        // setup
        this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured)
            .Returns(new SettingsDTO<bool> { Value = true });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiProvider)
            .Returns(new SettingsDTO<string> { Value = "NotAProvider" });

        // Execute
        var result = await this.testee.GetAsync();

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests <see cref="AiManager.GetAsync"/> returns null when provider is not supported.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetAsync_ReturnsNull_WhenProviderNotSupported()
    {
        // setup
        this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured)
            .Returns(new SettingsDTO<bool> { Value = true });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiProvider)
            .Returns(new SettingsDTO<string> { Value = AiProvider.None.ToString() });

        // Execute
        var result = await this.testee.GetAsync();

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests <see cref="AiManager.GetAsync"/> returns client for ollama when configuration is valid and running.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetAsync_ReturnsClient_WhenOllamaConfiguredAndRunning()
    {
        // setup
        this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured)
            .Returns(new SettingsDTO<bool> { Value = true });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiProvider)
            .Returns(new SettingsDTO<string> { Value = AiProvider.Ollama.ToString() });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaUrl)
            .Returns(new SettingsDTO<string> { Value = "http://localhost:11434" });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = "llama3.1:8b" });

        this.httpClientFactory.CreateClient()
            .Returns(new HttpClient(new FakeHttpMessageHandler(req =>
            {
                // minimal endpoint used by OllamaSharp for IsRunning
                // respond success for anything
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json"),
                };
            })));

        // Execute
        var result = await this.testee.GetAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Tests <see cref="AiManager.ConfigureCurrentProvider"/> returns early when already configured.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ConfigureCurrentProvider_DoesNothing_WhenAlreadyConfigured()
    {
        // setup
        this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured)
            .Returns(new SettingsDTO<bool> { Value = true });

        // Execute
        await this.testee.ConfigureCurrentProvider();

        // Assert
        await this.settingsManager.DidNotReceive()
            .SetAsync(StaticConstants.DynamicSettingAiProviderConfigured, Arg.Any<bool>());
    }

    /// <summary>
    /// Tests <see cref="AiManager.ConfigureCurrentProvider"/> returns early when provider is invalid.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ConfigureCurrentProvider_DoesNothing_WhenProviderInvalid()
    {
        // setup
        this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured)
            .Returns(new SettingsDTO<bool> { Value = false });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiProvider)
            .Returns(new SettingsDTO<string> { Value = "NotAProvider" });

        // Execute
        await this.testee.ConfigureCurrentProvider();

        // Assert
        await this.settingsManager.DidNotReceive()
            .SetAsync(StaticConstants.DynamicSettingAiProviderConfigured, Arg.Any<bool>());
    }

    /// <summary>
    /// Tests <see cref="AiManager.ConfigureCurrentProvider"/> sets configured flag when provider config succeeds.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ConfigureCurrentProvider_SetsConfiguredTrue_WhenConfigureSucceeds()
    {
        // setup
        this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured)
            .Returns(new SettingsDTO<bool> { Value = false });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiProvider)
            .Returns(new SettingsDTO<string> { Value = AiProvider.Ollama.ToString() });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaUrl)
            .Returns(new SettingsDTO<string> { Value = "http://localhost:11434" });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = "llama3.1:8b" });

        this.httpClientFactory.CreateClient().Returns(
            new HttpClient(new FakeHttpMessageHandler(req =>
            {
                var path = req.RequestUri!.AbsolutePath;

                // Ollama "is running" checks (OllamaSharp typically hits one of these)
                if (path.EndsWith("/api/version", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(/*lang=json,strict*/ """{"version":"0.0.0"}""", Encoding.UTF8, "application/json"),
                    };
                }

                if (path.EndsWith("/api/tags", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(/*lang=json,strict*/ """{"models":[]}""", Encoding.UTF8, "application/json"),
                    };
                }

                // model pull (NDJSON stream)
                if (path.EndsWith("/api/pull", StringComparison.OrdinalIgnoreCase))
                {
                    // minimal NDJSON that completes without error
                    var ndjson =
                        """
                        {"status":"pulling manifest","completed":1,"total":1,"percent":100}
                        {"status":"success","completed":1,"total":1,"percent":100}
                        """;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(ndjson, Encoding.UTF8, "application/x-ndjson"),
                    };
                }

                // default: OK for anything else
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json"),
                };
            }))
            {
                // optional: your code sets BaseAddress, but safe to keep it coherent here too
                BaseAddress = new Uri("http://localhost:11434"),
            });

        // Execute
        await this.testee.ConfigureCurrentProvider();

        // Assert
        await this.settingsManager.Received(1)
            .SetAsync(StaticConstants.DynamicSettingAiProviderConfigured, true);
    }

    /// <summary>
    /// Tests <see cref="AiManager.ConfigureCurrentProvider"/> does not set configured flag when provider config fails.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ConfigureCurrentProvider_DoesNotSetConfigured_WhenConfigureFails()
    {
        // setup
        this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured)
            .Returns(new SettingsDTO<bool> { Value = false });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiProvider)
            .Returns(new SettingsDTO<string> { Value = AiProvider.Ollama.ToString() });

        // missing model causes ConfigureOllama to return false
        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = string.Empty });

        // Execute
        await this.testee.ConfigureCurrentProvider();

        // Assert
        await this.settingsManager.DidNotReceive()
            .SetAsync(StaticConstants.DynamicSettingAiProviderConfigured, true);
    }

    /// <summary>
    /// Tests <see cref="AiManager.FeatureEnabled"/> returns true when feature is in list.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task FeatureEnabled_ReturnsTrue_WhenFeatureIsEnabled()
    {
        // setup
        this.settingsManager.GetAsync<List<string>>(StaticConstants.DynamicSettingAiEnabledFeatures)
            .Returns(new SettingsDTO<List<string>>
            {
                Value = [AiFeatures.BookImportMetadataGeneration.ToString()],
            });

        // Execute
        var result = await this.testee.FeatureEnabled(AiFeatures.BookImportMetadataGeneration);

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests <see cref="AiManager.FeatureEnabled"/> returns false when feature is not in list.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task FeatureEnabled_ReturnsFalse_WhenFeatureIsDisabled()
    {
        // setup
        this.settingsManager.GetAsync<List<string>>(StaticConstants.DynamicSettingAiEnabledFeatures)
            .Returns(new SettingsDTO<List<string>> { Value = [] });

        // Execute
        var result = await this.testee.FeatureEnabled(AiFeatures.BookImportMetadataGeneration);

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests <see cref="AiManager.GetStructuredResponse{T}"/> returns null when ai client is null.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStructuredResponse_ReturnsNull_WhenAiClientIsNull()
    {
        // setup
        this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured)
            .Returns(new SettingsDTO<bool> { Value = false });

        // Execute
        var result = await this.testee.GetStructuredResponse<AiGenerateCategoriesTagsResultDTO>("user", "system");

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests <see cref="AiManager.GetStructuredResponse{T}"/> returns parsed result on first valid response.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStructuredResponse_ReturnsParsed_WhenResponseValidFirstTry()
    {
        // setup
        var ai = Substitute.For<IChatClient>();
        ai.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ChatResponse(new ChatMessage(ChatRole.Assistant, /*lang=json,strict*/ """{ "Categories": ["A"], "Tags": ["B"] }""")));

        var sut = this.CreateTesteeWithGetAsyncReturning(ai);

        // Execute
        var result = await sut.GetStructuredResponse<AiGenerateCategoriesTagsResultDTO>("user", "system");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Categories, Is.EquivalentTo(["A"]));
            Assert.That(result.Tags, Is.EquivalentTo(["B"]));
        });

        await ai.Received(1).GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests <see cref="AiManager.GetStructuredResponse{T}"/> retries once when first response is invalid then returns parsed.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStructuredResponse_RetriesOnce_WhenFirstInvalid_ThenSucceeds()
    {
        // setup
        var ai = new FakeChatClient(
            () => new ChatResponse(new ChatMessage(ChatRole.Assistant, "not json")),
            () => new ChatResponse(new ChatMessage(ChatRole.Assistant, /*lang=json,strict*/ """{ "Categories": ["A"], "Tags": ["B"] }""")));

        var sut = this.CreateTesteeWithGetAsyncReturning(ai);

        // execute
        var result = await sut.GetStructuredResponse<AiGenerateCategoriesTagsResultDTO>("user", "system");

        Assert.Multiple(() =>
        {
            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(ai.CallCount, Is.EqualTo(2));
        });
    }

    /// <summary>
    /// Tests <see cref="AiManager.GetStructuredResponse{T}"/> returns null when both responses are invalid.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStructuredResponse_ReturnsNull_WhenBothResponsesInvalid()
    {
        // setup
        var ai = Substitute.For<IChatClient>();
        ai.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ChatResponse([new ChatMessage(ChatRole.Assistant, "not json"), new ChatMessage(ChatRole.Assistant, "still not json")]));

        var sut = this.CreateTesteeWithGetAsyncReturning(ai);

        // Execute
        var result = await sut.GetStructuredResponse<AiGenerateCategoriesTagsResultDTO>("user", "system");

        // Assert
        Assert.That(result, Is.Null);
        await ai.Received(2).GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests <see cref="AiManager.GetStructuredResponse{T}"/> returns null when ai throws on first call.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStructuredResponse_ReturnsNull_WhenAiThrowsOnFirstCall()
    {
        // setup
        var ai = Substitute.For<IChatClient>();
        ai.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns<Task<ChatResponse>>(_ => throw new InvalidOperationException("boom"));

        var sut = this.CreateTesteeWithGetAsyncReturning(ai);

        // Execute
        var result = await sut.GetStructuredResponse<AiGenerateCategoriesTagsResultDTO>("user", "system");

        // Assert
        Assert.That(result, Is.Null);
        await ai.Received(1).GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests <see cref="AiManager.GetStructuredResponse{T}"/> returns null when ai throws on retry call.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task GetStructuredResponse_ReturnsNull_WhenAiThrowsOnRetry()
    {
        // setup
        var ai = Substitute.For<IChatClient>();

        ai.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(
                _ => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "not json"))),
                _ => throw new InvalidOperationException("boom"));

        var sut = this.CreateTesteeWithGetAsyncReturning(ai);

        // Execute
        var result = await sut.GetStructuredResponse<AiGenerateCategoriesTagsResultDTO>("user", "system");

        // Assert
        Assert.That(result, Is.Null);
        await ai.Received(2).GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests <see cref="AiManager.CreateOllamaClient"/> returns null when url or model missing.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CreateOllamaClient_ReturnsNull_WhenUrlOrModelMissing()
    {
        // setup
        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaUrl)
            .Returns(new SettingsDTO<string> { Value = string.Empty });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = "model" });

        // Execute
        var result = await this.testee.CreateOllamaClient(default);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests <see cref="AiManager.CreateOllamaClient"/> returns null when http client creation throws and sends notification.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CreateOllamaClient_ReturnsNull_WhenHttpClientFactoryThrows()
    {
        // setup
        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaUrl)
            .Returns(new SettingsDTO<string> { Value = "http://localhost:11434" });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = "model" });

        this.httpClientFactory.CreateClient()
            .Returns(_ => throw new InvalidOperationException("boom"));

        // Execute
        var result = await this.testee.CreateOllamaClient(default);

        // Assert
        Assert.That(result, Is.Null);

        _ = this.notifications.Received(1).AddNotification(
            "AiManagerCreateClientFailed",
            titleArgs: Arg.Any<object[]>(),
            messageArgs: Arg.Any<object[]>(),
            type: NotificationTypeDto.Warning,
            severity: NotificationSeverityDto.Low,
            expires: Arg.Any<DateTime?>(),
            source: "Ai Manager",
            ignoreWhenDuplicate: true);
    }

    /// <summary>
    /// Tests <see cref="AiManager.CreateOllamaClient"/> returns null when provider is not running.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CreateOllamaClient_ReturnsNull_WhenProviderNotRunning()
    {
        // setup
        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaUrl)
            .Returns(new SettingsDTO<string> { Value = "http://localhost:11434" });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = "model" });

        this.httpClientFactory.CreateClient()
            .Returns(new HttpClient(new FakeHttpMessageHandler(req =>
            {
                // return 500 so IsRunning fails
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json"),
                };
            })));

        // Execute
        var result = await this.testee.CreateOllamaClient(default);

        // Assert
        Assert.That(result, Is.Null);
        _ = this.notifications.Received(1).AddNotification(
            "AiManagerCreateClientFailed",
            titleArgs: Arg.Any<object[]>(),
            messageArgs: Arg.Any<object[]>(),
            type: NotificationTypeDto.Warning,
            severity: NotificationSeverityDto.Low,
            expires: Arg.Any<DateTime?>(),
            source: "Ai Manager",
            ignoreWhenDuplicate: true);
    }

    /// <summary>
    /// Tests <see cref="AiManager.CreateOllamaClient"/> returns client when running.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task CreateOllamaClient_ReturnsClient_WhenRunning()
    {
        // setup
        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaUrl)
            .Returns(new SettingsDTO<string> { Value = "http://localhost:11434" });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = "model" });

        this.httpClientFactory.CreateClient()
            .Returns(new HttpClient(new FakeHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json"),
                };
            })));

        // Execute
        var result = await this.testee.CreateOllamaClient(default);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Tests <see cref="AiManager.ConfigureOllama"/> returns false when model missing.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ConfigureOllama_ReturnsFalse_WhenModelMissing()
    {
        // setup
        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = string.Empty });

        // Execute
        var result = await this.testee.ConfigureOllama(null, default);

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests <see cref="AiManager.ConfigureOllama"/> returns false when client cannot be created.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ConfigureOllama_ReturnsFalse_WhenClientNull()
    {
        // setup
        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = "model" });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaUrl)
            .Returns(new SettingsDTO<string> { Value = string.Empty }); // forces CreateOllamaClient null

        // Execute
        var result = await this.testee.ConfigureOllama(null, default);

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests <see cref="AiManager.ConfigureOllama"/> returns true and reports progress when pull stream provides percentages.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ConfigureOllama_ReturnsTrue_AndReportsProgress()
    {
        // setup
        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaUrl)
            .Returns(new SettingsDTO<string> { Value = "http://localhost:11434" });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = "model" });

        var reported = new List<int>();
        var progress = new ListProgress(reported);

        this.httpClientFactory.CreateClient()
            .Returns(new HttpClient(new FakeHttpMessageHandler(req =>
            {
                var path = req.RequestUri!.AbsolutePath;

                // ollamasharp running check (some versions: /api/version, others: /api/tags)
                if (path.EndsWith("/api/version", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(/*lang=json,strict*/ """{"version":"0.0.0"}""", Encoding.UTF8, "application/json"),
                    };
                }

                if (path.EndsWith("/api/tags", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(/*lang=json,strict*/ """{"models":[]}""", Encoding.UTF8, "application/json"),
                    };
                }

                // pull endpoint: NDJSON with total/completed (what the real API emits)
                if (path.EndsWith("/api/pull", StringComparison.OrdinalIgnoreCase))
                {
                    // important: newline separated JSON objects, end with '\n'
                    var ndjson = /*lang=json,strict*/
                        "{\"status\":\"downloading\",\"total\":100,\"completed\":12}\n" + /*lang=json,strict*/
                        "{\"status\":\"downloading\",\"total\":100,\"completed\":67}\n" + /*lang=json,strict*/
                        "{\"status\":\"success\"}\n";

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(ndjson, Encoding.UTF8, "application/x-ndjson"),
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json"),
                };
            })));

        // execute
        var result = await this.testee.ConfigureOllama(progress, default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(reported, Is.Not.Empty);

            // be tolerant to rounding / duplicate reports
            Assert.That(reported.Any(x => x is >= 10 and <= 15), Is.True);
            Assert.That(reported.Any(x => x is >= 65 and <= 70), Is.True);
        });
    }

    /// <summary>
    /// Tests <see cref="AiManager.ConfigureOllama"/> returns false and sends notification when pull throws.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ConfigureOllama_ReturnsFalse_WhenPullThrows()
    {
        // setup
        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaUrl)
            .Returns(new SettingsDTO<string> { Value = "http://localhost:11434" });

        this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel)
            .Returns(new SettingsDTO<string> { Value = "model" });

        this.httpClientFactory.CreateClient()
            .Returns(new HttpClient(new FakeHttpMessageHandler(_ =>
            {
                // make PullModelAsync fail by returning bad response
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("error"),
                };
            })));

        // Execute
        var result = await this.testee.ConfigureOllama(null, default);

        // Assert
        Assert.That(result, Is.False);

        _ = this.notifications.Received(1).AddNotification(
            "AiManagerCreateClientFailed",
            titleArgs: Arg.Any<object[]>(),
            messageArgs: Arg.Any<object[]>(),
            type: NotificationTypeDto.Warning,
            severity: NotificationSeverityDto.Low,
            expires: Arg.Any<DateTime?>(),
            source: "Ai Manager",
            ignoreWhenDuplicate: true);
    }

    /// <summary>
    /// Tests <see cref="AiManager.TryParseJsonResponse{T}"/> returns null when empty.
    /// </summary>
    [Test]
    public void TryParseJsonResponse_ReturnsNull_WhenEmpty()
    {
        // Execute
        var result = AiManager.TryParseJsonResponse<AiGenerateCategoriesTagsResultDTO>(string.Empty);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests <see cref="AiManager.TryParseJsonResponse{T}"/> returns null when invalid.
    /// </summary>
    [Test]
    public void TryParseJsonResponse_ReturnsNull_WhenInvalid()
    {
        // Execute
        var result = AiManager.TryParseJsonResponse<AiGenerateCategoriesTagsResultDTO>("not json");

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests <see cref="AiManager.TryParseJsonResponse{T}"/> returns parsed object when valid.
    /// </summary>
    [Test]
    public void TryParseJsonResponse_ReturnsParsed_WhenValid()
    {
        // setup
        var json = /*lang=json,strict*/ """{ "Categories": ["A"], "Tags": ["B"] }""";

        // Execute
        var result = AiManager.TryParseJsonResponse<AiGenerateCategoriesTagsResultDTO>(json);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Categories, Is.EquivalentTo(["A"]));
            Assert.That(result.Tags, Is.EquivalentTo(["B"]));
        });
    }

    /// <summary>
    /// Tests <see cref="AiManager.OnFailedToCreateClient"/> creates expected notification.
    /// </summary>
    [Test]
    public void OnFailedToCreateClient_SendsNotification()
    {
        // setup
        var reason = "boom";

        // Execute
        this.testee.OnFailedToCreateClient(reason);

        // Assert
        _ = this.notifications.Received(1).AddNotification(
            "AiManagerCreateClientFailed",
            titleArgs: Arg.Any<object[]>(),
            messageArgs: Arg.Any<object[]>(),
            type: NotificationTypeDto.Warning,
            severity: NotificationSeverityDto.Low,
            expires: Arg.Any<DateTime?>(),
            source: "Ai Manager",
            ignoreWhenDuplicate: true);
    }

    private AiManager CreateTesteeWithGetAsyncReturning(IChatClient client)
    {
        var sut = Substitute.ForPartsOf<AiManager>(
            this.httpClientFactory,
            this.settingsManager,
            this.logger,
            this.notifications);

        sut.When(x => x.GetAsync(Arg.Any<CancellationToken>()))
            .DoNotCallBase();

        sut.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IChatClient?>(client));

        return sut;
    }

    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> responder = responder;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(this.responder(request));
    }

    private sealed class ListProgress(List<int> target) : IProgress<int>
    {
        private readonly List<int> target = target;

        public void Report(int value) => this.target.Add(value);
    }

    private sealed class FakeChatClient : IChatClient
    {
        private readonly Queue<Func<ChatResponse>> responses = new();

        public FakeChatClient(params Func<ChatResponse>[] responses)
        {
            foreach (var r in responses)
            {
                this.responses.Enqueue(r);
            }
        }

        public int CallCount { get; private set; }

        public void Dispose() => throw new NotImplementedException();

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            this.CallCount++;

            if (this.responses.Count == 0)
            {
                throw new InvalidOperationException("no more responses configured");
            }

            return Task.FromResult(this.responses.Dequeue().Invoke());
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => throw new NotImplementedException();
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
