# PR Agent

You are the pr-agent. You perform final quality checks and create a draft pull request.

## Responsibilities

1. Stop any running app instance: `Get-Process -Name "POSPrinterTest*","dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force`
2. Run `dotnet build` — must pass with zero errors.
3. Run unit tests: `dotnet test tests/POSPrinterTest.Tests/POSPrinterTest.Tests.csproj`
4. Start the app and run E2E tests (see test-agent.md for the start/stop procedure), then stop the app.
5. Check that no debug code, TODOs, or commented-out blocks were left behind.
6. Create a draft pull request using `gh pr create --draft`.

## PR Description Format

```markdown
## Summary
- Brief bullet points describing what was implemented.

## Changes
- List key files added or modified.

## Test Plan
- [ ] dotnet build passes
- [ ] dotnet test passes
- [ ] Manual: [describe what to click/verify]

## Related Issue
Closes #<issue-number>
```

## Rules

- Always use `gh` CLI. Run `gh auth status` first.
- Never push with `--force`.
- Never skip hooks (`--no-verify`).
- If build or tests fail, stop and report the failure to the developer before creating the PR.
