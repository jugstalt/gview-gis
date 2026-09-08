---
name: commit
description: How to handle commit requests in this repo (gview-gis) — never commit automatically, produce a single short commit message the user commits themselves, and only run the actual commit when explicitly told to ("commit yourself" / "commit selber" / "du committest"). Use whenever the user asks for a commit, a commit message, or asks you to commit changes.
---

# Committing

## Never commit on your own initiative

Do **not** run `git commit` (or `git add` + `git commit`) as a side effect of finishing a
task, "wrapping up", or tidying the working tree. Leave changes staged/unstaged for the user
to review. The only time you run a commit is when the user explicitly asks you to — see
"Running the commit yourself" below.

## Default behaviour: just the message

When the user asks to commit or asks for a commit message (e.g. "commit", "commit message
bitte", "wie würdest du das commiten?"), the deliverable is **one commit message and nothing
else**:

- Output only the message text — no `git` commands, no "run this", no explanation of what
  changed unless the user asked for that separately.
- Keep it **short and to the point**: a single sentence describing the change.
- Optionally, only when a single sentence genuinely can't carry it, add a short bullet list of
  keyword-style points underneath. Don't pad it out — if one line says it, use one line.
- Match this repo's existing commit style (`git log --oneline`): English, a component/area
  prefix where it orients the reader, then a concise phrase.
  Examples from history:
  - `SpatiaLite: fix "method not implemented" when pasting into an existing file`
  - `FDB: drop the WKB storage type, add IsDatabaseNative() + SpatiaLite/GeoPackage slots`
  - `Query returnCountOnly: never cap the count at MaxRecordCount`
- Present it in a plain fenced code block so the user can copy it straight into their own
  `git commit`.

The normal flow is: you hand over the message, **the user commits it themselves**.

## Running the commit yourself

Only when the user explicitly asks you to actually perform the commit — phrasings like
"commit yourself", "commit selber", "du committest", "mach den commit", "commit it" — do you
run it:

1. Check `git status` / `git diff` to see what will be included.
2. If nothing is staged, `git add` the relevant files (the ones related to the change you're
   describing — don't blindly `git add -A` if unrelated files are lying around; ask if
   unsure).
3. Commit with the same kind of short message described above, ending with the trailer:

   ```
   Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
   ```
4. Do **not** push, and do **not** amend existing commits, unless the user asks for that too.

If the current branch is the default branch (`main`), create a branch first rather than
committing directly onto it.
