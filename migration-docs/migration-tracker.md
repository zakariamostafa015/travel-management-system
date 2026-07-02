# Migration Tracker

Last updated: 2026-07-02

## Resume Instructions

Start every new context by reading this file and `migration-docs/migration-plan.md`.

Current stop point: Phase 8 is complete. Review and push the public content APIs before starting contact and booking APIs.

Next phase to start only after Phase 8 is reviewed and pushed: Phase 9 - Contact and booking APIs.

## Phase Status

| Phase | Name | Status | Completed On | Notes |
|---:|---|---|---|---|
| 1 | Project setup and migration docs | Done | 2026-06-30 | Created .NET 8 API/Application/Infrastructure/Domain shell, added migration docs inside the new `src` repo, created API-only `TravelToursWebsite.Api.sln` and `TravelToursWebsite.Api.slnx`, added `.gitignore`, and verified build with `dotnet build src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj --no-restore -m:1`. |
| 2 | Shared API foundation | Done | 2026-06-30 | Added API response model, validation error response shaping, RFC 7807 ProblemDetails customization, global exception handler, Swagger/OpenAPI with API versioning, CORS allowlist config, fixed-window rate limiting, health checks, and versioned `/api/v1` info endpoint. Verified build and user smoke-tested `https://localhost:7157/api/v1`. |
| 3 | Domain migration | Done | 2026-06-30 | Added domain entities/enums/configuration models under `TravelToursWebsite.Domain`, including `TeamMember`; preserved legacy model shape and verified no old Core/Data namespace references. |
| 4 | Infrastructure EF Core | Done | 2026-07-01 | Added EF Core 8 SQL Server infrastructure, `ApplicationDbContext`, design-time context factory, baseline database seeder, DI registration, and API configuration wiring without copying legacy production secrets. |
| 5 | Application contracts | Done | 2026-07-01 | Added shared paging/result contracts, feature DTOs, request/query contracts, FluentValidation validators, manual mapping extensions, cancellation-token-aware service interfaces, and Application DI registration. |
| 6 | Auth API | Done | 2026-07-01 | Added JWT bearer authentication, role policies, auth contracts, legacy password-hash compatibility, token service, refresh-token persistence, auth endpoints, Swagger bearer support, safe JWT configuration, and a forward-only refresh-token migration. |
| 7 | Media service | Done | 2026-07-01 | Added WebP image upload service, media contracts, protected media endpoints, static upload serving, ImageSharp 3.1.11 processing, `ImageUrl`/`ImageLocalPath` model fields, and a forward-only image metadata migration. |
| 8 | Public content APIs | Done | 2026-07-02 | Added EF-backed public catalog services, public home/settings contracts, versioned read-only tours/blog/category/home/content endpoints, localized slug fallback, active/published filtering, pagination/search/sorting, and DI wiring. |
| 9 | Contact and booking APIs | Not Started |  | Add inquiry and booking endpoints. |
| 10 | Admin content APIs | Not Started |  | Add admin tours/blog/categories/itineraries/spots/translations/images endpoints. |
| 11 | Admin operations APIs | Not Started |  | Add admin users/languages/departments/team/settings/content endpoints. |
| 12 | Audit and hardening | Not Started |  | Add audit logs, secrets cleanup, logging, performance/index improvements. |
| 13 | Final cleanup | Not Started |  | Final docs and migration checklist. |

## Phase 1 Checklist

- [x] Create `migration-docs` folder.
- [x] Save migration plan.
- [x] Save migration tracker.
- [x] Create .NET 8 API project.
- [x] Create .NET 8 Domain project.
- [x] Create .NET 8 Application project.
- [x] Create .NET 8 Infrastructure project.
- [x] Add project references.
- [x] Add projects to solution.
- [x] Add .gitignore for .NET generated files.
- [x] Verify setup/build.
- [x] Mark Phase 1 as Done.

## Phase 1 Verification

- Command: `dotnet restore src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj`
- Command: `dotnet build src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj --no-restore -m:1`
- Result: build succeeded with 0 warnings and 0 errors.
- Note: normal parallel MSBuild hit generated-file access denied in this Windows sandbox; use `-m:1` if it happens again.

## Phase 2 Checklist

- [x] Add package references for Swagger/OpenAPI and API versioning.
- [x] Add unified API response model.
- [x] Add validation error response shaping.
- [x] Add RFC 7807 `ProblemDetails` customization.
- [x] Add global exception handler.
- [x] Add Swagger/OpenAPI generation for versioned APIs.
- [x] Add API versioning with URL segment and `X-Api-Version` support.
- [x] Add CORS allowlist configuration.
- [x] Add fixed-window rate limiting.
- [x] Add health check endpoint.
- [x] Add versioned API info endpoint.
- [x] Verify setup/build.
- [x] Mark Phase 2 as Done.

## Phase 2 Verification

