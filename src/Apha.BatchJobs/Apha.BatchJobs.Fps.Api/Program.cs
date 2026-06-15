using Amazon.EventBridge;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Fps.Api.Options;
using Apha.BatchJobs.Fps.Api.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BatchJobs FPS API",
        Version = "v1",
        Description = "Batch jobs trigger API for FPS routes"
    });
});
builder.Services.Configure<EventPublisherOptions>(builder.Configuration.GetSection("EventBridge"));
builder.Services.AddAWSService<IAmazonEventBridge>();
builder.Services.AddScoped<IEventPublisher, EventBridgePublisher>();
// Register DB context and job execution repository for Initiated record creation
var connectionString = builder.Configuration.GetConnectionString("FPSConnectionString");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<BatchJobsDbContext>(options =>
        options.UseNpgsql(connectionString));
    builder.Services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();
}
var fpsEventPublisherOptions = builder.Configuration.GetSection("EventBridge").Get<EventPublisherOptions>()
    ?? new EventPublisherOptions();

if (builder.Environment.IsProduction() && fpsEventPublisherOptions.DryRun)
{
    throw new InvalidOperationException(
        "EventBridge:DryRun must be false in Production for Apha.BatchJobs.Fps.Api.");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BatchJobs FPS API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "fps.api", timestamp = DateTime.UtcNow }));

app.Run();
