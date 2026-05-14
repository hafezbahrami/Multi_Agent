# MQ.Office.MCP.Server.Tests

Unit tests for the MQ Office MCP Server. Uses xUnit + Coverlet. Tests use a
fake COM bootstrap (ExpandoObject) so Word/Excel do not need to be installed
to run them.

Run from the repo root:

```powershell
.\ci-scripts\test.bat
```
