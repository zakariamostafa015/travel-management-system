# ASP.NET Core MVC to .NET 8 Web API Migration Plan

## Summary

Migrate the existing .NET 6 MVC application into a .NET 8 ASP.NET Core Web API using a pragmatic Clean/Onion architecture:

- `TravelToursWebsite.Domain`: entities, enums, domain rules, dependency-free abstractions.
- `TravelToursWebsite.Application`: use cases, DTOs, validators, manual mapping extensions, pagination/search/filter contracts.
- `TravelToursWebsite.Infrastructure`: EF Core SQL Server, file storage, image processing, email, caching, audit persistence.
- `TravelToursWebsite.Api`: controllers, auth, middleware, Swagger, versioning, CORS, rate limiting, health checks.

The existing SQL Server schema should be reused first to preserve data. Add forward-only migrations later for audit logging, refresh tokens, and enhanced media metadata.

## Current Findings

- Current projects: `Core`, `Data`, and `Web`.
- Main features: tours, tour categories, itineraries, tour spots, multilingual translations, blog/events, blog categories, contact inquiries, booking requests, users/admin, languages, departments, team members, site settings, and JSON resource content management.
- Current MVC controllers mix HTTP concerns, workflow orchestration, entity mapping, image upload handling, settings loading, and error handling.
- `AdminController` is very large and should be split into feature-oriented API controllers/services.
- Current services return EF entities directly and usually lack `AsNoTracking`, projections, cancellation tokens, pagination, and consistent filtering/sorting.
- Authentication is cookie-based; target API should use JWT and policies.
- Upload logic currently accepts several extensions and converts to JPEG. Target behavior is jpg/jpeg/png/webp input, validate content, convert to WebP, save to server, and store `ImageUrl` plus `ImageLocalPath`.
- Sensitive values are committed in `appsettings.json`; move secrets to environment variables/User Secrets/Key Vault before production.
- No scheduled jobs or background services were found.

## Target Standards

- Request DTO, response DTO, FluentValidation, manual mapping extension methods.
- Unified API response and RFC 7807 `ProblemDetails`.
- Proper HTTP status codes, XML docs, Swagger examples where useful.
- `CancellationToken` support everywhere.
- Pagination, filtering, sorting, searching on list endpoints.
- `AsNoTracking`, projection, and bounded page sizes for reads.
- JWT authentication, refresh tokens if needed, role/policy authorization.
- CORS allowlist, HTTPS, rate limiting, health checks, logging, audit logging.
- No AutoMapper in the new API.

## Phase Roadmap

| Order | Phase | Status Target | Branch | PR Title |
|---:|---|---|---|---|
| 1 | Project setup and migration docs | Create .NET 8 API architecture shell and persistent docs | `codex/phase-01-api-setup` | `Phase 1: Add .NET 8 API solution structure` |
| 2 | Shared API foundation | Response model, ProblemDetails, exception middleware, logging, Swagger, versioning, CORS, rate limiting, health checks | `codex/phase-02-api-foundation` | `Phase 2: Add API foundation and cross-cutting middleware` |
| 3 | Domain migration | Move/normalize entities/enums into Domain, including `TeamMember` | `codex/phase-03-domain-model` | `Phase 3: Migrate domain model` |
| 4 | Infrastructure EF Core | Port DbContext/configuration/seeding to EF Core 8 | `codex/phase-04-ef-infrastructure` | `Phase 4: Port database infrastructure` |
| 5 | Application contracts | DTOs, validators, mapping extensions, query contracts, service interfaces | `codex/phase-05-application-contracts` | `Phase 5: Add application DTOs and validators` |
| 6 | Auth API | JWT auth, policies, refresh tokens if needed, password compatibility | `codex/phase-06-jwt-auth` | `Phase 6: Add JWT authentication and authorization` |
| 7 | Media service | WebP upload/conversion service with `ImageUrl` and `ImageLocalPath` persistence | `codex/phase-07-media-service` | `Phase 7: Add WebP media upload service` |
| 8 | Public content APIs | Home, tours, categories, blog/events, search, details by id or slug | `codex/phase-08-public-content` | `Phase 8: Add public content APIs` |
| 9 | Contact and booking APIs | Inquiry and booking/quote submission while preserving email behavior | `codex/phase-09-contact-booking` | `Phase 9: Add contact and booking APIs` |
| 10 | Admin content APIs | Tours, blog, categories, itineraries, spots, translations, images | `codex/phase-10-admin-content` | `Phase 10: Add admin content management APIs` |
| 11 | Admin operations APIs | Users, languages, departments, team members, settings, resource content | `codex/phase-11-admin-operations` | `Phase 11: Add admin operations APIs` |
| 12 | Audit and hardening | Audit logging, structured logs, index migration, secret cleanup, performance review | `codex/phase-12-hardening` | `Phase 12: Add audit logging and hardening` |
| 13 | Final cleanup | Endpoint docs, migration checklist, remove API-scope dead code | `codex/phase-13-migration-cleanup` | `Phase 13: Finalize API migration cleanup` |

## Behavior To Preserve

- Public details/category routes support numeric IDs and localized slugs.
- Slug lookup should try requested language first, then default language.
- Contact inquiry and booking request creation should still save even when email delivery fails.
- Booking estimated total is `Tour.Price * NumberOfTravelers`.
- Creating tours/blog posts creates a default-language placeholder translation.
- Language rules: cannot delete/disable the default language; cannot delete a language with translations.
- Department delete should fail while team members reference it.

## Assumptions

- The existing database contains important data and should not be recreated unless the user explicitly approves it.
- New files should live beside the existing MVC app during migration.
- The old MVC app remains untouched unless a phase explicitly says otherwise.
- The API will initially serve uploaded files from server storage, not cloud object storage.
- Existing legacy media paths remain readable while new uploads use the enhanced WebP media model.

