# Final Migration Checklist

Use this checklist before switching clients from the MVC app to the .NET 8 API.

## Database

Apply migrations in order against a verified backup of the existing database:

1. `20260701000000_AddRefreshTokens`
2. `20260701001000_AddImageUrlAndLocalPath`
3. `20260703010000_AddAuditLogsAndHardeningIndexes`

Recommended command from `src`:

```powershell
dotnet tool run dotnet-ef database update --project TravelToursWebsite.Infrastructure\TravelToursWebsite.Infrastructure.csproj --startup-project TravelToursWebsite.Api\TravelToursWebsite.Api.csproj
```

Before applying to production:

- [ ] Confirm the target connection string points to the intended database.
- [ ] Take and verify a database backup.
- [ ] Confirm existing tables contain the expected production data.
- [ ] Apply migrations in a staging environment first.
- [ ] Verify refresh-token, image metadata, audit-log, and slug-index changes exist.

## Required Configuration

Do not commit production secrets. Provide these through environment variables, User Secrets, CI/CD secrets, or Key Vault equivalent:

- [ ] `ConnectionStrings__DefaultConnection`
- [ ] `Jwt__Secret` with at least 32 bytes of entropy
- [ ] `Jwt__Issuer`
- [ ] `Jwt__Audience`
- [ ] `EmailSettings__SmtpServer`
- [ ] `EmailSettings__SmtpPort`
- [ ] `EmailSettings__SmtpUsername`
- [ ] `EmailSettings__SmtpPassword`
- [ ] `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc.

Production hardening expectations:

- [ ] HTTPS is enabled at the hosting edge.
- [ ] CORS allowlist contains only approved client origins.
- [ ] JWT development secret is not used outside Development.
- [ ] SQL connection uses encryption/trust settings appropriate for the host.
- [ ] Upload storage under `wwwroot/uploads` is backed up or mapped to durable storage.
- [ ] Log sinks and retention are configured by the hosting environment.

## API Smoke Tests

Run these after deployment:

- [ ] `GET /health` returns success.
- [ ] `GET /api/v1` returns API info with `status: Ready`.
- [ ] `POST /api/v1/auth/login` succeeds for an active admin user.
- [ ] `GET /api/v1/auth/me` succeeds with the bearer token.
- [ ] `GET /api/v1/tours` returns active tours with pagination.
- [ ] `GET /api/v1/blog` returns published posts/events with pagination.
- [ ] `POST /api/v1/contact/inquiries` saves an inquiry even if SMTP is blank/unavailable.
- [ ] `POST /api/v1/contact/bookings` calculates estimated total from tour price and traveler count.
- [ ] `POST /api/v1/media/images` uploads and returns WebP image metadata for a content manager.
- [ ] `GET /api/v1/admin/users` is rejected without a token and succeeds for an admin token.
- [ ] A protected `POST`, `PUT`, `PATCH`, or `DELETE` writes an audit row.
- [ ] `GET /api/v1/admin/audit-logs` returns audit rows for an admin token.

## Behavior Preservation Checks

- [ ] Numeric id and localized slug detail routes work for tours, tour categories, blog posts, and blog categories.
- [ ] Slug lookup tries requested language first and falls back to the default language.
- [ ] Creating a contact inquiry persists before email delivery is attempted.
- [ ] Creating a booking persists before email delivery is attempted.
- [ ] Booking estimated total is `Tour.Price * NumberOfTravelers`.
- [ ] Default language cannot be deleted or disabled.
- [ ] A language with existing translations cannot be deleted.
- [ ] A department with team members cannot be deleted.
- [ ] Tour/blog category deletion is blocked while content references the category.

## Cutover Tasks

- [ ] Freeze MVC writes or plan a final data sync window.
- [ ] Apply database migrations.
- [ ] Deploy API binaries/configuration.
- [ ] Warm up the API and run smoke tests.
- [ ] Point frontend/client applications to the API base URL.
- [ ] Monitor logs, audit logs, 4xx/5xx rate, and database performance.
- [ ] Keep the MVC app available for rollback until acceptance is complete.

## Rollback Notes

- Keep the pre-migration database backup until the API has passed acceptance.
- API database changes are forward-only for normal operation; use the backup for production rollback rather than relying on destructive down migrations.
- Uploaded WebP files and JSON resource edits are filesystem changes and should be included in rollback/backups.
