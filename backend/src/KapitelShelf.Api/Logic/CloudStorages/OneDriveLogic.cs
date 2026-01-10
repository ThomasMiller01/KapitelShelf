// <copyright file="OneDriveLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using System.Text.Json;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces.CloudStorages;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models.CloudStorage;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic.CloudStorages;

/// <summary>
/// The OneDrive cloud storage logic.
/// </summary>
public class OneDriveLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper, KapitelShelfSettings settings, ICloudStoragesLogic baseLogic, ICloudStorage fileStorage) : IOneDriveLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    private readonly KapitelShelfSettings settings = settings;

    private readonly ICloudStoragesLogic baseLogic = baseLogic;

    private readonly ICloudStorage fileStorage = fileStorage;

    /// <inheritdoc/>
    public async Task<string> GetOAuthUrl(string redirectUrl)
    {
        var configuration = await this.baseLogic.GetConfiguration(CloudTypeDTO.OneDrive);

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
    public async Task<OAuthTokensDTO> GetOAuthTokensFromCode(string code)
    {
        var configuration = await this.baseLogic.GetConfiguration(CloudTypeDTO.OneDrive);

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
    public async Task GenerateRCloneConfig(OAuthTokensDTO tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var configuration = await this.baseLogic.GetConfiguration(CloudTypeDTO.OneDrive);
        ArgumentNullException.ThrowIfNull(configuration);

        // build the token JSON
        var expiry = DateTimeOffset.Now.AddSeconds(tokens.ExpiresIn).ToString("o");
        var tokenJson = $"{{\"access_token\":\"{tokens.AccessToken}\",\"token_type\":\"Bearer\",\"refresh_token\":\"{tokens.RefreshToken}\",\"expiry\":\"{expiry}\",\"experies_in\":\"{tokens.ExpiresIn}\"}}";

        var (driveId, email, name) = await GetDriveDetails(tokens.AccessToken);

        // build the rclone config content
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.CurrentCulture, $"[{StaticConstants.CloudStorageRCloneConfigName}]");
        sb.AppendLine("type = onedrive");
        sb.AppendLine(CultureInfo.CurrentCulture, $"client_id = {configuration.OAuthClientId}");
        sb.AppendLine(CultureInfo.CurrentCulture, $"token = {tokenJson}");
        sb.AppendLine(CultureInfo.CurrentCulture, $"drive_id = {driveId}");
        sb.AppendLine("drive_type = personal");

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
        cloudStorage.NeedsReAuthentication = false;
        cloudStorage.IsDownloaded = false;
        cloudStorage.CloudOwnerEmail = email;
        cloudStorage.CloudOwnerName = name;

        var rclonePath = this.fileStorage.FullPath(this.mapper.CloudStorageModelToCloudStorageDto(cloudStorage), StaticConstants.CloudStorageRCloneFileName);
        cloudStorage.RCloneConfig = rclonePath;

        // write rclone config file
        var configFile = Encoding.UTF8.GetBytes(sb.ToString()).ToFile(StaticConstants.CloudStorageRCloneFileName);
        await this.fileStorage.Save(this.mapper.CloudStorageModelToCloudStorageDto(cloudStorage), StaticConstants.CloudStorageRCloneFileName, configFile);

        await context.SaveChangesAsync();
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
