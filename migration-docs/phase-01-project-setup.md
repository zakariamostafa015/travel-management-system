# Phase 1 - Project Setup

## Goal

Create the .NET 8 API architecture shell without migrating business logic yet.

## Expected Output

- `src/TravelToursWebsite.Api`
- `src/TravelToursWebsite.Application`
- `src/TravelToursWebsite.Domain`
- `src/TravelToursWebsite.Infrastructure`
- Persistent migration docs in `migration-docs`
- Solution references for all new projects

## Validation Checklist

- New projects target `net8.0`.
- Dependency direction is:
  - Api -> Application, Infrastructure
  - Infrastructure -> Application
  - Application -> Domain
  - Domain -> no project references
- The solution includes the new projects.
- No business logic has been migrated yet.

## Suggested Git Metadata

- Branch: `codex/phase-01-api-setup`
- PR title: `Phase 1: Add .NET 8 API solution structure`
- Commit: `chore: add api clean architecture projects`

