using WorkflowHub.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentation()
    .AddApplicationModules(builder.Configuration)
    .AddApiServices();

var app = builder.Build();

app.UsePresentationPipeline();

app.Run();
