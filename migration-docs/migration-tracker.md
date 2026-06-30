# Migration Tracker

Last updated: 2026-06-30

## Resume Instructions

Start every new context by reading this file and `migration-docs/migration-plan.md`.

Current stop point: Phase 2 is complete. Review and push the shared API foundation before starting domain migration.

Next phase to start only after Phase 2 is reviewed and pushed: Phase 3 - Domain migration.

## Phase Status

| Phase | Name | Status | Completed On | Notes |
|---:|---|---|---|---|
| 1 | Project setup and migration docs | Done | 2026-06-30 | Created .NET 8 API/Application/Infrastructure/Domain shell, added migration docs inside the new `src` repo, created API-only `TravelToursWebsite.Api.sln` and `TravelToursWebsite.Api.slnx`, added `.gitignore`, and verified build with `dotnet build src\TravelToursWebsite.Api\TravelToursWebsite.Api.csproj --no-restore -m:1`. |
| 2 | Shared API foundation | Done | 2026-06-30 | Added API response model, validation error response shaping, RFC 7807 ProblemDetails customization, global exception handler, Swagger/OpenAPI with API versioning, CORS allowlist config, fixed-window rate limiting, health checks, and versioned `/api/v1` info endpoint. Verified build and user smoke-tested `https://localhost:7157/api/v1`. |
| 3 | Domain migration | Not Started |  | Move/normalize entities/enums into Domain. |
| 4 | Infrastructure EF Core | Not Started |  | Port DbContext and EF configuration to .NET 8. |
| 5 | Application contracts | Not Started |  | Add DTOs, validators, manual mapping, query contracts. |
| 6 | Auth API | Not Started |  | Add JWT auth, policies, refresh token support if needed. |
| 7 | Media service | Not Started |  | Add WebP upload service and image URL/local path persistence. |
| 8 | Public content APIs | Not Started |  | Add public home/tours/blog/content endpoints. |
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