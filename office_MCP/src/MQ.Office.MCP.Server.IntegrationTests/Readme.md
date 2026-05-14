# MQ.Office.MCP.Server.IntegrationTests

End-to-end tests that launch the MCP server as a child process and exercise
it through stdio. Microsoft Word and Excel must be installed on the host for
the tool calls to succeed; the smoke test only verifies the process starts.

Run with:

```powershell
.\ci-scripts\integration_tests.bat
```
