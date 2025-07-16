// <copyright file="OneDriveLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using System.Text.Json;
using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models.CloudStorage;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic.CloudStorages;

/// <summary>
/// The OneDrive cloud storage logic.
/// </summary>
public class OneDriveLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper, KapitelShelfSettings settings) : CloudStoragesBaseLogic(dbContextFactory, mapper, settings)
{
    /// <inheritdoc/>
    public override async Task<bool> IsConfigured()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.CloudConfiguration
            .Where(x => x.Type == CloudType.OneDrive)
            .AnyAsync();
    }

    /// <inheritdoc/>
    public override async Task Configure(ConfigureCloudDTO configureCloudDto)
    {
        ArgumentNullException.ThrowIfNull(configureCloudDto);

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        if (await this.IsConfigured())
        {
            // update the current configuration
            var configuration = await context.CloudConfiguration
                .FirstOrDefaultAsync(x => x.Type == CloudType.OneDrive);

            ArgumentNullException.ThrowIfNull(configuration);

            configuration.OAuthClientId = configureCloudDto.OAuthClientId;
        }
        else
        {
            // create a new configuration
            var configuration = new CloudConfigurationModel
            {
                Type = CloudType.OneDrive,
                OAuthClientId = configureCloudDto.OAuthClientId,
            };
            context.CloudConfiguration.Add(configuration);
        }

        // invalidate all cloud storages and require re-authentication
        await context.CloudStorages
            .Where(x => x.Type == CloudType.OneDrive)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.NeedsReAuthentication, true));

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public override async Task<CloudConfigurationDTO> GetConfiguration()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.CloudConfiguration
                .Where(x => x.Type == CloudType.OneDrive)
                .Select(x => this.mapper.Map<CloudConfigurationDTO>(x))
                .FirstAsync();
    }

    /// <inheritdoc/>
    public override async Task<string> GetOAuthUrl(string redirectUrl)
    {
        var configuration = await this.GetConfiguration();

        var stateInfo = new OAuthStateInfoDTO
        {
            RedirectUrl = redirectUrl,
        };
        var stateInfoJson = JsonSerializer.Serialize(stateInfo);
        var stateInfoBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(stateInfoJson));

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = configuration.OAuthClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = $"{this.settings.Domain}/cloudstorage/onedrive/oauth/callback",
            ["response_mode"] = "query",
            ["scope"] = "offline_access Files.ReadWrite.All User.Read",
            ["state"] = stateInfoBase64,
        };
        var queryString = string.Join("&", queryParams.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
        return "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?" + queryString;
    }

    /// <inheritdoc/>
    public override async Task<OAuthTokensDTO> GetOAuthTokensFromCode(string code)
    {
        var configuration = await this.GetConfiguration();

        using var httpClient = new HttpClient();
        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", configuration.OAuthClientId),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", $"{this.settings.Domain}/cloudstorage/onedrive/oauth/callback"),
        ]);

        var response = await httpClient.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(responseString);
        var root = json.RootElement;

        return new OAuthTokensDTO
        {
            AccessToken = root.GetProperty("access_token").GetString() ?? string.Empty,
            RefreshToken = root.GetProperty("refresh_token").GetString() ?? string.Empty,
            ExpiresIn = root.GetProperty("expires_in").GetInt32(),
        };
    }

    /// <inheritdoc/>
    public override async Task GenerateRCloneConfig(OAuthTokensDTO tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var configuration = await this.GetConfiguration();
        ArgumentNullException.ThrowIfNull(configuration);

        // build the token JSON
        var expiry = DateTimeOffset.Now.AddSeconds(tokens.ExpiresIn).ToString("o");
        var tokenJson = $"{{\"access_token\":\"{tokens.AccessToken}\",\"token_type\":\"Bearer\",\"refresh_token\":\"{tokens.RefreshToken}\",\"expiry\":\"{expiry}\",\"experies_in\":\"{tokens.ExpiresIn}\"}}";

        var (driveId, email, name) = await GetDriveDetails(tokens.AccessToken);

        // build the rclone config content
        var sb = new StringBuilder();
        sb.AppendLine("[MyOneDriveTest]");
        sb.AppendLine("type = onedrive");
        sb.AppendLine(CultureInfo.CurrentCulture, $"client_id = {configuration.OAuthClientId}");
        sb.AppendLine(CultureInfo.CurrentCulture, $"token = {tokenJson}");
        sb.AppendLine(CultureInfo.CurrentCulture, $"drive_id = {driveId}");
        sb.AppendLine("drive_type = personal");

        // ensure the directory exists
        var cloudTypePath = this.GetDataPath(CloudTypeDTO.OneDrive);
        Directory.CreateDirectory(cloudTypePath);

        // overwrite the file with the new config
        var rclonePath = Path.Combine(cloudTypePath, email, StaticConstants.CloudStorageRCloneFileName);
        File.WriteAllText(rclonePath, sb.ToString());

        // add new cloud storage
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var cloudStorage = await context.CloudStorages
            .Where(x => x.Type == CloudType.OneDrive && x.CloudOwnerEmail == email)
            .FirstOrDefaultAsync();

        // create new cloud storage if none exists for this email
        if (cloudStorage is null)
        {
            cloudStorage = new CloudStorageModel
            {
                Type = CloudType.OneDrive,
            };
            context.Add(cloudStorage);
        }

        // update information
        cloudStorage.RCloneConfig = rclonePath;
        cloudStorage.NeedsReAuthentication = false;
        cloudStorage.CloudOwnerEmail = email;
        cloudStorage.CloudOwnerName = name;

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public override async Task<List<CloudStorageDTO>> ListCloudStorages()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return context.CloudStorages
            .Where(x => x.Type == CloudType.OneDrive)
            .Select(x => this.mapper.Map<CloudStorageDTO>(x))
            .ToList();
    }

    private static async Task<(string driveId, string email, string name)> GetDriveDetails(string accessToken)
    {
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me/drive");
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(responseString);

        var driveId = json.RootElement.GetProperty("id").GetString();
        ArgumentNullException.ThrowIfNull(driveId);

        var email = json.RootElement.GetProperty("owner").GetProperty("user").GetProperty("email").GetString();
        ArgumentNullException.ThrowIfNull(email);

        var name = json.RootElement.GetProperty("owner").GetProperty("user").GetProperty("displayName").GetString();
        ArgumentNullException.ThrowIfNull(name);

        return (driveId, email, name);
    }
}
