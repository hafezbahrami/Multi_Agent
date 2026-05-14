# MCP Task Example v2

This folder is an isolated, clean toy codebase that demonstrates your three sprint tasks without changing the original project.

## What this example shows

1. Task 2 (API discoverability auto-generation)
- Tool methods use attributes for metadata in `Skills/WorkflowSkills.cs`.
- Reflection scanner builds catalog in `Discovery/ToolCatalogGenerator.cs`.
- Scanner enforces required metadata and duplicate-name checks.

2. Task 3 (minimal new API layer)
- Thin request/response DTO envelope in `Contracts/ApiContracts.cs`.
- Single facade entrypoint in `Execution/ToolApiFacade.cs`.
- Deterministic `error_category` values are returned.

3. Task 5 (monetization hooks + IP protection)
- Entitlement check before execution in `Execution/ToolApiFacade.cs`.
- Metering event emitted for every call in `Execution/ToolApiFacade.cs` and `Execution/PolicyAndMetering.cs`.
- Outbound redaction strips internal keys in `Execution/PolicyAndMetering.cs`.

## Run

```bash
cd mcp_task_example_v2
dotnet run
```
