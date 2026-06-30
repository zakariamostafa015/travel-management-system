using TravelToursWebsite.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiFoundation(builder.Configuration);

var app = builder.Build();

app.UseApiFoundation();
app.MapApiFoundationEndpoints();
app.Run();
