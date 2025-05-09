// <copyright file="Program.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Reflection;
using System.Text;
using System.Text.Json;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// configuration settings
var settings = builder.Configuration.GetSection("KapitelShelf").Get<KapitelShelfSettings>()!;
#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
Console.WriteLine(JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
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
builder.Services.AddDbContextFactory<KapitelShelfDBContext>(options =>
    options.UseNpgsql($"Host={settings.Database.Host};Database={StaticConstants.DatabaseName};Username={settings.Database.Username};Password={settings.Database.Password}"));

// automapper
builder.Services.AddAutoMapper(typeof(Program));

// logic
builder.Services.AddSingleton<BooksLogic>();
builder.Services.AddSingleton<SeriesLogic>();
builder.Services.AddSingleton<DemoDataLogic>();
builder.Services.AddSingleton<BookStorage>();
builder.Services.AddSingleton<BookParserManager>();

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

app.Run();
