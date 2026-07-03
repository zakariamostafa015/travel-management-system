using TravelToursWebsite.Api.Extensions;
using TravelToursWebsite.Application.DependencyInjection;
using TravelToursWebsite.Application.Features.Media;
using TravelToursWebsite.Domain.Configuration;
using TravelToursWebsite.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiFoundation(builder.Configuration);
builder.Services.AddApplication();
builder.Services.Configure<MediaOptions>(builder.Configuration.GetSection(MediaOptions.SectionName));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseApiFoundation();
app.MapApiFoundationEndpoints();
app.Run();