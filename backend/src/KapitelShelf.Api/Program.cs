// <copyright file="Program.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Reflection;
using KapitelShelf.Api.Logic;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
            policy.WithOrigins(
                    "http://localhost:5261", // swagger
                    "http://localhost:5173") // frontend
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// database
builder.Services.AddDbContextFactory<KapitelShelfDBContext>(options =>
    options.UseNpgsql("Host=localhost;Database=kapitelshelf;Username=kapitelshelf;Password=kapitelshelf"));

// automapper
builder.Services.AddAutoMapper(typeof(Program));

// logic
builder.Services.AddSingleton<BooksLogic>();
builder.Services.AddSingleton<SeriesLogic>();
builder.Services.AddSingleton<DemoDataLogic>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("CorsPolicy");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
