---
name: changelog
description: Update CHANGELOG.md for gView GIS, and bump the release version in SystemInfo.cs when starting a new release. Use when the user says "changelog update" (add entries under the current version) or "changelog update für einen neuen Release" / "changelog update for a new release" (bump the version first, then update the changelog).
---

# Changelog & version conventions

Two files, always kept in sync:

- `src/gView.Framework/Common/SystemInfo.cs` — `public static Version Version = new Version(8, YY, WWSS);`
  This is the *current* version of the codebase.
- `CHANGELOG.md` — [Keep a Changelog](http://keepachangelog.com/) format. The **top-most**
  `## {version}` heading must always match `SystemInfo.Version` exactly. There is no separate
  "Unreleased" section — the current version itself acts as the in-progress section until it's
  actually released (i.e. until the next version bump moves past it).

## Version format

`8.{Jahr - 2000}.{Kalenderwoche}{Sequenz}`

- `8` — major version, fixed.
- `{Jahr - 2000}` — two digits, e.g. 2026 → `26`.
- `{Kalenderwoche}` — ISO-8601 calendar week, zero-padded to 2 digits (week 6 → `06`, week 34 →
  `34`).
- `{Sequenz}` — 2-digit counter starting at `01`, incremented only when more than one release
  happens in the *same* calendar week. Resets to `01` for a new week.

Example progression from this repo's history: `8.26.1701` → `8.26.2401` → `8.26.3101` →
`8.26.3102` (second release in week 31) → `8.26.3302`.

Get today's ISO week-numbering year and week:

```bash
date +"%G %V"
```

Use `%G` (ISO week-numbering year), not `%Y` — it's what actually corresponds to `%V`. Subtract
2000 from it for the version's year component.

### Sequence number

Compare today's `{Jahr}{Kalenderwoche}` to the year+week of the *current* `SystemInfo.Version`:

- Same year+week → this is a second (or later) release the same week → use
  `current sequence + 1`.
- Different year+week (or no prior release info) → first release this week → use `01`.

## "changelog update" (no new release)

1. Read the current version from `SystemInfo.Version`.
2. Check the top-most `## {version}` heading in `CHANGELOG.md`:
   - Matches the current version → add entries into that section.
   - Doesn't match (top heading is an older, already-released version) → insert a **new**
     `## {version}` heading above it for the current version, then add entries there.
3. Figure out what to write: prefer what the user just described in the conversation. If they
   only said "changelog update" with no further detail, check `git log` / `git diff` since the
   last version-bump commit (look for commit subjects matching `Version`/`Release`, e.g.
   `git log --oneline --grep="^Version\|^Release" -i`) and summarize the notable user-facing
   changes since then. Don't invent entries you can't back up from the code/commits — ask the
   user if it's unclear.
4. Match the file's existing style: `## Added` / `## Fixed` / `## Changed` sub-headings (only the
   ones that apply, not all three every time), bullet points, past tense, a component prefix
   where it helps orient the reader (`MxlUtil ConvertAprx: ...`, `PostGIS: ...`,
   `SimpleScriptInterpreter: ...`).

## "changelog update für einen neuen Release"

1. Compute the new version per the format above, from today's date and the sequence logic.
2. Update the `Version = new Version(...)` line in
   `src/gView.Framework/Common/SystemInfo.cs` to the new version.
3. Then continue exactly like a normal changelog update (step 2 onward above), but using the
   *new* version as current — insert its `## {version}` heading in `CHANGELOG.md` and fill in
   entries for what's being released.
4. Don't commit or push on your own initiative — leave the changes staged/unstaged for the user
   to review, unless they explicitly ask you to commit.

## Style reference

Read the existing top entries in `CHANGELOG.md` before writing new ones — sub-bullets for
multi-part changes are fine, keep it factual and specific (name the affected
type/class/command rather than writing just "fixed a bug").
