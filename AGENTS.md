# Codex Project Instructions

Before starting any work in this repository, every Codex session must read:

- `Assets/Docs/Codex_Project_Brief.md`

Treat that file as the shared project brief for all future work in this Unity project.
When creating commits or pushing work, follow `Assets/Docs/Commit_Message_Guide.md`.

## Required Startup Routine

1. Read `Assets/Docs/Codex_Project_Brief.md`.
2. Identify which system, workflow, or development principle from the brief applies to the user's current request.
3. Inspect the relevant Unity scene, `Packages/manifest.json`, and `Assets` structure when needed.
4. Before committing or pushing, read `Assets/Docs/Commit_Message_Guide.md` and write the commit message from that guide.
5. If the requested work conflicts with the brief, briefly tell the user what conflicts before making changes.

## Project Defaults

- This is a 2D top-view roguelike Unity project.
- Build features in small playable increments, then integrate them.
- Do not rename existing scenes, files, or Unity settings without a clear reason.
- Do not treat `Library`, `Temp`, `Logs`, or `UserSettings` as normal work targets.
- When a new shared project assumption or development rule appears, update `Assets/Docs/Codex_Project_Brief.md` as part of the work.
