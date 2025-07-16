// <copyright file="CloudStoragesBaseLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic.CloudStorages;

/// <summary>
/// The cloud storages base logic.
/// </summary>
public abstract class CloudStoragesBaseLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper, KapitelShelfSettings settings)
{
#pragma warning disable SA1304 // Non-private readonly fields should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1401 // Fields should be private
    internal readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    internal readonly IMapper mapper = mapper;

    internal readonly KapitelShelfSettings settings = settings;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
#pragma warning restore SA1304 // Non-private readonly fields should begin with upper-case letter

    /// <summary>
    /// Check if OneDrive cloud is configured.
    /// </summary>
    /// <returns>True if OneDirve cloud is configured, otherwise false.</returns>
    public abstract Task<bool> IsConfigured();

    /// <summary>
    /// Configure the OneDrive cloud.
    /// </summary>
    /// <param name="configureCloudDto">The configure cloud dto.</param>
    /// <returns>A task.</returns>
    public abstract Task Configure(ConfigureCloudDTO configureCloudDto);

    /// <summary>
    /// Get the cloud configuration.
    /// </summary>
    /// <returns>The cloud configuration.</returns>
    public abstract Task<CloudConfigurationDTO> GetConfiguration();

    /// <summary>
    /// Get the url for the OAuth flow of OneDrive.
    /// </summary>
    /// <param name="redirectUrl">The url to redirect to after the OAuth flow finished.</param>
    /// <returns>The OAuth url.</returns>
    public abstract Task<string> GetOAuthUrl(string redirectUrl);

    /// <summary>
    /// Gets the OAuth token from the OAuth code.
    /// </summary>
    /// <param name="code">The OAuth code.</param>
    /// <returns>The OAuth tokens.</returns>
    public abstract Task<OAuthTokensDTO> GetOAuthTokensFromCode(string code);

    /// <summary>
    /// Generate the rclone config.
    /// </summary>
    /// <param name="tokens">The OAuth tokens.</param>
    /// <returns>A task.</returns>
    public abstract Task GenerateRCloneConfig(OAuthTokensDTO tokens);

    /// <summary>
    /// List all the cloud storages.
    /// </summary>
    /// <returns>The cloud storages.</returns>
    public abstract Task<List<CloudStorageDTO>> ListCloudStorages();

    /// <summary>
    /// Get the data path for a storage.
    /// </summary>
    /// <param name="cloudTypeDto">The cloud type dto.</param>
    /// <returns>The data path.</returns>
    internal string GetDataPath(CloudTypeDTO cloudTypeDto)
    {
        var cloudTypeSubPath = cloudTypeDto switch
        {
            CloudTypeDTO.OneDrive => "onedrive",
            _ => "unknown_type",
        };

        return Path.Combine(this.settings.DataDir, StaticConstants.CloudStorageConfigurationSubPath, cloudTypeSubPath);
    }
}
