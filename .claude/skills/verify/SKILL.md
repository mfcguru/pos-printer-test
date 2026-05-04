---
name: verify
description: Build the solution, run unit tests, and run Playwright E2E tests. Use after any implementation work before reporting done.
---

Run the full verification suite in this exact order. Do not skip any step. Do not report success until all steps pass.

## Step 1 — Stop the running app

```powershell
Get-Process -Name "POSPrinterTest*","dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
```

## Step 2 — Build

```powershell
dotnet build C:\Dev\Training\POSPrinterTest
```

Must complete with 0 errors. If it fails, stop and report the build errors.

## Step 3 — Unit tests

```powershell
dotnet test C:\Dev\Training\POSPrinterTest\tests\POSPrinterTest.Tests --no-build
```

All tests must pass (0 failed). If any fail, stop and report the failures.

## Step 4 — Playwright E2E tests

Start the app, wait for it to be ready, run E2E tests, then stop the app.

```powershell
# Start app in background
$app = Start-Process dotnet -ArgumentList "run --project C:\Dev\Training\POSPrinterTest\src\POSPrinterTest.Web --no-build" -PassThru

# Poll until ready (up to 30s)
$ready = $false
for ($i = 0; $i -lt 15; $i++) {
    Start-Sleep -Seconds 2
    try { Invoke-WebRequest http://localhost:5050 -UseBasicParsing -TimeoutSec 2 | Out-Null; $ready = $true; break } catch {}
}
if (-not $ready) { Write-Error "App did not start in time" }

# Run E2E tests
dotnet test C:\Dev\Training\POSPrinterTest\tests\POSPrinterTest.E2ETests --no-build

# Stop app
Get-Process -Name "POSPrinterTest*","dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
```

All E2E tests must pass. If any fail, report the failures.

## Step 5 — Report

Summarize: build result, unit test count, E2E test count. Only say "all clear" if everything passed.
