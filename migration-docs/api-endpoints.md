# TravelToursWebsite API Endpoint Reference

Base path: `/api/v1`

All responses use the shared `ApiResponse` envelope unless an RFC 7807 `ProblemDetails` response is returned by middleware.

## Public and Shared

| Method | Path | Auth | Purpose |
|---|---|---|---|
| GET | `/` | Anonymous | API service/version info. |
| GET | `/health` | Anonymous | Health check endpoint. |

## Authentication

| Method | Path | Auth | Purpose |
|---|---|---|---|
| POST | `/auth/login` | Anonymous | Issue JWT access token and refresh token. |
| POST | `/auth/refresh` | Anonymous | Rotate refresh token and issue a new access token. |
| POST | `/auth/revoke` | Bearer token | Revoke a refresh token. |
| GET | `/auth/me` | Bearer token | Return the current authenticated user. |

## Public Content

| Method | Path | Auth | Purpose |
|---|---|---|---|
| GET | `/home` | Anonymous | Home-page summary content. |
| GET | `/content/settings` | Anonymous | Active public site settings. |
| GET | `/content/settings/{key}` | Anonymous | Public site setting by key. |
| GET | `/tours` | Anonymous | Search/filter/page active tours. |
| GET | `/tours/{idOrSlug}` | Anonymous | Tour details by numeric id or localized slug. |
| GET | `/tours/categories` | Anonymous | Search/filter/page active tour categories. |
| GET | `/tours/categories/{idOrSlug}` | Anonymous | Tour category details by numeric id or localized slug. |
| GET | `/blog` | Anonymous | Search/filter/page published blog posts/events. |
| GET | `/blog/{idOrSlug}` | Anonymous | Blog details by numeric id or localized slug. |
| GET | `/blog/categories` | Anonymous | Search/filter/page active blog categories. |
| GET | `/blog/categories/{idOrSlug}` | Anonymous | Blog category details by numeric id or localized slug. |

## Contact and Booking

| Method | Path | Auth | Purpose |
|---|---|---|---|
| POST | `/contact/inquiries` | Anonymous | Submit a contact inquiry. |
| POST | `/contact/bookings` | Anonymous | Submit a booking or quote request. |
| GET | `/contact/inquiries` | AdminOnly | Page/filter contact inquiries. |
| GET | `/contact/inquiries/{id}` | AdminOnly | Contact inquiry details. |
| PATCH | `/contact/inquiries/{id}/status` | AdminOnly | Update inquiry status/admin notes. |
| DELETE | `/contact/inquiries/{id}` | AdminOnly | Delete inquiry. |
| GET | `/contact/bookings` | AdminOnly | Page/filter booking requests. |
| GET | `/contact/bookings/{id}` | AdminOnly | Booking request details. |
| PATCH | `/contact/bookings/{id}/status` | AdminOnly | Update booking status/admin notes. |
| DELETE | `/contact/bookings/{id}` | AdminOnly | Delete booking request. |

## Media

| Method | Path | Auth | Purpose |
|---|---|---|---|
| POST | `/media/images` | ContentManager | Upload jpg/jpeg/png/webp, convert to WebP variants, return URLs/paths. |
| DELETE | `/media/images` | ContentManager | Delete an uploaded media file by path. |

## Admin Tours

Policy: `ContentManager`.

| Method | Path | Purpose |
|---|---|---|
| POST | `/admin/tours` | Create tour. |
| PUT | `/admin/tours/{id}` | Update tour. |
| DELETE | `/admin/tours/{id}` | Delete tour. |
| POST | `/admin/tours/categories` | Create tour category. |
| PUT | `/admin/tours/categories/{id}` | Update tour category. |
| DELETE | `/admin/tours/categories/{id}` | Delete tour category if not referenced. |
| POST | `/admin/tours/images` | Add tour image association. |
| PUT | `/admin/tours/images/{id}` | Update tour image metadata. |
| DELETE | `/admin/tours/images/{id}` | Delete tour image association. |
| POST | `/admin/tours/itineraries` | Create or update itinerary. |
| DELETE | `/admin/tours/itineraries/{id}` | Delete itinerary. |
| POST | `/admin/tours/spots` | Create or update map spot. |
| DELETE | `/admin/tours/spots/{id}` | Delete map spot. |
| GET | `/admin/tours/{id}/translations` | List tour translations. |
| PUT | `/admin/tours/{id}/translations` | Upsert tour translation. |
| GET | `/admin/tours/categories/{id}/translations` | List tour category translations. |
| PUT | `/admin/tours/categories/{id}/translations` | Upsert tour category translation. |
| GET | `/admin/tours/itineraries/{id}/translations` | List itinerary translations. |
| PUT | `/admin/tours/itineraries/{id}/translations` | Upsert itinerary translation. |

