using WorkflowHub.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentation()
    .AddApplicationModules(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.ApplyDevelopmentMigrations();
app.UsePresentationPipeline();

app.Run();
