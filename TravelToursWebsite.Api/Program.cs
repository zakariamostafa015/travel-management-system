using TravelToursWebsite.Api.Extensions;
using TravelToursWebsite.Application.DependencyInjection;
using TravelToursWebsite.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiFoundation(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();

app.UseApiFoundation();
app.MapApiFoundationEndpoints();
app.Run();