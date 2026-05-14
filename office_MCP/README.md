# MQ Office MCP Server

An MCP (Model Context Protocol) server that exposes Microsoft Word and
Microsoft Excel as tools you can call from any MCP client — including the
GitHub Copilot Chat in VS Code.

For this first cut, three tools are exposed per app: **open**, **save**, **close**.

## Layout

```
office_MCP/
├── azure-pipelines.yml
├── README.md
├── ci-scripts/
│   ├── build.bat
│   ├── test.bat
│   ├── integration_tests.bat
│   ├── format.bat
│   ├── publish.bat
│   └── normalize-version.ps1
├── src/
│   ├── MQ.Office.MCP.Server.slnx
│   ├── MQ.Office.MCP.Server/                  # Host / entry point
│   ├── MQ.Office.MCP.Server.Word/             # Word module
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   └── Tools/
│   ├── MQ.Office.MCP.Server.Excel/            # Excel module
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   └── Tools/
│   ├── MQ.Office.MCP.Server.Tests/            # xUnit unit tests
│   └── MQ.Office.MCP.Server.IntegrationTests/ # End-to-end stdio tests
└── .vscode/
    └── mcp.json                               # Copilot client wiring
```

## Tools exposed

| Tool                    | Description                              |
|-------------------------|------------------------------------------|
| `word_open_document`    | Open or create a Word document           |
| `word_save_document`    | Save (or Save As) the active document    |
| `word_close_document`   | Close the active document                |
| `excel_open_workbook`   | Open or create an Excel workbook         |
| `excel_save_workbook`   | Save (or Save As) the active workbook    |
| `excel_close_workbook`  | Close the active workbook                |

## Prerequisites

- Windows (the COM ProgIDs `Word.Application` and `Excel.Application` are
  Windows-only)
- Microsoft Word and Microsoft Excel installed
- .NET 10 SDK
- VS Code with GitHub Copilot Chat (to use this server as an agent tool)

## Build & test

```powershell
cd ci-scripts
.\build.bat
.\test.bat
```

## Wiring up Copilot Chat

After copying the `office_MCP` folder to any location, the bundled
`.vscode/mcp.json` tells Copilot how to launch the server. Open the folder in
VS Code and the MCP server appears in Copilot's tools list automatically.

The config uses `${workspaceFolder}` so it works wherever you copy the folder:

```jsonc
{
  "servers": {
    "mq-office": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "${workspaceFolder}/src/MQ.Office.MCP.Server/MQ.Office.MCP.Server.csproj",
        "--no-launch-profile"
      ]
    }
  }
}
```

Then ask Copilot Chat something like:

> Open `C:\Docs\Report.docx` in Word and save it as `Report-backup.docx`.

Copilot will call `word_open_document` followed by `word_save_document`.

## Logs

All server logs go to `%TEMP%\MQOfficeMCP\YYYY-MM-DD.log` — stdout is reserved
for MCP JSON-RPC traffic.
