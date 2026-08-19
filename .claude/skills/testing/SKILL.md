---
name: testing
description: How automated tests are organized and written in this repo (gview-gis) — where xUnit test projects live, how they're named, how they get wired into the solution, and how to test internal types. Use whenever creating a new test project, adding test cases for a project, or running the existing test suite.
---

# Testing conventions

This repo's test projects follow one fixed layout. Don't improvise a different one (e.g. don't
put tests next to the source project, don't use MSTest/NUnit, don't skip the solution wiring).

## Layout & naming

- Test projects live under `src/tests/`, one folder per project under test, mirroring how every
  other project group in this repo is laid out (`src/<Group>/<ProjectName>/<ProjectName>.csproj`):
  ```
  src/tests/<ProjectName>.Tests/<ProjectName>.Tests.csproj
  ```
  Example: tests for `src/gView.Cmd.MxlUtil.Lib/gView.Cmd.MxlUtil.Lib.csproj` go in
  `src/tests/gView.Cmd.MxlUtil.Lib.Tests/gView.Cmd.MxlUtil.Lib.Tests.csproj`.
- One test project per project under test — don't fold multiple source projects' tests into one
  test project.
- Test class names mirror the type under test: `FooTests` for `Foo`. One test class per type
  under test unless a type is trivial enough not to need its own.
- Test method names describe scenario and expectation, e.g.
  `TryConvert_SingleIfWithoutElse_TreatsAsConditional`. Prefer this over generic names like
  `Test1`.

## Test framework: xUnit

Standard package set (match the version already used elsewhere in the repo — check an existing
test project first if one exists; otherwise use current stable versions):

- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`

Use plain xUnit `Assert.*` for assertions unless the user asks for something else (e.g.
FluentAssertions) — don't add extra assertion libraries speculatively.

- `[Fact]` for a single fixed-input test.
- `[Theory]` + `[InlineData(...)]` for the same test logic run over several inputs — prefer this
  over copy-pasted near-identical `[Fact]` methods.
- Structure each test body as Arrange / Act / Assert (blank line between sections is enough,
  comments optional).

## Project file template

Match the target framework and nullable/implicit-usings settings of the project under test
(check its `.csproj` — as of this writing that's `net10.0`, `<Nullable>enable</Nullable>`,
`<ImplicitUsings>enable</ImplicitUsings>` across this repo, but verify per-project rather than
assuming). Minimal shape:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="..." />
    <PackageReference Include="xunit" Version="..." />
    <PackageReference Include="xunit.runner.visualstudio" Version="..." />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\<ProjectName>\<ProjectName>.csproj" />
  </ItemGroup>

</Project>
```

## Testing `internal` types

Several projects (e.g. `gView.Cmd.MxlUtil.Lib`) keep implementation classes `internal` (see
`AprxLabelExpressionParser`, `AprxMapConverter`). To unit-test those directly instead of only
through reflection or the public surface, add an `InternalsVisibleTo` attribute to the project
under test, granting the matching `.Tests` assembly access:

```csharp
// in the project under test, e.g. AssemblyInfo.cs or any existing source file
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("gView.Cmd.MxlUtil.Lib.Tests")]
```

Add this to the **project under test**, not the test project. Don't reach for reflection-based
test hacks when `InternalsVisibleTo` is available — it's simpler and refactor-safe.

## Wiring into the solution

Test projects go in a solution folder named `tests` (a solution folder, not a filesystem folder —
the projects physically live under `src/tests/` as above, but appear grouped under `tests` in the
IDE's Solution Explorer). Add the project to the repo's main solution, `gView-gis.sln` (not
`gView.AspireHosting.sln` — that's for the Aspire orchestration host only), like this:

```bash
dotnet sln gView-gis.sln add src/tests/<ProjectName>.Tests/<ProjectName>.Tests.csproj --solution-folder tests
```

Running this repeatedly (e.g. once per new test project) reuses the same `tests` solution folder
instead of creating duplicates. Verify afterwards with:

```bash
dotnet sln gView-gis.sln list
```

## Running tests

```bash
dotnet test src/tests/<ProjectName>.Tests/<ProjectName>.Tests.csproj
```

Or, to run every test project in the solution:

```bash
dotnet test gView-gis.sln
```

## Checklist for adding a new test project

1. Create `src/tests/<ProjectName>.Tests/<ProjectName>.Tests.csproj` with the template above,
   TFM matching the project under test.
2. Add a `ProjectReference` to the project under test.
3. If the code under test uses `internal` types/members, add `InternalsVisibleTo` to the project
   under test for `<ProjectName>.Tests`.
4. Write test classes/methods per the naming conventions above.
5. `dotnet sln gView-gis.sln add ... --solution-folder tests`.
6. `dotnet build` the new project, then `dotnet test` it, to confirm it's wired up correctly
   before writing real assertions.
