// <copyright file="Program.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Reflection;
using System.Text;
using System.Text.Json;
using KapitelShelf.Api;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.CloudStorages;
using KapitelShelf.Api.Logic.Interfaces.Storage;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Settings;
using KapitelShelf.Api.Tasks;
using KapitelShelf.Api.Utils;
using KapitelShelf.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl.AdoJobStore;

var builder = WebApplication.CreateBuilder(args);

// configuration settings
var settings = builder.Configuration.GetSection("KapitelShelf").Get<KapitelShelfSettings>()!;
#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
Console.WriteLine(JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances

#pragma warning disable IDE0001 // Simplify Names
builder.Services.AddSingleton<KapitelShelfSettings>(settings);
#pragma warning restore IDE0001 // Simplify Names

// register support for System.Text.Encoding.GetEncoding("Windows-1252"), etc.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Add services to the container.
builder.Services.AddControllers();

// swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    // Include XML comments in Swagger
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// cors
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: "CorsPolicy",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// increase file size upload limit
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue;
});

// database
var databaseConnection = $"Host={settings.Database.Host};Database={StaticConstants.DatabaseName};Username={settings.Database.Username};Password={settings.Database.Password}";
builder.Services.AddDbContextFactory<KapitelShelfDBContext>(options => options.UseNpgsql(databaseConnection));

// automapper
builder.Services.AddAutoMapper(typeof(Program));

// logic
builder.Services.AddSingleton<IBooksLogic, BooksLogic>();
builder.Services.AddSingleton<SeriesLogic>();
builder.Services.AddSingleton<IBookParserManager, BookParserManager>();
builder.Services.AddSingleton<MetadataLogic>();
builder.Services.AddSingleton<IMetadataScraperManager, MetadataScraperManager>();
builder.Services.AddSingleton<UsersLogic>();
builder.Services.AddSingleton<TasksLogic>();
builder.Services.AddSingleton<SettingsLogic>();

builder.Services.AddSingleton<IBookStorage, BookStorage>();
builder.Services.AddSingleton<ICloudStorage, CloudStorage>();

builder.Services.AddSingleton<ICloudStoragesLogic, CloudStoragesLogic>();
builder.Services.AddSingleton<OneDriveLogic>();

builder.Services.AddSingleton<IProcessUtils, ProcessUtils>();
builder.Services.AddSingleton<IDynamicSettingsManager, DynamicSettingsManager>();

// background tasks using Quartz.NET
builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));
builder.Services.AddQuartz(q =>
{
    q.MaxBatchSize = 10;

    q.UsePersistentStore(c =>
    {
        c.UsePostgres(postgresOptions =>
        {
            postgresOptions.UseDriverDelegate<PostgreSQLDelegate>();
            postgresOptions.ConnectionString = $"{databaseConnection};SearchPath=quartz";
        });
    });
});
builder.Services.AddQuartzHostedService(options =>
{
    options.AwaitApplicationStarted = true;

    // let jobs finish gracefully
    options.WaitForJobsToComplete = true;
});
builder.Services.AddSingleton<ITaskRuntimeDataStore, TaskRuntimeDataStore>();
builder.Services.AddHostedService<StartupTasksHostedService>();

// healthchecks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<KapitelShelfDBContext>("CheckDatabase");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// healthchecks
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // Only checks app is running (liveness)
    Predicate = _ => false,
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Checks all registered health checks (readiness)
    Predicate = _ => true,
});
app.MapHealthChecks("/health/startup", new HealthCheckOptions
{
    // Checks all registered health checks (startup, same as readiness)
    Predicate = _ => true,
});

app.Run();
