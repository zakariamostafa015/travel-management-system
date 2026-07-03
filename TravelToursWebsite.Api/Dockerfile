FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TravelToursWebsite.Api.sln ./
COPY TravelToursWebsite.Api/TravelToursWebsite.Api.csproj TravelToursWebsite.Api/
COPY TravelToursWebsite.Application/TravelToursWebsite.Application.csproj TravelToursWebsite.Application/
COPY TravelToursWebsite.Domain/TravelToursWebsite.Domain.csproj TravelToursWebsite.Domain/
COPY TravelToursWebsite.Infrastructure/TravelToursWebsite.Infrastructure.csproj TravelToursWebsite.Infrastructure/
RUN dotnet restore TravelToursWebsite.Api/TravelToursWebsite.Api.csproj

COPY . .
RUN dotnet publish TravelToursWebsite.Api/TravelToursWebsite.Api.csproj -c Release -o /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TravelToursWebsite.Api.dll"]
