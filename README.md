# POS Printer Test

A .NET 8 Razor Pages application for testing POS printer output.

## Features

- **Test Print page** — Select a printer from a dropdown, paste content into a text area, and click "Test Print" to send the job to the printer.
- **Printer Management page** — Add, edit, and delete printers.

## Tech Stack

- .NET 8, ASP.NET Core Razor Pages
- Entity Framework Core with SQLite
- xUnit for unit tests
- Playwright for e2e tests

## Getting Started

```bash
cd src/POSPrinterTest.Web
dotnet run
```

## Running Tests

```bash
dotnet test
```

## Development Workflow

This project uses an AI-assisted agentic workflow. See [CLAUDE.md](CLAUDE.md) for the full process.
