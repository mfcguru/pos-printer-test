# POSPrinterTest

A .NET 8 Razor Pages application for testing POS printer output. Users can select a printer from a
dropdown, paste content into a text area, and trigger a test print. A separate management page
allows adding, editing, and deleting printers.

## Agents

- `issue-agent`. Uses the `gh` CLI — and only the `gh` CLI — to interact with GitHub issues and pull
  requests. Never use Python, `curl`, or direct `api.github.com` REST calls. If `gh` returns an error,
  surface the exact error message to the developer and stop.
- `spec-agent`. Creates detailed specifications for work to be done, stored as markdown files in `.claude/specs/`
  at the repo root (e.g., `.claude/specs/issue-42-short-title.md`). Specs are NEVER stored in a top-level
  `Specs/` folder.
- `dotnet-agent`. Develops features using C#, .NET 8, ASP.NET Core Razor Pages, EF Core, and related technologies.
- `test-agent`. Writes unit tests (xUnit) and end-to-end Playwright tests to verify that the
  implementation matches the spec.
- `pr-agent`. Performs final quality checks and creates draft pull requests using repo standards.

## Tech Stack

- **Framework**: .NET 8, ASP.NET Core Razor Pages
- **Database**: Microsoft SQL Server (MSSQL) via Entity Framework Core
- **Testing**: xUnit (unit), Playwright (e2e)
- **CI**: GitHub Actions

### Folder Conventions (Vertical Slice Architecture)
- All feature pages live under `src/POSPrinterTest.Web/Features/<FeatureName>/<SliceName>/`
- Shared infrastructure (DbContext, global layout) stays in `Data/` and `Pages/Shared/`
- Each slice folder contains: the `.cshtml` view, the `.cshtml.cs` page model, and any slice-local command/query classes
- No cross-slice dependencies — slices communicate only through shared services in `Data/`

## Workflow

Each feature added to the application should follow this workflow:

1. Use the `issue-agent` to read a given backlog item, break it into subtasks, update the GitHub issue with a checklist.
2. Use the `spec-agent` to review the GitHub issue and generate a specification that will serve as the detailed plan for the feature's implementation.
3. Developers review and iterate on the feature specification.
4. Developers initiate implementation of the spec.
5. Use the `dotnet-agent` for all server-side and Razor Pages behavior.
6. Use the `test-agent` to create and run xUnit unit tests — all must pass before proceeding.
7. Use the `test-agent` to create and run Playwright e2e tests for at least the "happy path" — the agent starts the app, runs the tests, and stops the app. All must pass before proceeding.
8. Request initial Developer review.
9. Use the `pr-agent` to run final local build/test checks and create a DRAFT pull request using the repo's PR template.
10. Developers review pull request and mark it ready for review.

## Rules

These rules apply to all agents in this repository. Violating them is never acceptable even if the
agent believes it has a good reason to work around them.

### GitHub Access

- **Always use `gh` CLI.** All interaction with GitHub — reading issues, updating issues, creating or
  reading pull requests — must go through the `gh` CLI. Never use Python scripts, `curl`,
  `api.github.com` REST calls, or any other mechanism.
- **Never work around `gh` failures.** If any `gh` command exits with a non-zero status or prints an
  error, the agent must:
  1. Capture and display the full error output to the developer.
  2. Stop execution immediately.
  3. Ask the developer to resolve the issue (e.g., run `gh auth login`, check network access) before
     retrying.
- **Proactively check authentication.** Before running any `gh` command that reads or writes GitHub
  data, run `gh auth status`. If the output indicates the user is not authenticated, stop and display
  the following message to the developer:

  > `gh` is not authenticated. Run `gh auth login` to authenticate, then retry.

  Do not attempt to authenticate on behalf of the developer or pass tokens via environment variables
  as a workaround.

### Build & Run

- Before running `dotnet build`, always stop any running instance of the app — the `.exe` is locked while the process is alive and the build will fail with MSB3027.
- To stop the app: `Get-Process -Name "POSPrinterTest*","dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force`

### Code Style

- Use C# 12 features where appropriate.
- Razor Pages only — no MVC controllers, no Blazor, no minimal API endpoints (except health checks).
- EF Core with Microsoft SQL Server for persistence. Always use migrations.
- Keep page models thin — business logic belongs in services registered via DI.
