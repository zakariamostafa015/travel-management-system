# Business Logic Modules

This file tracks feature modules to migrate into the new API.

## Modules

- Tours
- Tour categories
- Tour images/media
- Tour itineraries
- Tour translations
- Tour spots/map points
- Blog posts and events
- Blog categories
- Blog images/media
- Contact inquiries
- Booking and quote requests
- Users and roles
- Authentication and authorization
- Languages
- Departments
- Team members
- Site settings
- JSON resource content management
- Email notifications
- Media upload and image processing
- Audit logging
- Shared pagination/filtering/sorting/search

## Enhancement Candidates

- Media upload service: accept jpg/jpeg/png/webp, convert to WebP, store `ImageUrl` and `ImageLocalPath`.
- Query services: add pagination, projection, `AsNoTracking`, filtering, sorting, and search.
- Auth: replace cookie auth with JWT and policies.
- Settings/content: remove service locator usage and centralize cache invalidation.
- Email: move to queued/background delivery later if the API receives high traffic.
- Security: remove committed secrets and enable encrypted SQL connections.

