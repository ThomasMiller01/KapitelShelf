// <copyright file="OneDriveLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text;
using System.Text.Json;
using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic.CloudStorages;

/// <summary>
/// The OneDrive cloud storage logic.
/// </summary>
public class OneDriveLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper, KapitelShelfSettings settings)
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;
#pragma warning restore IDE0052 // Remove unread private members

    private readonly KapitelShelfSettings settings = settings;

    /// <summary>
    /// Get the url for the OAuth flow of OneDrive.
    /// </summary>
    /// <param name="redirectUrl">The url to redirect to after the OAuth flow finished.</param>
    /// <returns>The OAuth url.</returns>
    public string GetOAuthUrl(string redirectUrl)
    {
        var stateInfo = new OAuthStateInfoDTO
        {
            RedirectUrl = redirectUrl,
        };
        var stateInfoJson = JsonSerializer.Serialize(stateInfo);
        var stateInfoBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(stateInfoJson));

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = StaticConstants.CloudStorageOneDriveRCloneClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = $"{this.settings.Domain}/cloudstorage/onedrive/oauth/callback",
            ["response_mode"] = "query",
            ["scope"] = "offline_access Files.ReadWrite.AppFolder",
            ["state"] = stateInfoBase64,
        };
        var queryString = string.Join("&", queryParams.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
        return "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?" + queryString;
    }

    /// <summary>
    /// Gets the OAuth token from the OAuth code.
    /// </summary>
    /// <param name="code">The OAuth code.</param>
    /// <returns>The OAuth tokens.</returns>
    public async Task<OAuthTokensDTO> GetOAuthTokensFromCode(string code)
    {
        using var httpClient = new HttpClient();
        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", StaticConstants.CloudStorageOneDriveRCloneClientId),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", $"{this.settings.Domain}/cloudstorage/onedrive/oauth/callback"),
        ]);

        var response = await httpClient.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", content);
        var responseString = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(responseString);
        var root = json.RootElement;

        return new OAuthTokensDTO
        {
            AccessToken = root.GetProperty("access_token").GetString() ?? string.Empty,
            RefreshToken = root.GetProperty("refresh_token").GetString() ?? string.Empty,
            ExpiresIn = root.GetProperty("expires_in").GetInt32(),
        };
    }

    /// <summary>
    /// Generate the rclone config.
    /// </summary>
    /// <param name="tokens">The OAuth tokens.</param>
    /// <returns>A task.</returns>
    public async Task GenerateRCloneConfig(OAuthTokensDTO tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        Console.WriteLine(tokens.AccessToken);
        await Task.CompletedTask;
    }
}
