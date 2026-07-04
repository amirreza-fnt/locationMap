using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LocationMap.API.Data;
using LocationMap.API.Middleware;
using LocationMap.API.Models;
using LocationMap.API.Models.Enums;
using LocationMap.API.Repositories;
using LocationMap.API.Repositories.Interfaces;
using LocationMap.API.Services;
using LocationMap.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

#region 1. Controllers & API

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

#endregion

#region 2. Swagger

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LocationMap API",
        Version = "v1",
        Description = "API سرویس نقاط روی نقشه سبزوار",
        Contact = new OpenApiContact
        {
            Name = "LocationMap Team",
            Email = "support@locationsmap.sabzevar.ir"
        }
    });
});

#endregion

#region 3. Database

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=.;Database=LocationMapDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

#endregion

#region 4. Caching

builder.Services.AddMemoryCache();

#endregion

#region 5. CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? new[] { "http://localhost:3000", "https://map.sabzevar.ir" };

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
    }
});

#endregion

#region 6. Response Compression

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/json" });
});

#endregion

#region 7. Dependency Injection

builder.Services.AddScoped<IMapPointRepository, MapPointRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IMapUserRepository, MapUserRepository>();
builder.Services.AddScoped<IMapPointService, MapPointService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

#endregion

#region 8. Health Checks

builder.Services.AddHealthChecks();

#endregion

var app = builder.Build();

#region Middleware Pipeline

app.UseResponseCompression();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LocationMap API v1");
        options.RoutePrefix = "swagger";
    });
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowFrontend");
}

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

#endregion

#region Database Initialization

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var created = context.Database.EnsureCreated();
        if (created)
        {
            logger.LogInformation("Database created successfully!");
            await SeedDataAsync(context, logger);
        }
        logger.LogInformation("Database ready");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization error");
    }
}

#endregion

app.Logger.LogInformation("══════════════════════════════════════");
app.Logger.LogInformation("LocationMap API Started");
app.Logger.LogInformation("Environment: {Env}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Swagger: /swagger");
app.Logger.LogInformation("══════════════════════════════════════");

app.Run();

static async Task SeedDataAsync(AppDbContext context, ILogger logger)
{
    if (await context.Categories.AnyAsync()) return;

    var categories = new List<Category>
    {
        new() { Name = "کوچه‌ها", Icon = "road", Color = "#FF5733", SortOrder = 1 },
        new() { Name = "پارک‌ها", Icon = "tree", Color = "#33FF57", SortOrder = 2 },
        new() { Name = "مراکز خرید", Icon = "shopping-cart", Color = "#3357FF", SortOrder = 3 },
        new() { Name = "مساجد", Icon = "mosque", Color = "#FF33F5", SortOrder = 4 },
        new() { Name = "بیمارستان‌ها", Icon = "hospital", Color = "#FF3333", SortOrder = 5 },
        new() { Name = "مدارس", Icon = "school", Color = "#33FFF5", SortOrder = 6 },
        new() { Name = "اماکن ورزشی", Icon = "sports", Color = "#F5FF33", SortOrder = 7 },
        new() { Name = "سایر", Icon = "map-pin", Color = "#808080", SortOrder = 8 }
    };

    context.Categories.AddRange(categories);

    if (!await context.MapUsers.AnyAsync())
    {
        var admin = new MapUser
        {
            FullName = "مدیر سیستم",
            MelliCode = "0000000000",
            AccessLevel = AccessLevel.Admin,
            Phone = "09150000000"
        };
        context.MapUsers.Add(admin);
    }

    await context.SaveChangesAsync();
    logger.LogInformation("Seed data inserted: {Count} categories", categories.Count);
}

public partial class Program { }
