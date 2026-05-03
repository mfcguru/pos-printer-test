# Issue #3: Adopt Vertical Slice Architecture

## Overview
Restructure the project so code is organized by feature slice rather than technical layer. Every
file related to a single feature (page, handler, models, queries/commands) lives in one folder
under `Features/`. Shared infrastructure (DbContext, DI registration) stays in `Data/` and
`Program.cs`. This issue is purely structural — no new business logic.

## Acceptance Criteria
- [ ] CLAUDE.md documents the VSA folder structure and rules
- [ ] `dotnet-agent.md` updated with exact folder structure and naming conventions
- [ ] `spec-agent.md` updated so it describes pages and handlers in VSA terms
- [ ] Existing scaffolded pages (Index, Privacy, Error) moved into the correct `Features/` subfolders
- [ ] `_Layout.cshtml`, `_ViewImports.cshtml`, `_ViewStart.cshtml` remain in `Pages/Shared/` and `Pages/`
- [ ] `dotnet build` passes after reorganisation

## Target Folder Structure

```
src/POSPrinterTest.Web/
  Features/
    Home/
      Index.cshtml
      Index.cshtml.cs
    Privacy/
      Privacy.cshtml
      Privacy.cshtml.cs
    Error/
      Error.cshtml
      Error.cshtml.cs
    Printers/                   ← added by Issue #1
      List/
      Create/
      Edit/
      Delete/
    TestPrint/                  ← added by Issue #2
      Index.cshtml
      Index.cshtml.cs
      TestPrintCommand.cs
  Data/
    AppDbContext.cs             ← added by Issue #1
  Pages/
    Shared/
      _Layout.cshtml
      _Layout.cshtml.css
      _ValidationScriptsPartial.cshtml
    _ViewImports.cshtml
    _ViewStart.cshtml
  Program.cs
```

## Pages & UI
No new pages. Existing pages are relocated only.

| Current path | New path |
|---|---|
| `Pages/Index.cshtml[.cs]` | `Features/Home/Index.cshtml[.cs]` |
| `Pages/Privacy.cshtml[.cs]` | `Features/Privacy/Privacy.cshtml[.cs]` |
| `Pages/Error.cshtml[.cs]` | `Features/Error/Error.cshtml[.cs]` |

Razor routing is path-based — after moving files the `@page` directive and `asp-page` references in
`_Layout.cshtml` must be verified to still resolve correctly (Razor Pages supports files under any
subfolder of the root `Pages/` dir). Because we are placing slices under `Features/` (not `Pages/`),
the `RazorPagesOptions.RootDirectory` must be set to `"/"` in `Program.cs` so the framework scans
both `Pages/` and `Features/`.

## Program.cs Changes
```csharp
builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/";
});
```

## Agent File Updates

### CLAUDE.md
Add a **Folder Conventions** section under **Tech Stack**:
```
### Folder Conventions (Vertical Slice Architecture)
- All feature pages live under `src/POSPrinterTest.Web/Features/<FeatureName>/<SliceName>/`
- Shared infrastructure (DbContext, global layout) stays in `Data/` and `Pages/Shared/`
- Each slice folder contains: the `.cshtml` view, the `.cshtml.cs` page model, and any
  slice-local command/query classes
- No cross-slice dependencies — slices communicate only through shared services in `Data/`
```

### dotnet-agent.md
Add:
- Always create new feature pages under `Features/<FeatureName>/<SliceName>/`
- Command/query classes are named `<SliceName><FeatureName>Command.cs` or `<SliceName><FeatureName>Query.cs`
- Never create pages under `Pages/` (except Shared partials and view imports)
- Update `RazorPagesOptions.RootDirectory = "/"` is already set; do not remove it

### spec-agent.md
Add:
- Always describe pages in VSA terms: `Features/<Feature>/<Slice>/Index.cshtml`
- Route section should map URL → slice folder
- Specs must list any command/query classes needed in each slice

## Tests
No new tests. Confirm `dotnet build` passes — that is sufficient validation for a structural refactor.

## Out of Scope
- No business logic changes
- No new pages or features
- No CSS/JS changes
