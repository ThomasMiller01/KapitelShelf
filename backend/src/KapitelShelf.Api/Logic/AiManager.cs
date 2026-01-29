// <copyright file="AiManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Ai;
using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Resources;
using Microsoft.Extensions.AI;
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
        catch
        {
            this.OnFailedToCreateClient();
            return null;
        }

        var client = new OllamaApiClient(httpClient, defaultModel: modelSetting.Value);
        try
        {
            var isClientRunning = await client.IsRunningAsync(cancellationToken);
            if (!isClientRunning)
            {
                this.OnFailedToCreateClient();
                return null;
            }
        }
        catch
        {
            this.OnFailedToCreateClient();
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

    private void OnFailedToCreateClient()
    {
        _ = this.notifications.AddNotification(
                "AiManagerCreateClientFailed",
                titleArgs: [AiProvider.Ollama.ToString()],
                messageArgs: [AiProvider.Ollama.ToString()],
                type: NotificationTypeDto.Warning,
                severity: NotificationSeverityDto.Low,
                expires: DateTime.UtcNow.AddDays(1),
                source: "Ai Manager",
                ignoreWhenDuplicate: true);
    }
}