- Command: `dotnet restore src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj`
- Command: `dotnet build src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj --no-restore -m:1`
- Command while local API process was running: `dotnet build src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj --no-restore -m:1 --output C:\tmp\TravelToursWebsite.Api-phase2-build`
- Result: build succeeded with 0 warnings and 0 errors.
- Note: if the API is running, normal Debug output can be locked by `TravelToursWebsite.Api.exe`; build to a temp output folder or stop the process before rebuilding.
- User smoke test: `GET https://localhost:7157/api/v1` returned 200 with `success: true`, `status: Phase 2 foundation ready`, and `api-supported-versions: 1.0`.

## Phase 3 Checklist

- [x] Add `Entities` folder to Domain.
- [x] Add `Enums` folder to Domain.
- [x] Add `Configuration` folder to Domain.
- [x] Port tour, itinerary, image, spot, and category entities.
- [x] Port blog, blog category, translation, and image entities.
- [x] Port contact inquiry and booking request entities.
- [x] Port user, role, language, settings, department, and team member models.
- [x] Move `TeamMember` out of old `Data.TempModels` shape.
- [x] Verify Domain has no old MVC/Core/Data namespace references.
- [x] Verify solution build.
- [x] Mark Phase 3 as Done.

## Phase 3 Verification

- Command: `rg "TravelToursWebsite\.Core|TravelToursWebsite\.Data|TempModels" TravelToursWebsite.Domain`
- Result: no matches.
- Command: `dotnet build TravelToursWebsite.Api.sln --no-restore -m:1`
- Result: build succeeded with 0 warnings and 0 errors.

## Phase 4 Checklist

- [x] Add EF Core 8 SQL Server package references to Infrastructure.
- [x] Add `ApplicationDbContext` with DbSets for migrated Domain entities.
- [x] Port legacy indexes, unique constraints, delete behavior, and decimal precision.
- [x] Add design-time DbContext factory for EF tooling.
- [x] Add baseline database seeder for languages and non-secret site settings.
- [x] Add Infrastructure DI registration for SQL Server DbContext.
- [x] Wire Infrastructure registration into API startup.
- [x] Add safe connection-string placeholders without copying committed production credentials.
- [x] Verify Infrastructure/Domain have no old Core/Data namespace references.
- [x] Verify no legacy production secrets were copied into `src`.
- [x] Verify build.
- [x] Mark Phase 4 as Done.

## Phase 4 Verification

- Command: `dotnet restore src\TravelToursWebsite.Api.sln`
- Result: restore succeeded.
- Command: `dotnet build src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj --no-restore -m:1 --output C:\tmp\TravelToursWebsite.Api-phase4-build`
- Result: build succeeded with 0 warnings and 0 errors.
- Command: `rg "TravelToursWebsite\.Core|TravelToursWebsite\.Data|TempModels" src\TravelToursWebsite.Infrastructure src\TravelToursWebsite.Domain`
- Result: no matches.
- Command: `rg "db28030|5c\+HoW6|sqdm hjfi|n3gpy70|SmtpPassword\"\s*:\s*\"" src`
- Result: no copied legacy secret values found.

## Phase 5 Checklist

- [x] Add shared paging/query/result contracts.
- [x] Add FluentValidation package and Application DI registration.
- [x] Add public tour DTOs, requests, queries, validators, mapping extensions, and service interfaces.
- [x] Add blog/event DTOs, requests, queries, validators, mapping extensions, and service interfaces.
- [x] Add contact inquiry and booking DTOs, requests, queries, validators, mapping extensions, and service interfaces.
- [x] Add admin operation DTOs, requests, queries, validators, mapping extensions, and service interfaces for users, languages, departments, team members, and site settings.
- [x] Wire Application registration into API startup.
- [x] Verify Application has no EF or old Core/Data namespace references.
- [x] Verify no legacy production secrets were copied into `src`.
- [x] Verify build.
- [x] Mark Phase 5 as Done.

## Phase 5 Verification

- Command: `dotnet restore src\TravelToursWebsite.Api.sln`
- Result: restore succeeded.
- Command: `dotnet build src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj --no-restore -m:1 --output C:\tmp\TravelToursWebsite.Api-phase5-build`
- Result: build succeeded with 0 warnings and 0 errors.
- Command: `rg "TravelToursWebsite\.Core|TravelToursWebsite\.Data|TempModels|Microsoft\.EntityFrameworkCore" src\TravelToursWebsite.Application`
- Result: no matches.
- Command: `rg "db28030|5c\+HoW6|sqdm hjfi|n3gpy70|SmtpPassword\"\s*:\s*\"" src`
- Result: no copied legacy secret values found.
## Phase 6 Checklist

