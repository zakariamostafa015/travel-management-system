# TravelToursWebsite API Migration Docs

This folder is the persistent source of truth for the MVC-to-Web-API migration.

When resuming in a new Codex context:

1. Read `migration-docs/migration-plan.md`.
2. Read `migration-docs/migration-tracker.md`.
3. Continue from the first phase whose status is not `Done`.
4. After completing a phase, update `migration-docs/migration-tracker.md` before stopping.

Final handoff docs:

- `migration-docs/api-endpoints.md`: API endpoint reference by feature area.
- `migration-docs/final-migration-checklist.md`: database, configuration, smoke-test, and cutover checklist.

Do not start the next phase until the user has reviewed and pushed the completed phase.
