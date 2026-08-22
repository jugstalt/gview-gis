---
name: third-party-notices
description: Regenerate THIRD-PARTY-NOTICES.txt and THIRD-PARTY-NOTICES-clean.txt at the repo root from the current NuGet dependency graph. Use when the user asks to update/refresh the third-party notices, license notices, or says "THIRD-PARTY-NOTICES aktualisieren" / "third-party notices erneuern".
---

# Third-party notices

Two files at the repo root, both regenerated from scratch each time (don't hand-edit them):

- `THIRD-PARTY-NOTICES.txt` — raw output of the `thirdlicense` dotnet tool, one block per
  package (name + version + repo URL/commit + copyright + license + nuget.org license link).
  Verbose — the same license text repeats once per package that uses it.
- `THIRD-PARTY-NOTICES-clean.txt` — consolidated view built from the raw file by
  `scripts/build_clean.py` in this skill directory: entries with byte-identical license text are
  grouped under one block listing every package (name + version) that shares it. Much shorter,
  easier to read, still complete.

## Steps

1. **Ensure the tool is installed**: `thirdlicense` is a global dotnet tool (NuGet package id
   `thirdlicense`).
   ```bash
   dotnet tool list -g
   ```
   If `thirdlicense` isn't listed, install it:
   ```bash
   dotnet tool install -g thirdlicense
   ```

2. **Run it against the main solution** — `gView-gis.sln`, not `gView.AspireHosting.sln` (that
   one only covers the Aspire orchestration host, see [[testing]]-style solution conventions).
   Takes roughly 2 minutes; it resolves the full transitive NuGet graph across every project in
   the solution.
   ```bash
   thirdlicense --project gView-gis.sln --output THIRD-PARTY-NOTICES.txt
   ```
   Run this from the repo root so the output lands directly at `THIRD-PARTY-NOTICES.txt`. The
   tool writes UTF-8 with BOM, CRLF line endings — leave the encoding as-is, don't re-save it
   through a tool that might mangle non-ASCII copyright characters (`©`, accented author names
   like "Wiesław Šoltés") into `?`/mojibake. That happened once before — that's exactly what
   `THIRD-PARTY-NOTICES-clean.txt` regeneration below now fixes for good.

3. **Build the consolidated variant** from the freshly generated raw file:
   ```bash
   python .claude/skills/third-party-notices/scripts/build_clean.py THIRD-PARTY-NOTICES.txt THIRD-PARTY-NOTICES-clean.txt
   ```
   This writes proper UTF-8 (no BOM needed) and prints entry/group counts to stderr — sanity
   check those numbers look reasonable (a couple hundred package entries, consolidating down to
   roughly a third to a half as many unique license texts).

4. **Sanity-check the diff** before handing back to the user: compare the new package list
   against the previous commit's version to spot anything unexpected (a package that vanished
   entirely usually means a solution/project didn't restore correctly, not that the dependency
   was actually removed).
   ```bash
   git diff --stat THIRD-PARTY-NOTICES.txt THIRD-PARTY-NOTICES-clean.txt
   ```

5. **Don't commit on your own initiative** — leave both files modified/staged for the user to
   review and commit themselves, unless they explicitly ask you to commit.

## Notes

- If `thirdlicense --project` is pointed at a `.csproj` instead of the `.sln`, it only covers
  that one project's dependency graph — always use the solution file for a project-wide notices
  file.
- The raw file's package count (≈300, growing slowly release over release) roughly tracks the
  number of distinct `PackageReference` entries across every project after transitive resolution;
  a sudden large drop is a red flag, not a cleanup win.