- [x] Add JWT bearer package and configuration.
- [x] Add safe JWT settings with environment-driven production secret.
- [x] Add role authorization policies for admin/content/authoring access.
- [x] Add Auth application contracts, DTOs, validators, and service interfaces.
- [x] Preserve legacy PBKDF2 password-hash compatibility.
- [x] Add JWT access-token generation service.
- [x] Add refresh-token domain entity and EF configuration.
- [x] Add forward-only EF migration for `RefreshTokens`.
- [x] Add login, refresh, revoke, and current-user auth service behavior.
- [x] Add versioned `/api/v1/auth` endpoints.
- [x] Add Swagger bearer-token security definition.
- [x] Verify no old Core/Data namespace references were introduced.
- [x] Verify no legacy production secrets were copied into `src`.
- [x] Verify build.
- [x] Mark Phase 6 as Done.

## Phase 6 Verification

- Command: `dotnet restore src\TravelToursWebsite.Api.sln`
- Result: restore succeeded.
- Command: `dotnet build src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj --no-restore -m:1 --output C:\tmp\TravelToursWebsite.Api-phase6-build`
- Result: build succeeded with 0 warnings and 0 errors.
- Command: `rg "TravelToursWebsite\.Core|TravelToursWebsite\.Data|TempModels" src\TravelToursWebsite.Application src\TravelToursWebsite.Infrastructure src\TravelToursWebsite.Api src\TravelToursWebsite.Domain`
- Result: no matches.
- Command: `rg "db28030|5c\+HoW6|sqdm hjfi|n3gpy70|SmtpPassword\"\s*:\s*\"" src`
- Result: no copied legacy secret values found.
- Note: apply the `AddRefreshTokens` migration before using refresh-token endpoints against an existing database.
## Phase 7 Checklist

- [x] Add media upload contracts and options to Application.
- [x] Add WebP image storage service in Infrastructure.
- [x] Validate allowed extensions/content types for jpg, jpeg, png, and webp.
- [x] Validate image content by decoding with ImageSharp.
- [x] Strip EXIF metadata and resize oversized images.
- [x] Convert uploads to WebP.
- [x] Generate thumbnail and medium WebP variants.
- [x] Return `ImageUrl` and `ImageLocalPath` in media upload result.
- [x] Add `ImageUrl` and `ImageLocalPath` fields to tour/blog image models and DTOs.
- [x] Add EF configuration and migration for image metadata fields.
- [x] Add protected versioned media upload/delete endpoints.
- [x] Serve uploaded files from API `wwwroot/uploads`.
- [x] Avoid vulnerable/licensed ImageSharp 4.0 default by pinning ImageSharp 3.1.11.
- [x] Verify no old Core/Data namespace references were introduced.
- [x] Verify no legacy production secrets were copied into `src`.
- [x] Verify build.
- [x] Mark Phase 7 as Done.

## Phase 7 Verification

- Command: `dotnet restore src\TravelToursWebsite.Api.sln`
- Result: restore succeeded with no vulnerability warnings.
- Command: `dotnet build src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj --no-restore -m:1 --output C:\tmp\TravelToursWebsite.Api-phase7-build`
- Result: build succeeded with 0 warnings and 0 errors.
- Command: `rg "TravelToursWebsite\.Core|TravelToursWebsite\.Data|TempModels" src\TravelToursWebsite.Application src\TravelToursWebsite.Infrastructure src\TravelToursWebsite.Api src\TravelToursWebsite.Domain`
- Result: no matches.
- Command: `rg "db28030|5c\+HoW6|sqdm hjfi|n3gpy70|SmtpPassword\"\s*:\s*\"" src`
- Result: no copied legacy secret values found.
- Note: apply the `AddImageUrlAndLocalPath` migration before relying on `ImageUrl`/`ImageLocalPath` persistence in an existing database.
## Phase 8 Checklist

- [x] Add public content contracts for home and site settings.
- [x] Implement `ITourCatalogService` with active tour/category reads.
- [x] Implement `IBlogCatalogService` with published blog/event/category reads.
- [x] Support numeric ID and localized slug lookups with default-language fallback.
- [x] Add search, filtering, sorting, bounded pagination, `AsNoTracking`, and cancellation-token-aware reads.
- [x] Add public home summary and active site settings reads.
- [x] Add versioned `/api/v1/tours`, `/api/v1/blog`, `/api/v1/home`, and `/api/v1/content/settings` endpoints.
- [x] Wire catalog/public content services into Infrastructure DI.
- [x] Verify no old Core/Data namespace references were introduced.
- [x] Verify no legacy production secrets were copied into `src`.
- [x] Verify build.
- [x] Mark Phase 8 as Done.

## Phase 8 Verification

- Command: `dotnet build TravelToursWebsite.Api.sln --no-restore -m:1`
- Result: build succeeded with 0 warnings and 0 errors.
- Command: `rg "TravelToursWebsite\.Core|TravelToursWebsite\.Data|TempModels" TravelToursWebsite.Application TravelToursWebsite.Infrastructure TravelToursWebsite.Api TravelToursWebsite.Domain`
- Result: no matches.
- Command: `rg "db28030|5c\+HoW6|sqdm hjfi|n3gpy70|SmtpPassword\"\s*:\s*\"" .`
- Result: no copied legacy secret values found.
