# Phase 3 - Domain Migration

## Goal

Move the existing MVC business entities and enums into the new .NET 8 Domain project without adding EF Core, API, DTO, or validation service behavior yet.

## Completed

- Added domain entities under `TravelToursWebsite.Domain/Entities`.
- Added enums under `TravelToursWebsite.Domain/Enums`.
- Moved `TeamMember` out of the old `Data.TempModels` concept into the Domain layer.
- Added `EmailSettings` under `TravelToursWebsite.Domain/Configuration` as a configuration model.
- Preserved existing property names, defaults, navigation properties, status enums, and DataAnnotations used by the legacy model.
- Kept the Domain project free of references to the old MVC/Core/Data projects.

## Validation

- `rg "TravelToursWebsite\.Core|TravelToursWebsite\.Data|TempModels" TravelToursWebsite.Domain` found no old namespace references.
- `dotnet build TravelToursWebsite.Api.sln --no-restore -m:1` succeeded with 0 warnings and 0 errors.

## Next Phase

Phase 4 - Infrastructure EF Core: port the DbContext and entity configuration to .NET 8 using these Domain entities.