## Admin Blog

Policy: `ContentManager`.

| Method | Path | Purpose |
|---|---|---|
| POST | `/admin/blog` | Create blog post/event. |
| PUT | `/admin/blog/{id}` | Update blog post/event. |
| DELETE | `/admin/blog/{id}` | Delete blog post/event. |
| POST | `/admin/blog/categories` | Create blog category. |
| PUT | `/admin/blog/categories/{id}` | Update blog category. |
| DELETE | `/admin/blog/categories/{id}` | Delete blog category if not referenced. |
| POST | `/admin/blog/images` | Add blog image association. |
| PUT | `/admin/blog/images/{id}` | Update blog image metadata. |
| DELETE | `/admin/blog/images/{id}` | Delete blog image association. |
| GET | `/admin/blog/{id}/translations` | List blog post translations. |
| PUT | `/admin/blog/{id}/translations` | Upsert blog post translation. |
| GET | `/admin/blog/categories/{id}/translations` | List blog category translations. |
| PUT | `/admin/blog/categories/{id}/translations` | Upsert blog category translation. |
| POST | `/admin/blog/{id}/view-count` | Increment view count. |

## Admin Operations

Policy: `AdminOnly`.

| Method | Path | Purpose |
|---|---|---|
| GET | `/admin/users` | Page/filter users. |
| GET | `/admin/users/{id}` | User details by id. |
| GET | `/admin/users/by-username/{username}` | User details by username. |
| POST | `/admin/users` | Create user. |
| PUT | `/admin/users/{id}` | Update user. |
| PATCH | `/admin/users/{id}/password` | Change password with current password. |
| PATCH | `/admin/users/{id}/password/reset` | Admin reset password. |
| DELETE | `/admin/users/{id}` | Soft-delete/deactivate user. |
| PATCH | `/admin/users/{id}/reactivate` | Reactivate user. |
| GET | `/admin/languages` | Page/filter languages. |
| GET | `/admin/languages/active-codes` | Active language codes. |
| GET | `/admin/languages/default` | Default active language. |
| GET | `/admin/languages/{id}` | Language details. |
| PUT | `/admin/languages` | Create or update language. |
| DELETE | `/admin/languages/{id}` | Delete language with safeguards. |
| PATCH | `/admin/languages/{id}/default` | Set default language. |
| PATCH | `/admin/languages/{id}/toggle-status` | Enable/disable language with safeguards. |
| GET | `/admin/departments` | Page/filter departments. |
| PUT | `/admin/departments` | Create or update department. |
| DELETE | `/admin/departments/{id}` | Delete department if no team members reference it. |
| GET | `/admin/team-members` | Page/filter team members. |
| PUT | `/admin/team-members` | Create or update team member. |
| DELETE | `/admin/team-members/{id}` | Delete team member. |
| GET | `/admin/settings` | Page/filter site settings. |
| PUT | `/admin/settings` | Create or update site setting. |
| DELETE | `/admin/settings/{id}` | Delete site setting. |
| GET | `/admin/resources/languages` | List JSON resource languages. |
| GET | `/admin/resources/{cultureCode}` | Read resource file content. |
| GET | `/admin/resources/items/{key}` | Read content item across languages. |
| PUT | `/admin/resources/items` | Upsert content item across languages. |
| DELETE | `/admin/resources/items/{key}` | Delete content item across languages. |
| GET | `/admin/resources/{cultureCode}/validate` | Validate resource JSON file. |
| GET | `/admin/audit-logs` | Page/filter audit logs. |
| GET | `/admin/audit-logs/{id}` | Audit log details. |

## Authorization Policies

- `AdminOnly`: `Admin` role.
- `ContentManager`: `Admin` or `Editor` role.
- `Authoring`: `Admin`, `Editor`, or `Author` role.

## Common Query Parameters

Most list endpoints use the shared paging contract:

- `pageNumber`: defaults to `1`.
- `pageSize`: defaults to `20`, capped at `100`.
- `searchTerm`: endpoint-specific text search.
- `sortBy`: endpoint-specific sort key.
- `sortDirection`: `Ascending` or `Descending`.
