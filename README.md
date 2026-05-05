<div align="center">

<img src="logo.png" alt="WpfBuddy MCP" width="200" />

# WpfBuddy MCP Server

**The MCP server that understands WPF applications — not just pixels.**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MCP](https://img.shields.io/badge/MCP-0.2.0--preview.1-blue)](https://modelcontextprotocol.io/)
[![FlaUI](https://img.shields.io/badge/FlaUI-4.0.0-green)](https://github.com/FlaUI/FlaUI)
[![License](https://img.shields.io/badge/license-MIT-yellow)](LICENSE)
[![Tools](https://img.shields.io/badge/tools-200+-orange)](#tool-categories)

---

*Attach to any WPF app. Inspect its UI tree semantically. Automate workflows without coordinates. Generate tests. Diagnose binding errors. All through natural language via MCP.*

</div>

---

## Why This Exists

Generic Windows automation tools can click and type. **This server understands WPF.**

| Generic Automation MCP | WpfBuddy MCP |
|---|---|
| "Click at position (340, 220)" | "Click the Save button" |
| "Read pixels from screen" | "Get the UI tree with AutomationIds" |
| "Something failed" | "The Save button is disabled because `SaveCommand.CanExecute` returns false — the `Name` property is empty" |
| "Here's a screenshot" | "Here's the element tree, its bindings, validation state, and DataContext" |

### Core Differentiator

> Given any WPF screen, inspect it semantically, automate a workflow without raw coordinates, explain WPF-specific failures, record the workflow, replay it deterministically, and export a maintainable automated test.

---

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11 (UI Automation is Windows-only)

### Build & Run

```powershell
dotnet build WpfBuddyMcp.sln
dotnet run --project src/WpfBuddy.Mcp.Server
```

### Connect Your MCP Client

<details>
<summary><b>VS Code (GitHub Copilot)</b></summary>

Add to `.vscode/mcp.json`:
```json
{
  "mcpServers": {
    "wpfbuddy-mcp": {
      "command": "dotnet",
      "args": ["run", "--project", "src/WpfBuddy.Mcp.Server/WpfBuddy.Mcp.Server.csproj"]
    }
  }
}
```
</details>

<details>
<summary><b>Claude Code</b></summary>

Add to `claude_code_config.json`:
```json
{
  "mcpServers": {
    "wpfbuddy-mcp": {
      "command": "dotnet",
      "args": ["run", "--project", "C:/path/to/src/WpfBuddy.Mcp.Server/WpfBuddy.Mcp.Server.csproj"]
    }
  }
}
```
</details>

---

## What Can It Do?

### Attach & Inspect

```
"Attach to WPFapp and show me the main window structure"
```

The server attaches to any running WPF process, captures the full automation tree, and returns a semantic representation — control types, AutomationIds, names, patterns, states.

### Automate Without Coordinates

```
"Click 'New Project', fill in Name as 'Test Project', then click Save"
```

All actions use semantic selectors (AutomationId → Name → ControlType), never raw screen coordinates. Selectors self-heal when the UI changes.

### Record & Generate Tests

```
"Record what I'm doing, then export it as an xUnit test"
```

Records UI interactions as a workflow, validates selector stability, and exports production-ready `xUnit + FlaUI` test code with Page Object classes.

### Diagnose WPF Issues

```
"Why is the Submit button disabled?"
```

With the optional in-process probe, inspects ViewModel command state, binding errors, DataContext properties, and validation — answering the "why" that generic tools cannot.

### Accessibility Audit

```
"Run an accessibility check on this window"
```

Checks for missing names, keyboard accessibility, tab order, control patterns, and generates a report with recommendations.

---

## Tool Categories

| Category | Tools | Description |
|----------|:-----:|-------------|
| **Session** | 15 | Attach, launch, detach, window management |
| **Snapshot** | 15 | UI tree capture, element queries, diff |
| **Action** | 27 | Click, type, select, drag, context menu, slider |
| **Wait** | 15 | Wait for state, visibility, value, dialog, navigation |
| **Assertion** | 16 | Assert state, text, grid, accessibility, snapshot |
| **Selector** | 11 | Build, validate, rank, heal, detect brittle |
| **DataGrid & Tree** | 16 | Grid rows/columns/cells, tree expand/select |
| **Recording** | 16 | Record, replay, pause, optimize, explain failures |
| **Test Generation** | 8 | Page objects, smoke tests, accessibility tests |
| **Screenshot** | 8 | Capture, annotate, compare, highlight |
| **Accessibility** | 9 | Audit names, tab order, keyboard, patterns |
| **Policy & Safety** | 10 | Capabilities, dry-run, redaction, audit |
| **Reporting** | 6 | Testability scores, diagnostics, export |
| **Clipboard & Env** | 7 | Clipboard, culture, theme, screen info |
| **Probe (MVVM)** | 7 | Connect/status/health for in-process probe |
| **MVVM Diagnostics** | 11 | ViewModel, bindings, commands, validation |
| **Diagnostics** | 5 | Environment, runtime, and system diagnostics |
| | **202** | **Total tools** |

> Full tool reference: [docs/tools-reference.md](docs/tools-reference.md)

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│  MCP Client (VS Code Copilot, Claude Code, etc.)        │
└─────────────────────┬───────────────────────────────────┘
                      │ stdio (JSON-RPC)
┌─────────────────────▼───────────────────────────────────┐
│  WpfBuddy.Mcp.Server  (.NET 8)                          │
│                                                         │
│  ┌───────────────┐  ┌────────────┐  ┌─────────────────┐ │
│  │ SessionManager│  │ UiaAdapter │  │ SelectorBuilder │ |
│  │(attach/detach)│  │(FlaUI UIA3)│  │(heal/rank/build)│ |
│  └───────────────┘  └────────────┘  └─────────────────┘ │
│  ┌─────────────┐  ┌────────────┐  ┌─────────────────┐   │
│  │ Recording   │  │ Screenshot │  │   AuditLog      │   │
│  │  Service    │  │  Service   │  │                 │   │
│  └─────────────┘  └────────────┘  └─────────────────┘   │
│  ┌─────────────────────────────────────┐                │
│  │         ProbeClient (IPC)           │                │
│  └───────────────┬─────────────────────┘                │
└──────────────────┼──────────────────────────────────────┘
                   │ Named Pipe
┌──────────────────▼──────────────────────────────────────┐
│  Target WPF Application                                 │
│  ┌─────────────────────────────────────┐                │
│  │  WpfBuddy.Mcp.Probe (NuGet)         │                │
│  │  • ViewModel inspection             │                │
│  │  • Binding error capture            │                │
│  │  • Command state                    │                │
│  │  • Validation errors                │                │
│  │  • Dispatcher status                │                │
│  └─────────────────────────────────────┘                │
└─────────────────────────────────────────────────────────┘
```

---

## Project Structure

```
wpf-buddy-mcp/
├── WpfBuddyMcp.sln
├── src/
│   ├── WpfBuddy.Mcp.Server/       # MCP server (main project)
│   │   ├── Program.cs                # Entry point, DI registration
│   │   ├── Services/                 # Core services
│   │   │   ├── SessionManager.cs     # Process attach/detach
│   │   │   ├── UiaAdapter.cs         # FlaUI wrapper
│   │   │   ├── SelectorBuilder.cs    # Selector generation & healing
│   │   │   ├── RecordingService.cs   # Workflow recording
│   │   │   ├── ScreenshotService.cs  # Screen capture
│   │   │   ├── ProbeClient.cs        # Named pipe IPC client
│   │   │   └── AuditLog.cs           # Action audit trail
│   │   ├── Tools/                    # MCP tool implementations
│   │   │   ├── SessionTools.cs
│   │   │   ├── SnapshotTools.cs
│   │   │   ├── ActionTools.cs
│   │   │   ├── WaitTools.cs
│   │   │   ├── AssertionTools.cs
│   │   │   ├── SelectorTools.cs
│   │   │   ├── DataGridTools.cs
│   │   │   ├── RecordingTools.cs
│   │   │   ├── TestGenerationTools.cs
│   │   │   ├── ScreenshotTools.cs
│   │   │   ├── AccessibilityTools.cs
│   │   │   ├── PolicyTools.cs
│   │   │   ├── ReportingTools.cs
│   │   │   ├── ClipboardTools.cs
│   │   │   ├── DiagnosticsTools.cs
│   │   │   ├── ProbeTools.cs
│   │   │   └── MvvmTools.cs
│   │   └── Models/                   # Data models
│   └── WpfBuddy.Mcp.Probe/        # In-process probe (NuGet package)
│       ├── ProbeHost.cs              # Named pipe server + WPF inspection
│       └── WpfBuddy.Mcp.Probe.csproj
└── docs/                             # Documentation
```

---

## Safety & Security

| Principle | Implementation |
|-----------|---------------|
| **Scoped execution** | Only operates on the attached application — no OS-wide access |
| **Audit trail** | Every mutating action is logged with timestamp, tool name, and parameters |
| **Dry-run mode** | `wpf_preview_action` shows what would happen without executing |
| **Redaction** | Configurable rules to mask sensitive field values in snapshots |
| **Policy engine** | Control destructive actions, coordinate fallback, timeouts, retries |
| **No shell access** | No file system, registry, or process management tools |

---

## Optional: In-Process Probe

For deep WPF diagnostics (bindings, ViewModel, commands), install the probe NuGet in your target app:

```powershell
dotnet add package WpfBuddy.Mcp.Probe
```

```csharp
// In App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    ProbeHost.Start();  // Listens on named pipe automatically
}
```

Then from MCP: `wpf_probe_connect` → full MVVM diagnostics available.

> Probe setup guide: [docs/probe-setup.md](docs/probe-setup.md)

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Runtime | .NET 8 (`net8.0-windows`) |
| MCP SDK | [ModelContextProtocol 0.2.0-preview.1](https://www.nuget.org/packages/ModelContextProtocol) |
| UI Automation | [FlaUI.UIA3 4.0.0](https://github.com/FlaUI/FlaUI) |
| Transport | stdio (JSON-RPC) |
| DI/Hosting | Microsoft.Extensions.Hosting 8.0.1 |
| Serialization | System.Text.Json 8.0.5 |
| IPC | Named Pipes (System.IO.Pipes) |

---

## Documentation

| Document | Description |
|----------|-------------|
| [docs/tools-reference.md](docs/tools-reference.md) | Complete tool reference with parameters and examples |
| [docs/probe-setup.md](docs/probe-setup.md) | In-process probe installation and usage guide |
| [docs/architecture.md](docs/architecture.md) | Detailed architecture and design decisions |
| [docs/examples.md](docs/examples.md) | Real-world usage examples and workflows |

---

## Contributing

1. Fork & clone
2. `dotnet build WpfBuddyMcp.sln`
3. Make changes
4. Ensure build passes: `dotnet build`
5. Submit PR

---

## License

MIT
