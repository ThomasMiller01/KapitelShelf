// <copyright file="AiManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Ai;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Resources;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using OllamaSharp;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The ai manager provides the chat client based on the current configuration.
/// </summary>
/// <param name="httpClientFactory">The http client factory.</param>
/// <param name="settingsManager">The settings manager.</param>
/// <param name="logger">The logger.</param>
/// <param name="notifications">The notifications.</param>
public class AiManager(
    IHttpClientFactory httpClientFactory,
    IDynamicSettingsManager settingsManager,
    ILogger<AiManager> logger,
    INotificationsLogic notifications) : IAiManager
{
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

    private readonly IDynamicSettingsManager settingsManager = settingsManager;

    private readonly ILogger<AiManager> logger = logger;

    private readonly INotificationsLogic notifications = notifications;

    /// <inheritdoc/>
    public async Task<IChatClient?> GetAsync(CancellationToken cancellationToken = default)
    {
        var configuredSetting = await this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured);
        if (!configuredSetting.Value)
        {
            // provider not configured yet
            return null;
        }

        var providerSetting = await this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiProvider);
        if (!Enum.TryParse(providerSetting.Value, out AiProvider provider))
        {
            // no provider set
            return null;
        }

        return provider switch
        {
            AiProvider.Ollama => await this.CreateOllamaClient(cancellationToken),
            _ => null,
        };
    }

    /// <inheritdoc/>
    public async Task ConfigureCurrentProvider(IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        var configuredSetting = await this.settingsManager.GetAsync<bool>(StaticConstants.DynamicSettingAiProviderConfigured);
        if (configuredSetting.Value)
        {
            // provider already configured
            return;
        }

        var providerSetting = await this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiProvider);
        if (!Enum.TryParse(providerSetting.Value, out AiProvider provider))
        {
            // no provider set
            return;
        }

        var configured = provider switch
        {
            AiProvider.Ollama => await this.ConfigureOllama(progress, cancellationToken),
            _ => false,
        };

        if (configured)
        {
            await this.settingsManager.SetAsync(StaticConstants.DynamicSettingAiProviderConfigured, true);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> FeatureEnabled(AiFeatures feature)
    {
        var enabledFeaturesSetting = await this.settingsManager.GetAsync<List<string>>(StaticConstants.DynamicSettingAiEnabledFeatures);
        return enabledFeaturesSetting.Value.Contains(feature.ToString());
    }

    /// <inheritdoc/>
    public async Task<T?> GetStructuredResponse<T>(string userPrompt, string? systemPrompt = null)
        where T : class
    {
        var ai = await this.GetAsync();
        if (ai is null)
        {
            return null;
        }

        var aiOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema<AiGenerateCategoriesTagsResultDTO>(),
        };

        // execute ai prompt
        ChatResponse response;
        try
        {
            response = await ai.GetResponseAsync(
                [
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, userPrompt),
                ],
                options: aiOptions);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Get structured ai response failed");
            return null;
        }

        // parse ai response
        var text = response.Text.Trim() ?? string.Empty;
        var parsed = TryParseJsonResponse<T>(text);
        if (parsed is null)
        {
            // retry once with stricter instruction
            try
            {
                response = await ai.GetResponseAsync(
                    [
                        new ChatMessage(ChatRole.System, systemPrompt),
                        new ChatMessage(ChatRole.User, userPrompt),
                        new ChatMessage(ChatRole.User, "Your previous output was invalid. Return ONLY valid JSON now, no extra text."),
                    ],
                    options: aiOptions);

                text = response.Text?.Trim() ?? string.Empty;
                parsed = TryParseJsonResponse<T>(text);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Get structured ai response retry failed");
                return null;
            }
        }

        if (parsed is null)
        {
            this.logger.LogWarning("AI returned unparseable output: {Output}", text);
            return null;
        }

        return parsed;
    }

    private async Task<OllamaApiClient?> CreateOllamaClient(CancellationToken cancellationToken)
    {
        var urlSetting = await this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaUrl);
        var modelSetting = await this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel);
        if (string.IsNullOrEmpty(urlSetting.Value) || string.IsNullOrEmpty(modelSetting.Value))
        {
            // provider not yet configured, ignore
            return null;
        }

        HttpClient httpClient;
        try
        {
            httpClient = this.httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(urlSetting.Value);
        }
        catch (Exception ex)
        {
            this.OnFailedToCreateClient(ex.Message);
            return null;
        }

        var client = new OllamaApiClient(httpClient, defaultModel: modelSetting.Value);
        try
        {
            var isClientRunning = await client.IsRunningAsync(cancellationToken);
            if (!isClientRunning)
            {
                this.OnFailedToCreateClient("Provider is not running.");
                return null;
            }
        }
        catch (Exception ex)
        {
            this.OnFailedToCreateClient(ex.Message);
            return null;
        }

        return client;
    }

    private async Task<bool> ConfigureOllama(IProgress<int>? progress, CancellationToken cancellationToken)
    {
        var modelSetting = await this.settingsManager.GetAsync<string>(StaticConstants.DynamicSettingAiOllamaModel);
        if (string.IsNullOrEmpty(modelSetting.Value))
        {
            return false;
        }

        var client = await this.CreateOllamaClient(cancellationToken);
        if (client is null)
        {
            return false;
        }

        try
        {
            await foreach (var status in client.PullModelAsync(modelSetting.Value, cancellationToken))
            {
                if (status?.Percent is not null)
                {
                    progress?.Report((int)Math.Round(status.Percent));
                }
            }
        }
        catch (Exception ex)
        {
            _ = this.notifications.AddNotification(
                "AiManagerConfigureProviderFailed",
                titleArgs: [AiProvider.Ollama.ToString()],
                messageArgs: [AiProvider.Ollama.ToString()],
                type: NotificationTypeDto.Error,
                severity: NotificationSeverityDto.Medium,
                source: "Ai Manager");

            this.logger.LogError(ex, "Could not pull model '{Model}' in ollama", modelSetting.Value);
            return false;
        }

        return true;
    }

    private static T? TryParseJsonResponse<T>(string text)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(text.Trim());
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private void OnFailedToCreateClient(string reason)
    {
        _ = this.notifications.AddNotification(
                "AiManagerCreateClientFailed",
                titleArgs: [AiProvider.Ollama.ToString()],
                messageArgs: [AiProvider.Ollama.ToString(), reason],
                type: NotificationTypeDto.Warning,
                severity: NotificationSeverityDto.Low,
                expires: DateTime.UtcNow.AddDays(1),
                source: "Ai Manager",
                ignoreWhenDuplicate: true);
    }
}
